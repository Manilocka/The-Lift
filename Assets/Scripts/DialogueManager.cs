using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public bool IsDialogueActive { get; private set; }
    public static DialogueManager Instance;
    public DialogueUI ui;
    private DialogueSO currentDialogue;
    private DialogueSO mainDialogue; // Основной диалог
    private int currentIndex = 0;
    private int mainDialogueIndex = 0; // Индекс в основном диалоге
    private bool hasChoices = false;
    private int selectedChoice = 0; // Индекс выбранной опции

    public static bool DialogueDone = false; //это Олесечкин костыль
    public static void ResetDialogueDone()
    {
        DialogueDone = false;
    }
    //PlayerInLiftCheck playerLiftCheck;
    //public ElevatorMoving elevator;





    [Header("Настройки диалогов")]
    public bool oneTimeDialogueOnly = true;
    
    [Header("Диалог для повторных попыток")]
    public DialogueSO tiredDialogue; // Диалог "я устала говорить"

    // Простое отслеживание пройденных диалогов
    private static HashSet<string> completedDialogues = new HashSet<string>();

    // Добавляем ссылки на кнопки
    public Button choice1Button; // Первая кнопка выбора
    public Button choice2Button; // Вторая кнопка выбора (опционально)

    void Awake()
    {
        Instance = this;
        //playerLiftCheck = GameObject.FindObjectOfType<PlayerInLiftCheck>();
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        Debug.Log($"Попытка начать диалог: {dialogue.name}");
        
        // Простая проверка через HashSet
        if (oneTimeDialogueOnly && completedDialogues.Contains(dialogue.name))
        {
            Debug.Log("Диалог уже пройден, показываем tiredDialogue");
            if (tiredDialogue != null)
            {
                StartTiredDialogue();
            }
            return;
        }
        
        Debug.Log("Начинаем обычный диалог");
        IsDialogueActive = true;
        currentDialogue = dialogue;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
    }
    
    public void StartDialogueWithMain(DialogueSO dialogue)
    {
        if (oneTimeDialogueOnly && completedDialogues.Contains(dialogue.name))
        {
            if (tiredDialogue != null)
            {
                StartTiredDialogue();
            }
            return;
        }
        
        IsDialogueActive = true;
        mainDialogue = dialogue;
        currentDialogue = dialogue;
        mainDialogueIndex = 0;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
    }
    
    public void StartMainDialogue(DialogueSO dialogue)
    {
        IsDialogueActive = true;
        mainDialogue = dialogue;
        currentDialogue = dialogue;
        mainDialogueIndex = 0;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
    }
    
    public void StartTiredDialogue()
    {
        Debug.Log("Запускаем tiredDialogue");
        
        IsDialogueActive = true;
        currentDialogue = tiredDialogue;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
    }

    void DisplayLine()
    {
        if (!IsDialogueActive || currentDialogue == null)
        {
            return;
        }

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
            UpdateChoiceSelection();
        }
    }

    public void Continue()
    {
        if (!IsDialogueActive || currentDialogue == null)
        {
            return;
        }

        currentIndex++;
        selectedChoice = 0;
        
        if (currentDialogue == mainDialogue)
        {
            mainDialogueIndex = currentIndex;
        }
        
        DisplayLine();
    }

    void OnChoiceSelected(int choice)
    {
        var currentLine = currentDialogue.lines[currentIndex];
        
        if (choice >= 0 && choice < currentLine.choices.Count)
        {
            var selectedChoice = currentLine.choices[choice];
            
            if (selectedChoice.dialogueLines != null && selectedChoice.dialogueLines.Count > 0)
            {
                if (mainDialogue == null)
                {
                    mainDialogue = currentDialogue;
                }
                
                mainDialogueIndex = currentIndex;
                
                var tempDialogue = ScriptableObject.CreateInstance<DialogueSO>();
                tempDialogue.lines = selectedChoice.dialogueLines;
                
                StartDialogue(tempDialogue);
                StartCoroutine(ContinueAfterChoice());
            }
            else
            {
                EndDialogue();
            }
        }
    }
    
    private System.Collections.IEnumerator ContinueAfterChoice()
    {
        while (IsDialogueActive)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        if (mainDialogue == null)
        {
            EndDialogue();
            yield break;
        }
        
        currentDialogue = mainDialogue;
        mainDialogueIndex++;
        
        if (mainDialogueIndex < mainDialogue.lines.Count)
        {
            currentIndex = mainDialogueIndex;
            IsDialogueActive = true;
            ui.Show();
            DisplayLine();
        }
        else
        {
            EndDialogue();
        }
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

    public void EndDialogue()
    {
        Debug.Log($"Завершаем диалог: {currentDialogue?.name}");
        
        ui.Hide();
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = true;
        MouseLock mouseLock = FindObjectOfType<MouseLock>();
        if (mouseLock != null)
        {
            mouseLock.canLook = true;
            mouseLock.crosshair.gameObject.SetActive(true);
        }
        
        // Простое добавление в HashSet
        if (oneTimeDialogueOnly && currentDialogue != null)
        {
            Debug.Log($"Отмечаем диалог как пройденный: {currentDialogue.name}");
            completedDialogues.Add(currentDialogue.name);
        }

        // Проверяем, внутри ли игрок в лифте и можно ли открывать двери
        IsDialogueActive = false;
        DialogueDone = true;
        
        // Запускаем лифт к нужному этажу после завершения диалога в сцене day1_e
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "day1_e")
        {
            // Находим лифт и запускаем его к максимально доступному этажу
            GameObject elevator = GameObject.FindGameObjectWithTag("Elevator");
            if (elevator != null)
            {
                ElevatorMoving elevatorMoving = elevator.GetComponent<ElevatorMoving>();
                if (elevatorMoving != null && !elevatorMoving.canMove)
                {
                    // Используем максимально доступный этаж (последний в массиве stops)
                    // Это будет последний этаж, который может дать кнопка в лифте
                    int maxFloor = 3; // Судя по SendElevatorNew.cs, максимальный этаж = 3
                    
                    Debug.Log($"Запускаем лифт к максимально доступному этажу {maxFloor} после завершения диалога");
                    elevatorMoving.endStop = maxFloor; // Устанавливаем максимально доступный этаж
                    elevatorMoving.canMove = true; // Запускаем движение лифта
                }
            }
        }

        Debug.Log("Диалог полностью завершен, IsDialogueActive = false");
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
    
    // Простые методы для отслеживания пройденных диалогов
    public void ResetAllDialogueProgress()
    {
        completedDialogues.Clear();
        Debug.Log("Прогресс всех диалогов сброшен!");
    }

    void Update()
    {
        if (ui.gameObject.activeSelf && !ui.isTyping)
        {
            if (!hasChoices)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Continue();
                }
            }
            else
            {
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
                
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
                {
                    OnChoiceSelected(selectedChoice);
                }
            }
        }
    }
}