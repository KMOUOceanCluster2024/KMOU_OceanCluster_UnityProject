using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransferScene : MonoBehaviour
{
    public string transferMapName; // �̵��� ���� �̸�.

    private MovingObject thePlayer;

    void Start()
    {
        thePlayer = FindObjectOfType<MovingObject>(); //�ټ��� ��ü.
        // GetComponent // ���� ��ü.
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "�ؾ���")
        {
            thePlayer.currentMapName = transferMapName;
            SceneManager.LoadScene(transferMapName);
        }
    }
}
