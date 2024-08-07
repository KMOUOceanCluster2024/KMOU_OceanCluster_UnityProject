using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connect_Server : MonoBehaviour
{
    public GameObject reconnect_button;
    public float timeoutDuration = 5f; // 타임아웃 시간 설정 (초 단위)

    public void Start()
    {
        Debug.Log("Start");
        connect_start();
    }

    public void connect_start()
    {
        // 네트워크 연결 시도
        NetworkManager.Instance.Connect("192.168.0.189", 7979);

        // 코루틴 실행
        StartCoroutine(ConnectWithTimeout());
    }

    private IEnumerator ConnectWithTimeout()
    {
        float elapsedTime = 0f;

        // 타임아웃 시간 동안 주기적으로 연결 상태 확인
        while (elapsedTime < timeoutDuration)
        {
            if (NetworkManager.Instance.IsConnected)
            {
                SceneManager.LoadScene("Title");
                yield break;
            }

            elapsedTime += 0.5f; // 0.5초 간격으로 확인
            yield return new WaitForSeconds(0.5f);
        }

        // 타임아웃이 지나도 연결되지 않으면 재연결 버튼 활성화
        reconnect_button.SetActive(true);
    }
}
