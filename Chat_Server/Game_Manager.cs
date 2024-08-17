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
            ingame_new_message.ingame_info = new_info;
            request_message.game_message = ingame_new_message;
            game_message_queue.Enqueue(request_message);
        }
    }
}