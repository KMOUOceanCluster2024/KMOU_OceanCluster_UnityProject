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

public class LoginManager : MonoBehaviour
{
    float last_x_position;
    float last_y_position;
    string client_nickname;
    void Update()
    {
        if (NetworkManager.Instance.login_messages.Count != 0)
        {
            lock (NetworkManager.Instance.login_messages)
            {
                message login_react = NetworkManager.Instance.login_messages.Dequeue();
                if (login_react.pt_id == PROTOCOL.LOGIN_Success)
                {
                    Debug.Log("로그인 성공!");
                    last_x_position = (float)login_react.first_login_info.x_position;
                    last_y_position = (float)login_react.first_login_info.y_position;
                    client_nickname = login_react.first_login_info.Nickname;
                    PlayerPrefs.SetFloat("last_x_position", last_x_position);
                    PlayerPrefs.SetFloat("last_y_position", last_y_position);
                    PlayerPrefs.SetString("client_nickname", client_nickname);
                    SceneManager.LoadScene("Bridge");
                }
                else
                {
                    Debug.Log("로그인 실패. 다시 시도해주세요.");
                }
            }
        }
    }

    public void Login_info_send()
    {
        message send_login = new message();
        send_login.pt_id = PROTOCOL.LOGIN_Request;
        login_info login_account = new login_info();
        login_account.Email = GameObject.Find("Canvas/InputID").GetComponent<TMP_InputField>().text;
        login_account.PW = GameObject.Find("Canvas/InputPW").GetComponent<TMP_InputField>().text;
        send_login.signup_login_info = login_account;
        NetworkManager.Instance.SendData(send_login);
    }
}
