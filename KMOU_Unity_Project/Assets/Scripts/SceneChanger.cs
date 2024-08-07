using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void SignUp() // 회원가입 누르면 실행
    {
        SceneManager.LoadScene("SignUp");
    }
    public void Login() // 회원가입 누르면 실행
    {
        SceneManager.LoadScene("Login");
    }
    public void Lobby()
    {
        SceneManager.LoadScene("Title");
    }
    public void test_go_bridge1()
    {
        SceneManager.LoadScene("Bridge 1");
    }
    public void test_go_bridge()
    {
        SceneManager.LoadScene("Bridge");
    }
}
