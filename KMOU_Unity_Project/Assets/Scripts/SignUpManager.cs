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

public class SignUpManager : MonoBehaviour
{
    void Update()
    {
        if(NetworkManager.Instance.signup_messages.Count != 0)
        {
            lock(NetworkManager.Instance.signup_messages)
            {
                message signup_react = NetworkManager.Instance.signup_messages.Dequeue();
                if(signup_react.pt_id == PROTOCOL.SIGNUP_Success)
                {
                    Debug.Log("회원가입 성공!");
                    SceneManager.LoadScene("Title");
                }
                else
                {
                    Debug.Log("회원가입 실패. 다시 시도해주세요.");
                }
            }
        }
    }
    public void Signup_info_send()
    {
        message send_signup = new message();
        send_signup.pt_id = PROTOCOL.SIGNUP_Request;
        login_info new_account = new login_info();
        new_account.Email = GameObject.Find("Canvas/InputID").GetComponent<TMP_InputField>().text;
        new_account.PW = GameObject.Find("Canvas/InputPW").GetComponent<TMP_InputField>().text;
        new_account.Nickname = GameObject.Find("Canvas/InputNickname").GetComponent<TMP_InputField>().text;
        send_signup.signup_login_info = new_account;
        NetworkManager.Instance.SendData(send_signup);
    }
}
