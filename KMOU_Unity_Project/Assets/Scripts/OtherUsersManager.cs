using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherUsersManager : MonoBehaviour
{
    class users
    {
        public GameObject other_user;
        public string other_user_nickname;
    }
    List<users> other_users_character = new List<users>();
    public GameObject userPrefab;
    void Update()
    {
        if (NetworkManager.Instance.other_users.Count != 0)
        {
            lock (NetworkManager.Instance.other_users)
            {
                message other_users_position = NetworkManager.Instance.other_users.Dequeue();
                login_success_info other_user_info = other_users_position.first_login_info;

                bool userFound = false;
                foreach (users u in other_users_character)
                {
                    if (other_user_info.Nickname == u.other_user_nickname && other_users_position.pt_id == PROTOCOL.Deliver_Position)
                    {
                        // 기존 사용자 위치 업데이트
                        u.other_user.transform.position = new Vector3((float)other_user_info.x_position, (float)other_user_info.y_position, -1.0f);
                        userFound = true;
                        break;
                    }
                    else if (other_user_info.Nickname == u.other_user_nickname && other_users_position.pt_id == PROTOCOL.Delete_User)
                    {
                        if (u.other_user != null)
                        {
                            Destroy(u.other_user);
                        }
                        other_users_character.Remove(u);
                        Debug.Log($"Removed user: {u.other_user_nickname}");
                        userFound = true;
                        break;
                    }
                }

                if (!userFound)
                {
                    // 새로운 사용자 생성
                    GameObject newUser = Instantiate(userPrefab, new Vector3((float)other_user_info.x_position, (float)other_user_info.y_position, -1.0f), Quaternion.identity);
                    newUser.SetActive(true);
                    other_users_character.Add(new users { other_user = newUser, other_user_nickname = other_user_info.Nickname });
                }
            }
        }
    }
}
