using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    public bool IsDialogueActive { get; private set; }
    public static DialogueManager Instance;
    public DialogueUI ui;
    private DialogueSO currentDialogue;
    private int currentIndex = 0;
    private bool hasChoices = false;
    private int selectedChoice = 0; // Индекс выбранной опции

    public bool DialogueDone = false; //это Олесечкин костыль

    // Добавляем ссылки на кнопки
    public Button choice1Button; // Первая кнопка выбора
    public Button choice2Button; // Вторая кнопка выбора (опционально)

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        IsDialogueActive = true;
        currentDialogue = dialogue;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
        Debug.Log("started dialog");
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
        if (hasChoices)
        {
            UpdateChoiceSelection(); // Обновляем выделение опций
        }
        Debug.Log("Displayed a line with index "+currentIndex+" = "+line );

    }

    public void Continue()
    {
        currentIndex++;
        selectedChoice = 0; // Сбрасываем выбор при переходе
        DisplayLine();
        Debug.Log("Continued dialog index"+ currentIndex);

    }

    void OnChoiceSelected(int choice)
    {
        currentIndex = currentDialogue.lines[currentIndex].choices[choice].nextLineIndex;
        hasChoices = false;
        selectedChoice = 0;
        DisplayLine();
    }

    void UpdateChoiceSelection()
    {
        var line = currentDialogue.lines[currentIndex];
        if (line.choices.Count > 0)
        {
            choice1Button.interactable = (selectedChoice == 0);
            if (line.choices.Count > 1)
            {
                choice2Button.interactable = (selectedChoice == 1);
            }
            
            // Обновляем подсветку выбора в UI
            ui.UpdateChoiceHighlight(selectedChoice, line.choices.Count);
        }
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
            mouseLock.crosshair.gameObject.SetActive(true);
        }
        IsDialogueActive = false;
        DialogueDone = true;
    }
    
    public DialogueLine? GetCurrentLine()
    {
        if (currentDialogue != null && currentIndex < currentDialogue.lines.Count)
        {
            return currentDialogue.lines[currentIndex];
        }
        return null;
    }
    
    public int GetSelectedChoice()
    {
        return selectedChoice;
    }

    void Update()
    {
        if (ui.gameObject.activeSelf && Input.GetMouseButtonDown(0)){
            // Continue();

            //EditorUtility.DisplayDialog("TUTA", "Message", "OK");
        }

        if (ui.gameObject.activeSelf && !ui.isTyping)
        {
            if (!hasChoices)
            {
                // Продолжение диалога - левая кнопка мыши или Enter
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Continue();
                }
            }
            else
            {
                // Навигация по выбору
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    selectedChoice = Mathf.Max(0, selectedChoice - 1);
                    UpdateChoiceSelection();
                }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    selectedChoice = Mathf.Min(currentDialogue.lines[currentIndex].choices.Count - 1, selectedChoice + 1);
                    UpdateChoiceSelection();
                }
                
                // Выбор опции - Enter или левая кнопка мыши
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
                {
                    OnChoiceSelected(selectedChoice);
                }
            }
        }
    }
}