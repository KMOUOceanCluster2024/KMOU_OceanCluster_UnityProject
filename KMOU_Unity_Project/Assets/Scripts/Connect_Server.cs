using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connect_Server : MonoBehaviour
{
    public GameObject reconnect_button;
    public float timeoutDuration = 5f; // Ÿ�Ӿƿ� �ð� ���� (�� ����)

    public void Start()
    {
        Debug.Log("Start");
        connect_start();
    }

    public void connect_start()
    {
        // ��Ʈ��ũ ���� �õ�
        NetworkManager.Instance.Connect("192.168.0.189", 7979);

        // �ڷ�ƾ ����
        StartCoroutine(ConnectWithTimeout());
    }

    private IEnumerator ConnectWithTimeout()
    {
        float elapsedTime = 0f;

        // Ÿ�Ӿƿ� �ð� ���� �ֱ������� ���� ���� Ȯ��
        while (elapsedTime < timeoutDuration)
        {
            if (NetworkManager.Instance.IsConnected)
            {
                SceneManager.LoadScene("Title");
                yield break;
            }

            elapsedTime += 0.5f; // 0.5�� �������� Ȯ��
            yield return new WaitForSeconds(0.5f);
        }

        // Ÿ�Ӿƿ��� ������ ������� ������ �翬�� ��ư Ȱ��ȭ
        reconnect_button.SetActive(true);
    }
}
