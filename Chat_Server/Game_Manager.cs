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
    class Game_Manager
    {
        class game_queue_info
        {
            public Token owner_info { get; set; }
            public message game_message { get; set; }
        }
        NetworkService server_network;
        Queue<game_queue_info> game_message_queue;
        List<Token> scene_users = new List<Token>();

        public Game_Manager()
        {
            game_message_queue = new Queue<game_queue_info>();
        }
        public void start_gamemanager(NetworkService server_network)
        {
            this.server_network = server_network;
            Thread game_manager_thread = new Thread(game_manager_do_thread);
            game_manager_thread.Start();
        }
        public void game_manager_do_thread()
        {
            while (true)
            {
                game_queue_info game_request;
                lock (game_message_queue)
                {
                    if (game_message_queue.Count == 0) continue;
                    game_request = game_message_queue.Dequeue();
                }
                Console.WriteLine(game_request.game_message.pt_id);
                switch (game_request.game_message.pt_id)
                {
                    case PROTOCOL.Quest_Start_Request:
                        message new_message1 = new message();
                        new_message1.pt_id = PROTOCOL.Quest_Start_Success;
                        InGame_message quest_info1 = new InGame_message();
                        quest_info1.semester = game_request.game_message.ingame_info.semester;
                        quest_info1.main_quest_num = game_request.game_message.ingame_info.main_quest_num;
                        quest_info1.detail_quest_num = game_request.game_message.ingame_info.detail_quest_num;
                        quest_info1.quest_state = 1;
                        new_message1.ingame_info = quest_info1;
                        string new_deliver_message1 = JsonConvert.SerializeObject(new_message1);
                        byte[] messageBuffer1 = Encoding.UTF8.GetBytes(new_deliver_message1);
                        game_request.owner_info.socket.Send(messageBuffer1);
                        Console.WriteLine("Sent to Client Quest_Start_Success");

                        //DB 작업 => this.server_network.UserCharacter
                        var updateFilter1 = Builders<BsonDocument>.Filter.Eq("Nickname", game_request.owner_info.client_nickname);
                        var update1 = Builders<BsonDocument>.Update
                            .Set("Main_Quest_num", quest_info1.main_quest_num)
                            .Set("Detail_Quest_num", quest_info1.detail_quest_num)
                            .Set("Quest_State", quest_info1.quest_state);
                        this.server_network.UserCharacter.UpdateOne(updateFilter1, update1);
                        break;
                    case PROTOCOL.Quest_Complete_Request:
                        message new_message2 = new message();
                        new_message2.pt_id = PROTOCOL.Quest_Complete_Success;
                        InGame_message quest_info2 = new InGame_message();
                        quest_info2.semester = game_request.game_message.ingame_info.semester;
                        quest_info2.main_quest_num = game_request.game_message.ingame_info.main_quest_num;
                        quest_info2.detail_quest_num = game_request.game_message.ingame_info.detail_quest_num;
                        quest_info2.quest_state = 0;
                        new_message2.ingame_info = quest_info2;
                        string new_deliver_message2 = JsonConvert.SerializeObject(new_message2);
                        byte[] messageBuffer2 = Encoding.UTF8.GetBytes(new_deliver_message2);
                        game_request.owner_info.socket.Send(messageBuffer2);
                        Console.WriteLine("Sent to Client Quest_Complete_Success");

                        //DB 작업 => this.server_network.UserCharacter
                        var updateFilter2 = Builders<BsonDocument>.Filter.Eq("Nickname", game_request.owner_info.client_nickname);
                        var update2 = Builders<BsonDocument>.Update
                            .Set("Main_Quest_num", quest_info2.main_quest_num)
                            .Set("Detail_Quest_num", quest_info2.detail_quest_num)
                            .Set("Quest_State", quest_info2.quest_state);
                        this.server_network.UserCharacter.UpdateOne(updateFilter2, update2);
                        break;
                    case PROTOCOL.MiniGame_End_Request:
                        message new_message3 = new message();
                        new_message3.pt_id = PROTOCOL.MiniGame_End_Success;
                        InGame_message minigame_info = new InGame_message();
                        minigame_info.scene_num = game_request.owner_info.scene_num;
                        new_message3.ingame_info = minigame_info;
                        string new_deliver_message3 = JsonConvert.SerializeObject(new_message3);
                        byte[] messageBuffer3 = Encoding.UTF8.GetBytes(new_deliver_message3);
                        game_request.owner_info.socket.Send(messageBuffer3);
                        Console.WriteLine("Send to Client MiniGame_End_Success");
                        
                        break;
                    case PROTOCOL.Sub_Quest_End_Request:
                        var filter1 = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("Nickname", game_request.owner_info.client_nickname), Builders<BsonDocument>.Filter.Eq("Minigame_num", game_request.game_message.ingame_info.minigame_num));
                        var docs = this.server_network.MinigameRecord.Find(filter1).ToList();
                        if (docs.Count == 1)    //이미 기록이 있을 때는 있는 기록이랑 비교해서 최고 기록으로 갱신
                        {
                            var firstDoc = docs.First();
                            if(firstDoc["score"].AsDouble < game_request.game_message.ingame_info.sub_quest_score)
                            {
                                var update = Builders<BsonDocument>.Update.Set("score", game_request.game_message.ingame_info.sub_quest_score); // 새로운 LoginST 값 설정
                                var updateResult = this.server_network.MinigameRecord.UpdateOne(filter1, update);
                            }
                        }
                        else
                        {
                            var doc1 = new BsonDocument { { "Nickname", game_request.owner_info.client_nickname }, { "Minigame_num", game_request.game_message.ingame_info.minigame_num }, { "score", game_request.game_message.ingame_info.sub_quest_score } };
                            this.server_network.MinigameRecord.InsertOne(doc1);
                        }   //기록이 없을 때는 추가

                        message new_message4 = new message();
                        new_message4.pt_id = PROTOCOL.Sub_Quest_End_Success;
                        InGame_message subquest_info = new InGame_message();
                        subquest_info.scene_num = game_request.owner_info.scene_num;
                        new_message4.ingame_info = subquest_info;
                        string new_deliver_message4 = JsonConvert.SerializeObject(new_message4);
                        byte[] messageBuffer4 = Encoding.UTF8.GetBytes(new_deliver_message4);
                        game_request.owner_info.socket.Send(messageBuffer4);
                        Console.WriteLine("Send to Client Sub Quest End Success");
                        break;
                    default:
                        break;
                }
            }
        }
        public void enqueue_game_message(Token client_token, message received_game_message)
        {
            game_queue_info request_message = new game_queue_info();
            request_message.owner_info = client_token;
            message ingame_new_message = new message();
            ingame_new_message.pt_id = received_game_message.pt_id;
            InGame_message new_info = new InGame_message();
            new_info.x_position = received_game_message.ingame_info.x_position;
            new_info.y_position = received_game_message.ingame_info.y_position;
            new_info.scene_num = received_game_message.ingame_info.scene_num;
            new_info.room_num = received_game_message.ingame_info.room_num;
            new_info.semester = received_game_message.ingame_info.semester;
            new_info.main_quest_num = received_game_message.ingame_info.main_quest_num;
            new_info.detail_quest_num = received_game_message.ingame_info.detail_quest_num;
            new_info.quest_state = received_game_message.ingame_info.quest_state;
            new_info.own_nickname = received_game_message.ingame_info.own_nickname;
            new_info.sub_quest_score = received_game_message.ingame_info.sub_quest_score;
            new_info.minigame_num = received_game_message.ingame_info.minigame_num;
            ingame_new_message.ingame_info = new_info;
            request_message.game_message = ingame_new_message;
            game_message_queue.Enqueue(request_message);
        }
    }
}