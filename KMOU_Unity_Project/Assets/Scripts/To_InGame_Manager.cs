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

public class To_InGame_Manager : MonoBehaviour
{
    Dictionary<int, string> scene_num_to_name = new Dictionary<int, string>();
    float character_x_position;
    float character_y_position;
    // Start is called before the first frame update
    void Start()
    {
        scene_num_to_name.Add(0, "Bridge");
        scene_num_to_name.Add(1, "Bridge 1");
        scene_num_to_name.Add(-1, "Waiting_Room");

        character_x_position = PlayerPrefs.GetFloat("last_x_position");
        character_y_position = PlayerPrefs.GetFloat("last_y_position");
        this.transform.position = new Vector3(character_x_position, character_y_position, -1.0f);

        string to_last_scene = scene_num_to_name[PlayerPrefs.GetInt("scene_num")];
        SceneManager.LoadScene(to_last_scene);
    }
}
