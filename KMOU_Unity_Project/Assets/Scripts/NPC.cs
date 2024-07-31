using System.Collections;
        using UnityEngine;
        using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public Text dialogueText;  // UI 텍스트 컴포넌트
    public GameObject dialogueBox;  // 대화 상자 UI

    private string[] dialogueLines;  // 대화 문장 배열
    private int currentLineIndex = 0;  // 현재 대화 인덱스
    private bool dialogueActive = false;  // 대화 상태 여부
    public bool dialogueend = false;
    IEnumerator coroutine;
    public Quest_Manager quest_manager_cs;
    void Start()
    {
        // 대화 데이터 초기화 (실제 게임에서는 파일이나 데이터베이스에서 가져올 수 있음)
        dialogueLines = new string[]
        {
            "안녕하세요!",
            "만나서 반가워요.",
            "오늘 날씨가 참 좋네요.",
            "퀘스트를 줄게요."
        };

        // 시작 시 대화 상자 비활성화
        dialogueBox.SetActive(false);
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !dialogueActive && !dialogueend)
        {
            Debug.Log("getkeydown");
            dialogueActive = true;
            dialogueText.text = dialogueLines[currentLineIndex];
            dialogueBox.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && dialogueActive && !dialogueend)
        {
            Debug.Log(currentLineIndex + ", " + dialogueLines.GetLength(0));
            if (currentLineIndex >= dialogueLines.GetLength(0)-1)
            {
                dialogueend = true;
                Debug.Log("end >> " + currentLineIndex + ", " + dialogueLines.GetLength(0));
                dialogueActive = false;
                dialogueBox.SetActive(false);
                currentLineIndex = 0;
                quest_manager_cs.start_quest_send();
            }
            else
            {
                currentLineIndex++;
                dialogueText.text = dialogueLines[currentLineIndex];
            }
        }
    }
}