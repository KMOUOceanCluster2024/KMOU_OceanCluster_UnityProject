using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading;
using UnityEngine.UI;

public class Quest_Manager : MonoBehaviour
{
    public static bool minigame = false;
    public Queue<InGame_message> Quest_Manager_Queue;
    public Button MiniGame_Button;
    public Button MiniGame_End_Button;
    public Button Quest_Complete_Button;
    public NPC dialoguenpc_isend;

    public void Start()
    {
        Quest_Manager_Queue = new Queue<InGame_message>();
        MiniGame_Button.gameObject.SetActive(false);
        MiniGame_End_Button.gameObject.SetActive(false);
        Quest_Complete_Button.gameObject.SetActive(false);
        if(minigame) MiniGame_End_Button.gameObject.SetActive(true);
    }
    public void quest_message_enqueue(InGame_message quest_message)
    {
        Quest_Manager_Queue.Enqueue(quest_message);
    }
    public void minigame_scene()
    {
        minigame = true;
        MiniGame_Button.gameObject.SetActive(false);
        MiniGame_End_Button.gameObject.SetActive(true);
        SceneManager.LoadScene("MiniGame");
    }
    public void start_quest_send()
    {
        Debug.Log("start_quest_send");
        InGame_message message_send = new InGame_message();
        message_send.pt_id = PROTOCOL.Quest_Start_Request;
        message_send.quest_num = 0;

        string info_json = JsonConvert.SerializeObject(message_send);
        byte[] info_data = Encoding.UTF8.GetBytes(info_json);
        GlobalState.ClientSocket.Send(info_data);
    }
    public void end_quest_send()
    {
        InGame_message message_send = new InGame_message();
        message_send.pt_id = PROTOCOL.Quest_Complete_Request;
        message_send.quest_num = 0;

        string info_json = JsonConvert.SerializeObject(message_send);
        byte[] info_data = Encoding.UTF8.GetBytes(info_json);
        GlobalState.ClientSocket.Send(info_data);
        Debug.Log(info_json);
    }
    public void minigame_end_send()
    {
        InGame_message message_send = new InGame_message();
        message_send.pt_id = PROTOCOL.MiniGame_End_Request;
        message_send.score = 50;
        string info_json = JsonConvert.SerializeObject(message_send);
        Debug.Log(info_json);
        byte[] info_data = Encoding.UTF8.GetBytes(info_json);
        GlobalState.ClientSocket.Send(info_data);
    }
    public void Update()
    {
        lock (Quest_Manager_Queue)
        {
            // Ã¤ÆÃ ui
            if (Quest_Manager_Queue.Count != 0)
            {
                InGame_message new_quest_message = Quest_Manager_Queue.Dequeue();
                if(new_quest_message.pt_id == PROTOCOL.Quest_Start_Success)
                {
                    MiniGame_Button.gameObject.SetActive(true);
                }
                else if(new_quest_message.pt_id == PROTOCOL.MiniGame_End_Success)
                {
                    MiniGame_End_Button.gameObject.SetActive(false);
                    Quest_Complete_Button.gameObject.SetActive(true);
                }
                else if(new_quest_message.pt_id == PROTOCOL.Quest_Complete_Success)
                {
                    Quest_Complete_Button.gameObject.SetActive(false);
                    dialoguenpc_isend.dialogueend = false;
                }
            }
        }
    }

}
