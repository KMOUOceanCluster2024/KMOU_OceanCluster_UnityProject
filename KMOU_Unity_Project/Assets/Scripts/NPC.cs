using System.Collections;
        using UnityEngine;
        using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public Text dialogueText;  // UI �ؽ�Ʈ ������Ʈ
    public GameObject dialogueBox;  // ��ȭ ���� UI

    private string[] dialogueLines;  // ��ȭ ���� �迭
    private int currentLineIndex = 0;  // ���� ��ȭ �ε���
    private bool dialogueActive = false;  // ��ȭ ���� ����
    public bool dialogueend = false;
    IEnumerator coroutine;
    public Quest_Manager quest_manager_cs;
    void Start()
    {
        // ��ȭ ������ �ʱ�ȭ (���� ���ӿ����� �����̳� �����ͺ��̽����� ������ �� ����)
        dialogueLines = new string[]
        {
            "�ȳ��ϼ���!",
            "������ �ݰ�����.",
            "���� ������ �� ���׿�.",
            "����Ʈ�� �ٰԿ�."
        };

        // ���� �� ��ȭ ���� ��Ȱ��ȭ
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