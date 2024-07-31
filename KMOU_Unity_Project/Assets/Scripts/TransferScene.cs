using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransferScene : MonoBehaviour
{
    public string transferMapName; // 이동할 맵의 이름.

    private MovingObject thePlayer;

    void Start()
    {
        thePlayer = FindObjectOfType<MovingObject>(); //다수의 객체.
        // GetComponent // 단일 객체.
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "해양이")
        {
            thePlayer.currentMapName = transferMapName;
            SceneManager.LoadScene(transferMapName);
        }
    }
}
