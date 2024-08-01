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

public class ChatManager : MonoBehaviour
{
    public TMP_InputField m_inputField;
    string chatText;
    
    // Update is called once per frame
    private void Start()
    {
        chatText = "";
    }
    void Update()
    {
        lock(NetworkManager.Instance.chat_messages)
        {
            if (NetworkManager.Instance.chat_messages.Count != 0)
            {
                message new_chat_react = NetworkManager.Instance.chat_messages.Dequeue();
                Debug.Log(new_chat_react.ingame_info.message);
                chatText += new_chat_react.ingame_info.message + "\n"; // 줄바꿈
                GameObject.Find("Canvas/Scroll View/Viewport/Content/Message_Text").GetComponent<TMP_Text>().text = chatText.TrimEnd('\n'); // 마지막 줄바꿈 제거
                GameObject.Find("Canvas/Scroll View").GetComponent<ScrollRect>().verticalNormalizedPosition = 0.0f;
            }
        }
    }
    public void OnEndEditEvent()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("end edit event enter");
            string strMessage = m_inputField.text;

            message message_send_info = new message();
            message_send_info.pt_id = PROTOCOL.Send_Message;
            InGame_message message_info = new InGame_message();
            message_info.room_num = 0;
            message_info.message = strMessage;
            message_info.target_nickname = "";
            message_send_info.ingame_info = message_info;
            NetworkManager.Instance.SendData(message_send_info);

            m_inputField.text = "";
        }
    }
}
