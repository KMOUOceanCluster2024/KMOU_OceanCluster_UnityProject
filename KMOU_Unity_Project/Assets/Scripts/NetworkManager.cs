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
}

public class InGame_message
{
    public double x_position { get; set; }
    public double y_position { get; set; }
    public int scene_num { get; set; }
    public int room_num { get; set; }
    public string target_nickname { get; set; }
    public string message { get; set; }
    public int quest_num { get; set; }
    public string own_nickname { get; set; }
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    private Socket _socket;
    private SocketAsyncEventArgs _receiveEventArgs;
    private byte[] _receiveBuffer;
    public bool IsConnected = false;
    public PROTOCOL current_pt_id = PROTOCOL.Setting;

    public Queue<message> every_messages = new Queue<message>();
    public Queue<message> signup_messages = new Queue<message>();
    public Queue<message> login_messages = new Queue<message>();
    public Queue<message> chat_messages = new Queue<message>();
    public Queue<message> other_users = new Queue<message>();
    public int scene_num;

    Dictionary<string, int> scene_name_to_num = new Dictionary<string, int>();

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NetworkManager");
                _instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        scene_name_to_num.Add("Bridge", 0);
        scene_name_to_num.Add("Bridge 1", 1);
        scene_name_to_num.Add("Waiting_Room", -1);
        scene_name_to_num.Add("Title", -1);
        scene_name_to_num.Add("SignUp", -1);
        scene_name_to_num.Add("SampleScene", -1);
        scene_name_to_num.Add("Login", -1);
        // �� ��ȯ �̺�Ʈ �ڵ鷯 ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // �� ��ȯ �̺�Ʈ �ڵ鷯 ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene_name_to_num.TryGetValue(scene.name, out int sceneNumber))
        {
            scene_num = sceneNumber;
            Debug.Log("���ο� �� �ε��: " + scene.name + " - Scene Num: " + scene_num);
        }
        else
        {
            Debug.LogWarning("�� �̸��� ��ųʸ��� ����: " + scene.name);
        }
    }

    public void Connect(string ipAddress, int port)
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ipAddress, port);
            IsConnected = true;
            Debug.Log("Connected to server.");

            // �񵿱� ���� ����
            _receiveBuffer = new byte[1024];
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            _receiveEventArgs.Completed += OnReceiveCompleted;

            StartReceive();
        }
        catch (Exception ex)
        {
            Debug.Log("Connection error: " + ex.Message);
        }
    }

    private void StartReceive()
    {
        if (_socket != null && _socket.Connected)
        {
            bool pending = _socket.ReceiveAsync(_receiveEventArgs);
            if (!pending)
            {
                OnReceiveCompleted(this, _receiveEventArgs);
            }
        }
    }

    private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
        {
            try
            {
                int bytesReceived = e.BytesTransferred;
                string receivedJson = Encoding.UTF8.GetString(_receiveBuffer, 0, bytesReceived);
                message receivedInfo = JsonConvert.DeserializeObject<message>(receivedJson);
                Debug.Log(receivedJson);

                every_messages.Enqueue(receivedInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error handling receive: " + ex.Message);
            }

            // ���� ���� ����
            StartReceive();
        }
        else if (e.SocketError == SocketError.OperationAborted)
        {
            // ������ ������ �������� �߻��� ������ �����մϴ�.
            Debug.LogWarning("Receive operation was aborted.");
        }
        else
        {
            Debug.LogError("Receive error: " + e.SocketError.ToString());
            // ������ �ݰų� ��õ��ϴ� ���� ���� ó��
        }
    }

    private void Update()
    {
        if (every_messages.Count > 0)
        {
            lock (every_messages)
            {
                message new_message = every_messages.Dequeue();
                if (current_pt_id == PROTOCOL.SIGNUP_Request && (new_message.pt_id == PROTOCOL.SIGNUP_Success || new_message.pt_id == PROTOCOL.SIGNUP_Fail))
                {
                    signup_messages.Enqueue(new_message);
                }
                else if (current_pt_id == PROTOCOL.LOGIN_Request && (new_message.pt_id == PROTOCOL.LOGIN_Success || new_message.pt_id == PROTOCOL.LOGIN_Fail))
                {
                    login_messages.Enqueue(new_message);
                }
                else if (new_message.pt_id == PROTOCOL.Deliver_Message)
                {
                    chat_messages.Enqueue(new_message);
                }
                else if (new_message.pt_id == PROTOCOL.Deliver_Position || new_message.pt_id == PROTOCOL.Delete_User)
                {
                    if (new_message.first_login_info.scene_num == scene_num)
                    {
                        other_users.Enqueue(new_message);
                    }
                }
            }
        }
    }

    public void SendData(message data)
    {
        if (_socket != null && IsConnected)
        {
            try
            {
                current_pt_id = data.pt_id;
                string new_message = JsonConvert.SerializeObject(data);
                byte[] messageBuffer = Encoding.UTF8.GetBytes(new_message);
                _socket.Send(messageBuffer);
            }
            catch (Exception ex)
            {
                Debug.LogError("Send error: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Socket is not connected. Unable to send data.");
        }
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    public void CloseSocket()
    {
        if (_socket != null && _socket.Connected)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                IsConnected = false;
                Debug.Log("Socket closed.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error closing socket: " + ex.Message);
            }
        }
    }
}
