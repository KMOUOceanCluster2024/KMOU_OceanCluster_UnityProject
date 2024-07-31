using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Connect_Server : MonoBehaviour
{
    static public List<Token> serverlist;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        connect_start();
    }
    public void connect_start()
    {
        if(GlobalState.IsStart == false)
        {
            GlobalState.IsStart = true;
            int port = 7979;
            string serverIP = "172.30.98.187";
            serverlist = new List<Token>();

            IPAddress address = IPAddress.Parse(serverIP);
            IPEndPoint endpoint = new IPEndPoint(address, port);
            Connector connector = new Connector();
            connector.connected_callback += on_connected_gameserver;
            connector.connect(endpoint);
        }
    }
    static void on_connected_gameserver(Token server_token)
    {
        lock (serverlist)
        {
            serverlist.Add(server_token);
            Console.WriteLine("Connected! >> " + serverlist.Count);
        }
    }
}
