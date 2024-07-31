using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class login_info
{
    public string Email { get; set; }
    public string PW { get; set; }
    public string Nickname { get; set; }
}
public class accept_send
{
    public PROTOCOL pt_id { get; set; }
    public login_info info { get; set; }
}
public class InGame_message
{
    public PROTOCOL pt_id { get; set; }
    public double x_position { get; set; }
    public double y_position { get; set; }
    public int scene_num { get; set; }
    public int room_num { get; set; }
    public string target_nickname { get; set; }
    public string message { get; set; }
    public int quest_num { get; set; }
    public int score { get; set; }
}
public class login_success_info
{
    public PROTOCOL pt_id { get; set; }
    public string Nickname { get; set; }
    public int scene_num { get; set; }
    public double x_position { get; set; }
    public double y_position { get; set; }
} 
public static class GlobalState
{
    private static Socket clientSocket;
    private static bool isStart;
    private static SocketAsyncEventArgs receiveEventArgs;
    private static PROTOCOL send_pt_id;

    public static PROTOCOL get_send_pt_id
    {
        get { return send_pt_id; }
        set { send_pt_id = value; }
    }
    public static bool IsStart
    {
        get { return isStart; }
        set
        {
            isStart = value;
            PlayerPrefs.SetInt("isstart", isStart ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static Socket ClientSocket
    {
        get { return clientSocket; }
        set { clientSocket = value; }
    }
    public static SocketAsyncEventArgs get_receiveEventArgs
    {
        get { return receiveEventArgs; }
        set { receiveEventArgs = value; }
    }
    public static bool IsSocketConnected()
    {
        return clientSocket != null && clientSocket.Connected;
    }

    public static void SocketClose()
    {
        if (clientSocket != null)
        {
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (SocketException ex)
            {
                Debug.LogError("SocketException: " + ex.ToString());
            }
            finally
            {
                clientSocket = null;
                IsStart = false;
            }
        }
    }

    static GlobalState()
    {
        receiveEventArgs = new SocketAsyncEventArgs();
        StateObject state = new StateObject();
        state.workSocket = clientSocket;
        receiveEventArgs.UserToken = state;
        receiveEventArgs.SetBuffer(state.buffer, 0, StateObject.BufferSize);
    }

    private class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
}
public class Connector : MonoBehaviour
{
	public delegate void ConnectedHandler(Token token);
	public ConnectedHandler connected_callback { get; set; }
	public Token client_chat_manager;
	public Connector()
	{
		this.connected_callback = null;
	}
	SocketAsyncEventArgs event_arg;
    public void connect(IPEndPoint remote_endpoint)
    {
        GlobalState.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        GlobalState.ClientSocket.NoDelay = true;

        // 비동기 접속을 위한 event args는 GlobalState에서 초기화된 receiveEventArgs를 사용
        SocketAsyncEventArgs event_arg = new SocketAsyncEventArgs();
        event_arg.Completed += on_connect_completed;
        event_arg.RemoteEndPoint = remote_endpoint;
        bool pending = GlobalState.ClientSocket.ConnectAsync(event_arg);
        if (!pending)
        {
            on_connect_completed(null, event_arg);
        }
    }

    int accept_menu;
	string ID;
    private void on_connect_completed(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            Debug.Log("Connected to server.");
        }
        else
        {
            Debug.Log("Failed to connect to server: " + e.SocketError);
        }
    }


    public void connector_close()
	{
		client_chat_manager.token_close();
	}
}
