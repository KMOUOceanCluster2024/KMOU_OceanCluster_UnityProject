using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    Text timeT;
    Text bestT;

    float t = 0;

    bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    public Quest_Manager quest_manager_cs;

    void Start()
    {
        i = this;
        timeT = GameObject.Find("TimeText").GetComponent<Text>();
        
        bestT = GameObject.Find("BestText").GetComponent<Text>();
    }


    void Update()
    {
        if (isGameOver) return;

        t += Time.deltaTime;
        timeT.text = "TIME\n" + SetTime((int)t);
    }

    string SetTime(int t)
    {
        string min = (t / 60).ToString();

        if (int.Parse(min) < 10) min = "0" + min;

        string sec = (t % 60).ToString();

        if(int.Parse(sec) < 10) sec = "0" + sec;

        return min + ":" + sec;
    }

    public void GameOver()
    {
        isGameOver = true;
        SetBestTime();
    }

    void SetBestTime()
    {
        if(PlayerPrefs.HasKey("BEST"))
        {
            int b = PlayerPrefs.GetInt("BEST");

            if ((int)t > b)
                PlayerPrefs.SetInt("BEST", b = (int)t);

            bestT.text = "BEST\n" + SetTime(b);
        }
        else
        {
            PlayerPrefs.SetInt("BEST", (int)t);
            bestT.text = "BEST\n" + SetTime((int)t);
        }

        bestT.enabled = true;
        Debug.Log("gamemanager over");
        SceneManager.LoadScene("Bridge");
    }
}