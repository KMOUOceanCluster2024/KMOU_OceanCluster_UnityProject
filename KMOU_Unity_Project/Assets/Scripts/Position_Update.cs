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

public class Position_Update : MonoBehaviour
{
    float character_x_position;
    float character_y_position;
    void Start()
    {
        character_x_position = PlayerPrefs.GetFloat("x_position");
        character_y_position = PlayerPrefs.GetFloat("y_position");
        transform.position = new Vector3(character_x_position, character_y_position, -1.0f);
        StartCoroutine(UpdateSendPosition());
    }
    IEnumerator UpdateSendPosition()
    {
        while (true)
        {
            InGame_message position_send = new InGame_message();
            position_send.pt_id = PROTOCOL.Position_Update;
            position_send.x_position = transform.position.x;
            position_send.y_position = transform.position.y;

            string info_json = JsonConvert.SerializeObject(position_send);
            Debug.Log(info_json);
            byte[] info_data = Encoding.UTF8.GetBytes(info_json);
            GlobalState.ClientSocket.Send(info_data);

            // 1√  ¥Î±‚
            yield return new WaitForSeconds(1.0f);
        }
    }
}
