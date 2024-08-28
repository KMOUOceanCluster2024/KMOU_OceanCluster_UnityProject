using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Chat_Server
{
    public class message
    {
        public PROTOCOL pt_id { get; set; }
        public login_info signup_login_info { get; set; }
        public login_success_info first_login_info { get; set; }
        public InGame_message ingame_info { get; set; }
    }
    public class login_info
    {
        public string Email { get; set; }
        public string PW { get; set; }
        public string Nickname { get; set; }
    }
    public class login_success_info
    {
        public string Nickname { get; set; }
        public int scene_num { get; set; }
        public double x_position { get; set; }
        public double y_position { get; set; }
        public int semester { get; set; }
        public int main_quest_num { get; set; }
        public int detail_quest_num { get; set; }
        public int quest_state { get; set; }
        public List<int> minigame_nums { get; set; }
        public List<double> scores { get; set; }
    }
    public class InGame_message
    {
        public double x_position { get; set; }
        public double y_position { get; set; }
        public int scene_num { get; set; }
        public int room_num { get; set; }
        public string target_nickname { get; set; }
        public string message { get; set; }
        public int semester { get; set; }
        public int main_quest_num { get; set; }
        public int detail_quest_num { get; set; }
        public int quest_state { get; set; }
        public string own_nickname { get; set; }
        public double sub_quest_score { get; set; }
        public int minigame_num { get; set; }
    }
    class NetworkService
    {
        class accept_receive_data
        {
            public PROTOCOL pt_id { get; set; }
            public login_info info { get; set; }
        }
        
        //listen 소켓
        Socket listen_socket;
        SocketAsyncEventArgs accept_event;
        AutoResetEvent flow_control_event;
        public delegate void ClientHandler(Socket client_socket, object token, string client_ID, string client_nickname, int scene);
        public ClientHandler callback_on_newclient;

        public delegate void SessionHandler(Token token);
        public SessionHandler session_created_callback { get; set; }

        public Stack<SocketAsyncEventArgs> receive_event_pool;
        public Stack<SocketAsyncEventArgs> send_event_pool;

        //buffer 설정
        public byte[] m_buffer;
        public int max_connections;
        public int buffer_size;
        public int pre_alloc_count = 1;
        public int connected_count;

        public int m_numBytes;
        public int m_currentIndex = 0;
        public int m_bufferSize;
        public Stack<int> m_freeIndexPool;

        List<Socket> client_sockets;
        List<Token> users;
        //List<login_info> fake_user_db;

        public const string MONGODB_URI_FORMAT = "mongodb+srv://capstone:20211275@cluster0.ynjxsbf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        private const string TEST_DB = "Game_DB";
        private const string User_info_Collection = "User_info";
        private const string User_Character_Collection = "User_Character";
        private const string Minigame_Record_Collection = "Minigame_Record";

        public MongoClient Mongo_Server;
        public IMongoDatabase database;
        public IMongoCollection<BsonDocument> UsersCollection;
        public IMongoCollection<BsonDocument> UserCharacter;
        public IMongoCollection<BsonDocument> MinigameRecord;

        public Chat_Manager chat_manager_cs;
        public Game_Manager game_manager_cs;
        public NetworkService()
        {
            Mongo_Server = new MongoClient(MONGODB_URI_FORMAT);
            database = Mongo_Server.GetDatabase(TEST_DB);

            chat_manager_cs = new Chat_Manager();
            game_manager_cs = new Game_Manager();

            client_sockets = new List<Socket>();
            users = new List<Token>();

            //fake_user_db = new List<login_info>();

            this.connected_count = 0;
            this.session_created_callback = null;

            this.max_connections = 10000;
            this.buffer_size = 1024;

            this.m_numBytes = this.max_connections * this.buffer_size * this.pre_alloc_count;
            this.m_bufferSize = this.buffer_size;
            this.m_buffer = new byte[m_numBytes];
            m_freeIndexPool = new Stack<int>();

            this.callback_on_newclient = null;

            this.receive_event_pool = new Stack<SocketAsyncEventArgs>(max_connections);
            this.send_event_pool = new Stack<SocketAsyncEventArgs>(max_connections);

            SocketAsyncEventArgs arg;
            for (int i = 0; i < max_connections; i++)
            {
                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    arg.UserToken = null;
                    SetBuffer(arg);
                    this.receive_event_pool.Push(arg);
                }

                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
                    arg.UserToken = null;
                    arg.SetBuffer(null, 0, 0);
                    this.send_event_pool.Push(arg);
                }
            }
        }
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }
        void receive_completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                chat_client_receive_completed(null, e);
                return;
            }
            throw new ArgumentException("The last operation completed on the socket was not a receive");
        }
        void send_completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Token token = e.UserToken as Token;
            }
            catch (Exception)
            {

            }
        }

        //서버 시작
        public void start(string host, int port, int backlog)
        {
            this.listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine(this.listen_socket);
            IPAddress address;
            if (host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);
            }
            IPEndPoint endpoint = new IPEndPoint(address, port);
            this.callback_on_newclient += on_new_client;
            this.session_created_callback += add_users;
            UsersCollection = database.GetCollection<BsonDocument>(User_info_Collection);
            UserCharacter = database.GetCollection<BsonDocument>(User_Character_Collection);
            MinigameRecord = database.GetCollection<BsonDocument>(Minigame_Record_Collection);
            chat_manager_cs.start_chatmanager(this);
            game_manager_cs.start_gamemanager(this);
            try
            {
                listen_socket.Bind(endpoint);
                listen_socket.Listen(backlog);
                this.accept_event = new SocketAsyncEventArgs();
                this.accept_event.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

                Thread listen_thread = new Thread(do_listen);
                listen_thread.Start();
            }
            catch (Exception e)
            {

            }
        }
        public void do_listen()
        {
            Console.WriteLine("Start listen");
            this.flow_control_event = new AutoResetEvent(false);
            while (true)
            {
                this.accept_event.AcceptSocket = null;
                bool pending = true;
                try
                {
                    pending = listen_socket.AcceptAsync(this.accept_event);
                }
                catch (Exception e)
                {
                    continue;
                }
                if (!pending)
                {
                    on_accept_completed(null, this.accept_event);
                }

                this.flow_control_event.WaitOne();
            }
        }
        void on_accept_completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket client_socket = e.AcceptSocket;
                this.client_sockets.Add(client_socket);
                Console.WriteLine("Client connected >> " + client_sockets.Count);

                // 클라이언트와의 비동기 통신 시작
                StartReceive(client_socket);
            }
            else
            {
                // 에러 처리
            }
            this.flow_control_event.Set();
            return;
        }
        void StartReceive(Socket clientSocket)
        {
            try
            {
                // 비동기 수신 작업 설정
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                byte[] buffer = new byte[1024];
                receiveArgs.SetBuffer(buffer, 0, buffer.Length);
                receiveArgs.UserToken = clientSocket;
                receiveArgs.Completed += ReceiveCompleted;

                // 비동기 수신 시작
                bool willRaiseEvent = clientSocket.ReceiveAsync(receiveArgs);
                if (!willRaiseEvent)
                {
                    // 동기적으로 완료됨, 직접 완료 처리
                    ReceiveCompleted(clientSocket, receiveArgs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting receive: " + ex.Message);
                // 에러 처리
            }
        }
        void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)sender;
            try
            {
                bool islogon = false;
                int bytesReceived = e.BytesTransferred;
                if (bytesReceived > 0 && e.SocketError == SocketError.Success)
                {
                    byte[] buffer = e.Buffer;
                    string receivedJson = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    message receivedInfo = JsonConvert.DeserializeObject<message>(receivedJson);
                    Console.WriteLine(receivedJson);

                    PROTOCOL client_select_menu = receivedInfo.pt_id;
                    Console.WriteLine(client_select_menu);

                    PROTOCOL pt_id = PROTOCOL.Setting;
                    string nickname = "";
                    int scene = -1;

                    message signup_login_send = new message();
                    login_success_info signup_login_state = new login_success_info();
                    if(client_select_menu == PROTOCOL.SIGNUP_Request)
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("Email", receivedInfo.signup_login_info.Email);
                        var docs = UsersCollection.Find(filter).ToList();
                        if (docs.Count == 0)
                        {
                            var doc = new BsonDocument { { "Email", receivedInfo.signup_login_info.Email }, { "PW", receivedInfo.signup_login_info.PW }, { "Nickname", receivedInfo.signup_login_info.Nickname }, { "LoginST", 0 } };
                            UsersCollection.InsertOne(doc);

                            var initial_position_doc = new BsonDocument { { "Nickname", receivedInfo.signup_login_info.Nickname }, { "scene_num", 0 }, { "x_position", -2715.0 }, { "y_position", 2612.0 }, {"Semester", 0 }, {"Main_Quest_num", 0 }, {"Detail_Quest_num", 0 }, { "Quest_State", 0 } };
                            UserCharacter.InsertOne(initial_position_doc);
                            Console.WriteLine("Send Signup Access");
                            pt_id = PROTOCOL.SIGNUP_Success;
                        }
                        else
                        {
                            Console.WriteLine("User with the same ID already exists");
                            pt_id = PROTOCOL.SIGNUP_Fail;
                        }
                    }
                    else if(client_select_menu == PROTOCOL.LOGIN_Request)
                    {
                        var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("Email", receivedInfo.signup_login_info.Email), Builders<BsonDocument>.Filter.Eq("PW", receivedInfo.signup_login_info.PW));
                        var docs = UsersCollection.Find(filter).ToList();
                        if (docs.Count == 1)
                        {
                            var firstDoc = docs.First();
                            var loginST = firstDoc["LoginST"].AsInt32;
                            if (loginST == 0)
                            {
                                var updateFilter = Builders<BsonDocument>.Filter.Eq("Email", firstDoc["Email"]); // 특정 문서를 찾기 위한 필터
                                var update = Builders<BsonDocument>.Update.Set("LoginST", 1); // 새로운 LoginST 값 설정
                                var updateResult = UsersCollection.UpdateOne(updateFilter, update);
                                Console.WriteLine("Send Login Access");
                                pt_id = PROTOCOL.LOGIN_Success;

                                string user_nickname = firstDoc["Nickname"].AsString;
                                nickname = user_nickname;

                                var position_filter = Builders<BsonDocument>.Filter.Eq("Nickname", user_nickname);
                                var user_position_info = UserCharacter.Find(position_filter).ToList();
                                var position_info_doc = user_position_info.First();
                                int last_scene = position_info_doc["scene_num"].AsInt32;
                                double last_position_x = position_info_doc["x_position"].AsDouble;
                                double last_position_y = position_info_doc["y_position"].AsDouble;
                                int current_semester = position_info_doc["Semester"].AsInt32;
                                int current_main_quest = position_info_doc["Main_Quest_num"].AsInt32;
                                int current_detail_quest = position_info_doc["Detail_Quest_num"].AsInt32;
                                int current_quest_state = position_info_doc["Quest_State"].AsInt32;

                                List<int> current_minigame_nums = new List<int>();
                                List<double> current_scores = new List<double>();
                                var minigame_filter = Builders<BsonDocument>.Filter.Eq("Nickname", user_nickname);
                                var minigame_records = MinigameRecord.Find(minigame_filter).ToList();
                                foreach (var record in minigame_records)
                                {
                                    int minigame_num = record["Minigame_num"].AsInt32;
                                    double score = record["score"].AsDouble;

                                    current_minigame_nums.Add(minigame_num);
                                    current_scores.Add(score);
                                }
                                //user_nickname으로 MinigameRecord 콜렉션에서 찾아서
                                //current_minigame_nums에는 Minigame_num 데이터들을
                                //current_scores에는 score 데이터들을 넣기

                                scene = last_scene;

                                islogon = true;

                                signup_login_state.Nickname = user_nickname;
                                signup_login_state.scene_num = last_scene;
                                signup_login_state.x_position = last_position_x;
                                signup_login_state.y_position = last_position_y;
                                signup_login_state.semester = current_semester;
                                signup_login_state.main_quest_num = current_main_quest;
                                signup_login_state.detail_quest_num = current_detail_quest;
                                signup_login_state.quest_state = current_quest_state;
                                signup_login_send.first_login_info = signup_login_state;
                                signup_login_state.minigame_nums = current_minigame_nums;
                                signup_login_state.scores = current_scores;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Send Login Fail");
                            pt_id = PROTOCOL.LOGIN_Fail;
                        }

                    }
                    signup_login_send.pt_id = pt_id;
                    string jsonData = JsonConvert.SerializeObject(signup_login_send);
                    byte[] send_buffer = Encoding.UTF8.GetBytes(jsonData);
                    clientSocket.Send(send_buffer);

                    if(islogon==false) StartReceive(clientSocket);
                    else callback_on_newclient(clientSocket, null, receivedInfo.signup_login_info.Email, nickname, scene);
                }
                else
                {
                    Console.WriteLine(e.SocketError);
                    client_sockets.Remove(clientSocket);
                    clientSocket.Close();
                    Console.WriteLine("Client disconnected >> " + client_sockets.Count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling receive: " + ex.Message);
                // 에러 처리
            }
        }
        void on_new_client(Socket client_socket, object token, string client_ID, string client_nickname, int scene_num)
        {
            Console.WriteLine("on_new_client");
            //멀티 스레딩에서 connected_count를 안전하게 증가시키는 역할
            //다른 스레드가 이 변수에 동시에 접근하여 값을 변경하는 것을 방지하고 원자적으로 증가
            Interlocked.Increment(ref this.connected_count);
            SocketAsyncEventArgs receive_args = this.receive_event_pool.Pop();
            SocketAsyncEventArgs send_args = this.send_event_pool.Pop();
            Token client_token = new Token(client_ID);
            client_token.on_session_closed += this.on_session_closed;
            receive_args.UserToken = client_token;
            send_args.UserToken = client_token;
            client_token.client_nickname = client_nickname;
            client_token.scene_num = scene_num;
            begin_receive(client_socket, receive_args, send_args);

            if (this.session_created_callback != null)
            {
                Console.WriteLine("add user");
                this.session_created_callback(client_token);
            }
            Console.WriteLine("connected_count >> " + users.Count + "\n");

        }
        void begin_receive(Socket socket, SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
            Console.WriteLine("Chat Program Start");
            Token user_token = receive_args.UserToken as Token;
            user_token.set_event_args(send_args, receive_args);
            user_token.socket = socket;

            byte[] client_receive = new byte[1024];
            user_token.receive_event_args.SetBuffer(client_receive, 0, client_receive.Length);

            bool pending = socket.ReceiveAsync(receive_args);
            if (!pending)
            {
                chat_client_receive_completed(socket, receive_args);
            }
        }
        void chat_client_receive_completed(object sender, SocketAsyncEventArgs e)
        {
            Token user_token = e.UserToken as Token;
            try
            {
                //Console.WriteLine("chat client receive completed");
                int bytesReceived = e.BytesTransferred;
                if (bytesReceived > 0 && e.SocketError == SocketError.Success)
                {
                    byte[] buffer = e.Buffer;
                    string receivedJson = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    message received_info = JsonConvert.DeserializeObject<message>(receivedJson);
                    Console.WriteLine(receivedJson);

                    if(received_info.pt_id == PROTOCOL.Position_Update)
                    {
                        var updateFilter = Builders<BsonDocument>.Filter.Eq("Nickname", user_token.client_nickname);
                        var update = Builders<BsonDocument>.Update
                            .Set("scene_num", received_info.ingame_info.scene_num)
                            .Set("x_position", received_info.ingame_info.x_position)
                            .Set("y_position", received_info.ingame_info.y_position);

                        UserCharacter.UpdateOne(updateFilter, update);
                        //Console.WriteLine("Nickname: " + user_token.client_nickname + " >> scene_num: " + received_info.ingame_info.scene_num + ", x_position: " + received_info.ingame_info.x_position + ", y_position: " + received_info.ingame_info.y_position);

                        message new_message1 = new message();
                        new_message1.pt_id = PROTOCOL.Deliver_Position;
                        login_success_info other_user_position = new login_success_info();
                        other_user_position.Nickname = received_info.ingame_info.own_nickname;
                        other_user_position.scene_num = received_info.ingame_info.scene_num;
                        other_user_position.x_position = received_info.ingame_info.x_position;
                        other_user_position.y_position = received_info.ingame_info.y_position;
                        new_message1.first_login_info = other_user_position;
                        string new_deliver_message1 = JsonConvert.SerializeObject(new_message1);
                        byte[] messageBuffer1 = Encoding.UTF8.GetBytes(new_deliver_message1);

                        message new_message2 = new message();
                        new_message2.pt_id = PROTOCOL.Delete_User;
                        login_success_info other_user_off = new login_success_info();
                        other_user_off.Nickname = user_token.client_nickname;
                        other_user_off.scene_num = user_token.scene_num;
                        new_message2.first_login_info = other_user_off;
                        string new_deliver_message2 = JsonConvert.SerializeObject(new_message2);
                        byte[] messageBuffer2 = Encoding.UTF8.GetBytes(new_deliver_message2);

                        if (user_token.scene_num == received_info.ingame_info.scene_num)
                        {
                            foreach (Token t in users)
                            {
                                if (t.client_nickname != user_token.client_nickname && t.scene_num == user_token.scene_num)
                                {
                                    try
                                    {
                                        t.socket.Send(messageBuffer1);
                                    }
                                    catch (Exception sendEx)
                                    {
                                        Console.WriteLine("Error sending to client: " + sendEx.Message);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (Token t in users)
                            {
                                if (t.client_nickname != user_token.client_nickname && t.scene_num == user_token.scene_num)
                                {
                                    try
                                    {
                                        t.socket.Send(messageBuffer2);
                                    }
                                    catch (Exception sendEx)
                                    {
                                        Console.WriteLine("Error sending to client: " + sendEx.Message);
                                    }
                                }
                            }
                            user_token.scene_num = received_info.ingame_info.scene_num;
                            foreach (Token t in users)
                            {
                                if (t.client_nickname != user_token.client_nickname && t.scene_num == user_token.scene_num)
                                {
                                    try
                                    {
                                        t.socket.Send(messageBuffer1);
                                    }
                                    catch (Exception sendEx)
                                    {
                                        Console.WriteLine("Error sending to client: " + sendEx.Message);
                                    }
                                }
                            }
                        }
                    }
                    else if(received_info.pt_id == PROTOCOL.Send_Message)
                    {
                        chat_manager_cs.enqueue_chat_message(user_token, received_info);
                    }
                    else if(received_info.pt_id == PROTOCOL.Quest_Start_Request || received_info.pt_id == PROTOCOL.Quest_Complete_Request || received_info.pt_id == PROTOCOL.MiniGame_End_Request || received_info.pt_id == PROTOCOL.Sub_Quest_End_Request)
                    {
                        Console.WriteLine("\nClient Send Quest Message\n");
                        game_manager_cs.enqueue_game_message(user_token, received_info);
                    }

                    bool pending = user_token.socket.ReceiveAsync(e);
                    if (!pending) chat_client_receive_completed(sender, e);
                }
                else
                {
                    Console.WriteLine("Logon Client disconnected");
                    client_sockets.Remove(user_token.socket);
                    user_token.socket.Close();
                    Console.WriteLine("Client disconnected >> " + client_sockets.Count);
                    on_session_closed(user_token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling receive: " + ex.Message);
                Console.WriteLine("Logon Client disconnected");
                client_sockets.Remove(user_token.socket);
                user_token.socket.Close();
                Console.WriteLine("Client disconnected >> " + client_sockets.Count);
                on_session_closed(user_token);
            }
        }
        void SendDataToToken(Token token, string message)
        {
            try
            {
                byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                token.socket.Send(messageBuffer);
                Console.WriteLine("deliver message");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending data to token: " + ex.Message);
                // 에러 처리
            }
        }
        void add_users(Token token)
        {
            Console.WriteLine("on_session_created");
            lock (users)
            {
                this.users.Add(token);
            }
        }
        void on_session_closed(Token token)
        {
            Interlocked.Decrement(ref this.connected_count);
            Console.WriteLine("on_session_closed");
            lock (users)
            {
                this.users.Remove(token);
                if (this.receive_event_pool != null) this.receive_event_pool.Push(token.receive_event_args);
                if (this.send_event_pool != null) this.send_event_pool.Push(token.send_event_args);
                var updateFilter = Builders<BsonDocument>.Filter.Eq("Email", token.client_ID);
                var update = Builders<BsonDocument>.Update.Set("LoginST", 0);
                var updateResult = UsersCollection.UpdateOne(updateFilter, update);

                message new_message = new message();
                new_message.pt_id = PROTOCOL.Delete_User;
                login_success_info other_user_off = new login_success_info();
                other_user_off.Nickname = token.client_nickname;
                other_user_off.scene_num = token.scene_num;
                new_message.first_login_info = other_user_off;
                string new_deliver_message = JsonConvert.SerializeObject(new_message);
                byte[] messageBuffer = Encoding.UTF8.GetBytes(new_deliver_message);

                foreach(Token t in users)
                {
                    try
                    {
                        t.socket.Send(messageBuffer);
                    }
                    catch (Exception sendEx)
                    {
                        Console.WriteLine("Error sending to client: " + sendEx.Message);
                    }
                }
                Console.WriteLine("user connected_count >> " + users.Count + "\n");
                token.token_close();
            }
        }
        public List<Token> deliver_current_tokens()
        {
            return users;
        }
    }
}