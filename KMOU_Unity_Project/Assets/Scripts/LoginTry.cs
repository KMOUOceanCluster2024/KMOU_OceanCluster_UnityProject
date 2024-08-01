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

public class LoginTry : MonoBehaviour
{
    string ID;
    public SocketAsyncEventArgs send_arg;
    public SocketAsyncEventArgs receive_arg;
    public delegate void ConnectedHandler(Token token);
    public ConnectedHandler connected_callback { get; set; }
    public Queue<login_success_info> messages = new Queue<login_success_info>();
    private void Start()
    {
        Thread Login_queue = new Thread(StartReceive);
        Login_queue.Start();
    }
    void StartReceive()
    {
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
                login_success_info received_info = JsonConvert.DeserializeObject<login_success_info>(receivedJson);
                Debug.Log(receivedJson);

                if (GlobalState.get_send_pt_id == PROTOCOL.LOGIN_Request)
                {
                    messages.Enqueue(received_info);
                    Debug.Log("message enqueue");
                    serverreplied = true;
                }
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
    public bool serverreplied = false;
    void Update()
    {
        // ���� ������ ���� ��쿡�� go_to_bridge()�� ȣ���մϴ�.
        if (serverreplied)
        {
            go_to_bridge();
            // ȣ�� �Ŀ��� �÷��׸� �缳���մϴ�.
            serverreplied = false;
        }
    }
    public void Login_Send()
    {
        GlobalState.get_send_pt_id = PROTOCOL.LOGIN_Request;   //���� op_code ���´��� ����
        accept_send send_data_info = new accept_send();
        login_info info = new login_info();
        ID = GameObject.Find("Canvas/InputID").GetComponent<TMP_InputField>().text;
        info.Email = GameObject.Find("Canvas/InputID").GetComponent<TMP_InputField>().text;
        info.PW = GameObject.Find("Canvas/InputPW").GetComponent<TMP_InputField>().text;
        send_data_info.pt_id = PROTOCOL.LOGIN_Request;
        send_data_info.info = info;
        string info_json = JsonConvert.SerializeObject(send_data_info);
        byte[] info_data = Encoding.UTF8.GetBytes(info_json);
        Debug.Log(GlobalState.ClientSocket);
        GlobalState.ClientSocket.Send(info_data);

        StartCoroutine(WaitForSignupResponse());
    }
    IEnumerator WaitForSignupResponse()
    {
        float timer = 0f;
        float timeout = 10f;

        while (!serverreplied && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
    public void go_to_bridge()
    {
        Debug.Log("go to bridge");
        while (messages.Count > 0)
        {
            login_success_info server_send_message = messages.Dequeue();
            switch (server_send_message.pt_id)
            {
                case PROTOCOL.LOGIN_Success:
                    PlayerPrefs.SetFloat("x_position", (float)server_send_message.x_position);
                    PlayerPrefs.SetFloat("y_position", (float)server_send_message.y_position);
                    Debug.Log("Login success!");
                    SceneManager.LoadScene("Bridge");
                    return;
                case PROTOCOL.LOGIN_Fail:
                    Debug.Log("Login failed!");
                    return;
            }
        }
    }
    public Token client_chat_manager;
    public void socket_close(Token token)
    {
        Console.WriteLine("socket close");
        token.token_close();
    }


}
