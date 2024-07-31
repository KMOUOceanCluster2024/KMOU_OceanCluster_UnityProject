using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    Rigidbody2D rb;
    public float speed = 10;

    Animator anit;

    bool isDeath = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        anit = GetComponent<Animator>();
    }
    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(x * speed, 0);

        anit.SetBool("isMove", x != 0 ? true : false);

        if (x != 0 && x != transform.localScale.x)
            transform.localScale = new Vector3(x * 2, 2, 2);
    }

    public void Death()
    {
        isDeath = true;
        GameManager.i.GameOver();

        anit.SetTrigger("Death");
        Destroy(gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDeath || GameManager.i.IsGameOver) return;

        Move();
    }


}
