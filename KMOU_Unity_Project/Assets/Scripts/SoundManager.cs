using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource bgm; // 오디오 소스 퍼블릭으로 받아주기
    // Start is called before the first frame update
    private void Awake() {
        var soundManagers = FindObjectsOfType<SoundManager>(); // soundManager 다 가져옴
        if (soundManagers.Length == 1) { // 길이가 1인 첫번째
            DontDestroyOnLoad(gameObject); // 파괴되지 않음
        } else {Destroy(gameObject);} // 두 번째는 파괴됨 (씬 이동한 후 되돌아왔을 때)
    }
    void Start()
    {
        bgm.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
