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

public class Chat_Manager : MonoBehaviour
{
    public GameObject m_Content;
    public TMP_InputField m_inputField;
    public string nickname;

    GameObject m_ContentText;
    string m_strUserName;

    public SocketAsyncEventArgs receive_arg;
    Queue<string> messageQueue;
    public Quest_Manager quest_message_manager;
    void Start()
    {
        messageQueue = new Queue<string>();
        try
        {
            // �񵿱� ���� �۾� ����
            receive_arg = GlobalState.get_receiveEventArgs; // GlobalState�� receiveEventArgs ���
            receive_arg.Completed += ReceiveCompleted;

            // �񵿱� ���� ����
            bool willRaiseEvent = GlobalState.ClientSocket.ReceiveAsync(receive_arg);
            if (!willRaiseEvent)
            {
                // ���������� �Ϸ��, ���� �Ϸ� ó��
                ReceiveCompleted(GlobalState.ClientSocket, receive_arg);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error starting receive: " + ex.Message);
            // ���� ó��
        }
    }
    void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log("Receive");
        try
        {
            int bytesReceived = e.BytesTransferred;
            if (bytesReceived > 0 && e.SocketError == SocketError.Success)
            {
                byte[] buffer = e.Buffer;
                string receivedJson = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                InGame_message received_message = JsonConvert.DeserializeObject<InGame_message>(receivedJson);
                if (received_message.pt_id == PROTOCOL.Deliver_Message)
                {
                    Debug.Log(received_message.message);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(received_message.message);
                        Debug.Log("Enqueue: " + received_message.message);
                    }
                }
                else if (received_message.pt_id == PROTOCOL.Quest_Start_Success || received_message.pt_id == PROTOCOL.MiniGame_End_Success || received_message.pt_id == PROTOCOL.Quest_Complete_Success)
                {
                    quest_message_manager.quest_message_enqueue(received_message);
                    Debug.Log("Enqueue: " + received_message.pt_id);
                }
                bool pending = GlobalState.ClientSocket.ReceiveAsync(receive_arg);
                if (!pending) ReceiveCompleted(sender, e);
            }
            else
            {
                GlobalState.SocketClose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling receive: " + ex.Message);
            // ���� ó��
        }
    }
    void Update()
    {
        lock(messageQueue)
        {
            if (messageQueue.Count != 0)
            {
                while (messageQueue.Count > 5) messageQueue.Dequeue(); // 5�� ������ ���� ������ �޽��� ����

                // �� �޽��� ǥ��
                string chatText = "";
                foreach (string message in messageQueue)
                {
                    chatText += message + "\n"; // �ٹٲ�
                }

                GameObject.Find("Canvas/Scroll View/Viewport/Content/Message_Text").GetComponent<TMP_Text>().text = chatText.TrimEnd('\n'); // ������ �ٹٲ� ����
            }
        }
    }
    public void OnEndEditEvent()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("end edit event enter");
            string strMessage = m_inputField.text;

            InGame_message message_send = new InGame_message();
            message_send.pt_id = PROTOCOL.Send_Message;
            message_send.room_num = 0;
            message_send.message = strMessage;
            message_send.target_nickname = "";

            string info_json = JsonConvert.SerializeObject(message_send);
            byte[] info_data = Encoding.UTF8.GetBytes(info_json);
            GlobalState.ClientSocket.Send(info_data);
            m_inputField.text = "";
        }
    }
}
