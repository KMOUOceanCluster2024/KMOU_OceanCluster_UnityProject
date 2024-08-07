using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{

    static public MovingObject instance;

    // A 캐릭터 instance = a
    // B 캐릭터 instance = a

    public string currentMapName; // transferMap 스크립에 있는 transferMapName 변수의 값을 저장.

    private BoxCollider2D boxCollider;
    public LayerMask layerMask;

    public float speed; // 캐릭터들의 움직임을 담당할 변수 선언

    private Vector3 vector; // 3개의 값을 동시에 가지고 있는 변수(x, y, z축)

    public float runSpeed;
    private float applyRunSpeed; // Shift 키를 누른 경우에만 적용. 실제 적용되는 값.
    private bool applyRunFlag = false;

    public int walkCount;
    private int currentWalkCount;

    // speed = 2.4, walkCount = 20
    // 2.4 * 20 = 48. 한 번 방향키가 눌릴 때마다 48px만큼 이동
    // While문에서 빠져나가려면 current 필요
    // currentWalkCount += 1 , 20,

    private bool canMove = true;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    IEnumerator MoveCoroutine()
    {
        while (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            // 상하좌우 방향키가 눌렸을 때 아래 내용 실행

            if (Input.GetKey(KeyCode.LeftShift))
            {
                applyRunSpeed = runSpeed;
                applyRunFlag = true;
            }
            else
            {
                applyRunSpeed = 0; // 적용이 안 됨
                applyRunFlag = false;
            }


            vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z); // vector에 눌린 값 저장(x, y, z). z 값은 바뀌지 않음

            if (vector.x != 0)
                vector.y = 0;

            // vector.x = 1 : 우측으로 이동
            // vector.y = 0; y 정보는 필요 없음

            animator.SetFloat("DirX", vector.x);
            animator.SetFloat("DirY", vector.y);

            RaycastHit2D hit;
            // A지점, B지점
            // 레이저
            // hit = Null;
            // hit = 방해물

            Vector2 start = transform.position; // A지점, 캐릭터의 현재 위치 값
            Vector2 end = start + new Vector2(vector.x * speed * walkCount, vector.y * speed * walkCount); // B지점, 캐릭터가 이동하고자 하는 위치 값

            boxCollider.enabled = false;
            hit = Physics2D.Linecast(start, end, layerMask);
            boxCollider.enabled = true;

            if (hit.transform != null)
                break;

            animator.SetBool("Walking", true);

            while (currentWalkCount < walkCount)
            {
                // vector 값을 토대로 움직이기
                if (vector.x != 0) // vector에 저장된 x축이 0이 아닐 경우
                {
                    transform.Translate(vector.x * (speed + applyRunSpeed), 0, 0); // Vector.x * speed: Vector.x의 값은 +-1이 리턴 되므로 +-2.4가 됨

                    // transform.position = vector; // 움직이는 다른 방법
                }
                else if (vector.y != 0)
                {
                    transform.Translate(0, vector.y * (speed + applyRunSpeed), 0); // Translate 함수: 현재 있는 값에서 수치만큼 더해주는 것
                }
                if (applyRunFlag)
                    currentWalkCount++; // 한 회차 돌 때마다 current 1씩 증가. shift가 눌리면 2씩 증가
                currentWalkCount++;
                yield return new WaitForSeconds(0.01f); // 코루틴 기본 문법. 다중 처리 개념. 반복문이 20번이면 0.2초 대기
            }
            currentWalkCount = 0; // 반복문에서 빠져나오면 다시 0으로 초기화


        }
        animator.SetBool("Walking", false);
        canMove = true; // 코루틴이 완료되면 다시 방향키가 눌릴 수 있게 true로 변경
    }



    // Update is called once per frame. 매 프레임마다 함수를 실행
    void Update()
    {
        if (canMove) // 중복 실행 방지.
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) // Horizontal(수평): 우 방향키 1 리턴, 좌 방향키 -1 리턴. Vertical(수직): 상 방향키 1 리턴, 하 방향키 -1 리턴.
            {
                canMove = false; // 방향키가 눌린 순간 canMove = false
                StartCoroutine(MoveCoroutine()); // 코루틴을 실행시키는 명령어.
            }
        }

    }
}