using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

    //private FadeManager theFade; // Fade 연출
    //private AudioManager theAudio; // sound

    //public string click_sound;

    //private PlayerManager thePlayer; // 시작할 때 현재 scene 위치와 map 이름 넣기
    //private GameManager theGM; // 시작하면 호출. scene 이동 후 카메라 제위치

    // Start is called before the first frame update
    void Start() {

        //theFade = FindObjectOfType<FadeManager>();
        //theAudio = FindObjectOfType<AudioManager>();
        //thePlayer = FindObjectOfType<PlayerManager>();
        //theGM = FindObjectOfType<GameManager>();

    }

    public void Login() // 로그인 누르면 실행
    {
        SceneManager.LoadScene("Login");
    }

    public void SignUp() // 회원가입 누르면 실행
    {
        SceneManager.LoadScene("signUp");
    }

    public void Continue() // 회원가입 누르면 실행
    {
        SceneManager.LoadScene("Bridge");
    }
    //IEnumerator GameStartCoroutine() // 대기 시간
    //{

    //}
}