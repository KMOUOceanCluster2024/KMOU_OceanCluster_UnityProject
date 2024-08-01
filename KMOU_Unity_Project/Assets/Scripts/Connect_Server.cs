using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connect_Server : MonoBehaviour
{
    public GameObject reconnect_button;
    void Start()
    {
        Debug.Log("Start");
        connect_start();
    }

    public void connect_start()
    {
        NetworkManager.Instance.Connect("192.168.0.189", 7979);
        if (NetworkManager.Instance.IsConnected)
        {
            SceneManager.LoadScene("Title");
        }
        else
        {
            reconnect_button.SetActive(true);
        }
    }
}
