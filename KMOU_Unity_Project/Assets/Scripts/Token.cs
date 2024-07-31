using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Token : MonoBehaviour
{
    public Socket socket { get; set; }
    public SocketAsyncEventArgs receive_event_args { get; private set; }
    public SocketAsyncEventArgs send_event_args { get; private set; }

    public delegate void ClosedDelegate(Token token);
    public ClosedDelegate on_session_closed;

    public string client_ID;
    int is_closed;
    public Token(string client_ID)
    {
        this.client_ID = client_ID;
    }
    public void set_event_args(SocketAsyncEventArgs send_args, SocketAsyncEventArgs receive_args)
    {
        this.send_event_args = send_args;
        this.receive_event_args = receive_args;
    }

    public void start_chat()
    {
        Console.WriteLine("Chat program Start");
        //비동기 송신, 비동기 수신
        ReceiveData();
        SendLoop();
    }
    void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        receive_event_args.SetBuffer(buffer, 0, buffer.Length);
        receive_event_args.UserToken = socket;
        receive_event_args.Completed += Message_received_completed;

        // 비동기 수신 시작
        bool pending = socket.ReceiveAsync(receive_event_args);
        if (!pending)
        {
            // 동기적으로 완료됨, 직접 완료 처리
            Message_received_completed(socket, receive_event_args);
        }
    }
    void Message_received_completed(object send, SocketAsyncEventArgs e)
    {
        try
        {
            int bytesReceived = e.BytesTransferred;
            if (bytesReceived > 0 && e.SocketError == SocketError.Success)
            {
                byte[] receive_buffer = e.Buffer;
                string server_send_message = Encoding.UTF8.GetString(receive_buffer, 0, bytesReceived);
                Console.WriteLine(server_send_message);

                bool pending = socket.ReceiveAsync(e);
                if (!pending) Message_received_completed(send, e);
            }
            else
            {
                on_session_closed(this);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling receive: " + ex.Message);
        }
    }
    void SendLoop()
    {
        while (true)
        {
            string message = Console.ReadLine();
            if (message.ToLower() == "q")
            {
                break;
            }

            SendData(message);
        }
    }
    void SendData(string data)
    {
        string data_add_ID = client_ID + " >> " + data;
        byte[] messageBuffer = Encoding.UTF8.GetBytes(data_add_ID);

        // 새로운 SocketAsyncEventArgs 객체를 생성하여 사용합니다.
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        sendArgs.SetBuffer(messageBuffer, 0, messageBuffer.Length);
        sendArgs.UserToken = socket;
        sendArgs.Completed += SendCallback;

        if (!socket.SendAsync(sendArgs))
        {
            // 비동기 송신을 시작하고 바로 완료되면 SendCallback을 직접 호출합니다.
            SendCallback(socket, sendArgs);
        }
    }

    void SendCallback(object sender, SocketAsyncEventArgs e)
    {
        try
        {
            if (e.SocketError == SocketError.Success)
            {
                int bytesSent = e.BytesTransferred;
                Console.WriteLine("Sent {0} bytes to server", bytesSent);
            }
            else
            {
                Console.WriteLine("Error sending data to server: " + e.SocketError.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending data to server: " + ex.Message);
        }
    }
    public void token_close()
    {
        Console.WriteLine("token close");
        if (Interlocked.CompareExchange(ref this.is_closed, 1, 0) == 1)
        {
            return;
        }
        this.socket.Close();
        this.socket = null;
        this.send_event_args = null;
        this.receive_event_args = null;
    }
}
