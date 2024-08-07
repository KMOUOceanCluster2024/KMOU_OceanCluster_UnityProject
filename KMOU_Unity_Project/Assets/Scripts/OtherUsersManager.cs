using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OtherUsersManager : MonoBehaviour
{
    class User
    {
        public GameObject other_user;
        public string other_user_nickname;
    }

    List<User> other_users_character = new List<User>();
    public GameObject userPrefab;

    private void Start()
    {
        GameObject[] remainingObjects = GameObject.FindGameObjectsWithTag("other_user");
        foreach (GameObject obj in remainingObjects)
        {
            Destroy(obj);
        }

        other_users_character.Clear();
        Debug.Log("Other Users Count >> " + other_users_character.Count);
    }

    void Update()
    {
        if (NetworkManager.Instance.other_users.Count != 0)
        {
            lock (NetworkManager.Instance.other_users)
            {
                message other_users_position = NetworkManager.Instance.other_users.Dequeue();
                login_success_info other_user_info = other_users_position.first_login_info;

                bool userFound = false;
                for (int i = 0; i < other_users_character.Count; i++)
                {
                    User u = other_users_character[i];
                    if (other_user_info.Nickname == u.other_user_nickname)
                    {
                        if (other_users_position.pt_id == PROTOCOL.Deliver_Position)
                        {
                            // 기존 사용자 위치 업데이트
                            if (u.other_user != null)
                            {
                                u.other_user.transform.position = new Vector3((float)other_user_info.x_position, (float)other_user_info.y_position, -1.0f);
                            }
                        }
                        else if (other_users_position.pt_id == PROTOCOL.Delete_User)
                        {
                            if (u.other_user != null)
                            {
                                Destroy(u.other_user);
                            }
                            other_users_character.RemoveAt(i);
                            Debug.Log($"Removed user: {u.other_user_nickname}");
                        }
                        userFound = true;
                        break;
                    }
                }

                if (!userFound && other_users_position.pt_id == PROTOCOL.Deliver_Position)
                {
                    // 새로운 사용자 생성
                    GameObject newUser = Instantiate(userPrefab, new Vector3((float)other_user_info.x_position, (float)other_user_info.y_position, -1.0f), Quaternion.identity);
                    newUser.SetActive(true);
                    other_users_character.Add(new User { other_user = newUser, other_user_nickname = other_user_info.Nickname });
                }
            }
        }
    }
    public void go_to_next_scene()
    {
        for (int i = other_users_character.Count - 1; i >= 0; i--)
        {
            Destroy(other_users_character[i].other_user);
        }
        GameObject[] remainingObjects = GameObject.FindGameObjectsWithTag("other_user");
        foreach (GameObject obj in remainingObjects)
        {
            Destroy(obj);
        }
        other_users_character.Clear();
        string scene_name = SceneManager.GetActiveScene().name;
        if (scene_name == "Bridge") SceneManager.LoadScene("Bridge 1");
        else if (scene_name == "Bridge 1") SceneManager.LoadScene("Bridge");
    }
}
