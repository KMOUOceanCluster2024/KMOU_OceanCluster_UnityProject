using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour {

    //private FadeManager theFade; // Fade ����
    //private AudioManager theAudio; // sound

    //public string click_sound;

    //private PlayerManager thePlayer; // ������ �� ���� scene ��ġ�� map �̸� �ֱ�
    //private GameManager theGM; // �����ϸ� ȣ��. scene �̵� �� ī�޶� ����ġ

    // Start is called before the first frame update
    void Start() {

        //theFade = FindObjectOfType<FadeManager>();
        //theAudio = FindObjectOfType<AudioManager>();
        //thePlayer = FindObjectOfType<PlayerManager>();
        //theGM = FindObjectOfType<GameManager>();

    }

    public void Login() // �α��� ������ ����
    {
        SceneManager.LoadScene("Login");
    }

    public void SignUp() // ȸ������ ������ ����
    {
        SceneManager.LoadScene("signUp");
    }

    public void Continue() // ȸ������ ������ ����
    {
        SceneManager.LoadScene("Bridge");
    }
    //IEnumerator GameStartCoroutine() // ��� �ð�
    //{

    //}
}