using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionManager : MonoBehaviour
{
    float character_x_position;
    float character_y_position;
    string owner_nickname;
    
    //public OtherUsersManager other_users_list;
    
    private void Start()
    {
        //other_users_list.change_scene();
        owner_nickname = PlayerPrefs.GetString("client_nickname");

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
        if(NetworkManager.Instance.scene_num == -1) ingame_position_info.scene_num = 0;
        else ingame_position_info.scene_num = NetworkManager.Instance.scene_num;
        ingame_position_info.x_position = this.transform.position.x;
        ingame_position_info.y_position = this.transform.position.y;
        position_update_info.ingame_info = ingame_position_info;
        NetworkManager.Instance.SendData(position_update_info);
    }
    
}
