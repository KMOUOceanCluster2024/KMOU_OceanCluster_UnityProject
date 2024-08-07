using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionManager : MonoBehaviour
{
    float character_x_position;
    float character_y_position;
    string owner_nickname;
    private void Start()
    {
        owner_nickname = PlayerPrefs.GetString("client_nickname");
        character_x_position = PlayerPrefs.GetFloat("last_x_position");
        character_y_position = PlayerPrefs.GetFloat("last_y_position");
        this.transform.position = new Vector3(character_x_position, character_y_position, -1.0f);
        StartCoroutine(UpdateSendPosition());
    }
    IEnumerator UpdateSendPosition()
    {
        while (true)
        {
            position_info_send();
            // 1√  ¥Î±‚
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void position_info_send()
    {
        message position_update_info = new message();
        position_update_info.pt_id = PROTOCOL.Position_Update;
        InGame_message ingame_position_info = new InGame_message();
        ingame_position_info.own_nickname = owner_nickname;
        ingame_position_info.x_position = this.transform.position.x;
        ingame_position_info.y_position = this.transform.position.y;
        position_update_info.ingame_info = ingame_position_info;
        NetworkManager.Instance.SendData(position_update_info);
    }
}
