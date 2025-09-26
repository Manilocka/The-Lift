using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public DialogueUI ui;
    private DialogueSO currentDialogue;
    private int currentIndex = 0;
    private bool hasChoices = false;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        currentDialogue = dialogue;
        currentIndex = 0;
        ui.Show();
        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }
        var line = currentDialogue.lines[currentIndex];
        hasChoices = line.choices.Count > 0;
        ui.ShowLine(line, OnChoiceSelected);
    }

    public void Continue()
    {
        currentIndex++;
        DisplayLine();
    }

    void OnChoiceSelected(int choice)
    {
        currentIndex = currentDialogue.lines[currentIndex].choices[choice].nextLineIndex;
        hasChoices = false;
        DisplayLine();
    }

    void EndDialogue()
    {
        ui.Hide();
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = true;
        MouseLock mouseLock = FindObjectOfType<MouseLock>();
        if (mouseLock != null)
        {
            mouseLock.canLook = true;
            mouseLock.crosshair.gameObject.SetActive(true); // Возвращаем crosshair
        }
    }

    void Update()
    {
        if (ui.gameObject.activeSelf && Input.GetMouseButtonDown(0) && !hasChoices && !ui.isTyping)
        {
            Continue();
        }
    }
}