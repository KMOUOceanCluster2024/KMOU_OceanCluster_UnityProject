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
    class Chat_Manager
    {
        class chat_queue_info
        {
            public Token owner_info { get; set; }
            public message chat_message { get; set; }
        }
        Queue<chat_queue_info> chat_message_queue;
        NetworkService server_network;

        public IMongoCollection<BsonDocument> ChatLogCollection;
        private const string ChatLog_Collection = "Chat_Log";
        public Chat_Manager()
        {
            chat_message_queue = new Queue<chat_queue_info>();
        }
        public void start_chatmanager(NetworkService server_network)
        {
            this.server_network = server_network;
            ChatLogCollection = server_network.database.GetCollection<BsonDocument>(ChatLog_Collection);
            Thread chat_manager_thread = new Thread(chat_manager_do_thread);
            chat_manager_thread.Start();
        }
        public void chat_manager_do_thread()
        {
            while(true)
            {
                chat_queue_info chat_request;
                lock(chat_message_queue)
                {
                    if (chat_message_queue.Count == 0) continue;
                    chat_request = chat_message_queue.Dequeue();
                }
                var doc = new BsonDocument { { "Nickname", chat_request.owner_info.client_nickname }, { "Room_num", chat_request.chat_message.ingame_info.room_num }, { "message", chat_request.chat_message.ingame_info.message }, { "receiver", chat_request.chat_message.ingame_info.target_nickname } };
                ChatLogCollection.InsertOne(doc);
                switch(chat_request.chat_message.ingame_info.room_num)
                {
                    case 0:
                        List<Token> current_users = new List<Token>();
                        current_users = server_network.deliver_current_tokens();
                        message new_message = new message();
                        new_message.pt_id = PROTOCOL.Deliver_Message;
                        InGame_message ingame_chat = new InGame_message();
                        ingame_chat.room_num = chat_request.chat_message.ingame_info.room_num;
                        ingame_chat.message = chat_request.owner_info.client_nickname + ": " + chat_request.chat_message.ingame_info.message;
                        new_message.ingame_info = ingame_chat;
                        string new_deliver_message = JsonConvert.SerializeObject(new_message);
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(new_deliver_message);
                        foreach(var current_token in current_users)
                        {
                            current_token.socket.Send(messageBuffer);
                        }
                        break;
                    case 1:
                        break;
                    default:
                        break;
                }
            }
        }
        public void enqueue_chat_message(Token client_token, message received_chat_message)
        {
            Console.WriteLine(received_chat_message.ingame_info.message);
            chat_queue_info request_message = new chat_queue_info();
            request_message.owner_info = client_token;
            message ingame_new_message = new message();
            InGame_message new_info = new InGame_message();
            new_info.room_num = received_chat_message.ingame_info.room_num;
            new_info.message = received_chat_message.ingame_info.message;
            new_info.target_nickname = received_chat_message.ingame_info.target_nickname;
            ingame_new_message.ingame_info = new_info;
            request_message.chat_message = ingame_new_message;
            chat_message_queue.Enqueue(request_message);
            Console.WriteLine(chat_message_queue.Count);
        }
    }
}
