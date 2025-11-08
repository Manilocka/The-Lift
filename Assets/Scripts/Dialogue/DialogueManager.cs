using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private PlayerInLiftCheck playerLiftCheck;
    private ElevatorMoving elevator;
    private ElevatorDoorController elevatorDoorController;

    void Start()
    {
        Instance = this;
        playerLiftCheck = FindObjectOfType<PlayerInLiftCheck>();
        elevator = FindObjectOfType<ElevatorMoving>();
        elevatorDoorController = FindObjectOfType<ElevatorDoorController>();
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
            
            // Проверяем, нужно ли загрузить сцену
            if (!string.IsNullOrEmpty(selectedChoice.sceneToLoad))
            {
                Debug.Log($"Переходим на сцену: {selectedChoice.sceneToLoad}");
                EndDialogue();
                UnityEngine.SceneManagement.SceneManager.LoadScene(selectedChoice.sceneToLoad);
                return;
            }
            
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
    
    private IEnumerator ContinueAfterChoice()
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

        // Проверяем, внутри ли игрок в лифте и можно ли открывать двери, не костыль, а кресло инвалидное


        IsDialogueActive = false;
        if (GetLastLine()?.hasImpact == true)
        {
            DialogueDone = true;
        }
        
        if (playerLiftCheck != null && playerLiftCheck.IsPlayerInsideElevator())
        {
            if (!elevator.canMove && elevator.rememberStartStop != elevator.startStop)
            {
                elevatorDoorController.OpenDoors();
            }
        }
        CheckForSceneTransition();

        Debug.Log("Диалог полностью завершен, IsDialogueActive = false");
    }
    private void CheckForSceneTransition()
    {
        // Проверяем текущую сцену
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    
        // Проверка для сцены day7_2
        if (currentSceneName == "day7_2")
        {
            // Проверяем, что диалог завершился и это нужный диалог
            // Вы можете добавить дополнительные проверки, например по имени диалога
            if (currentDialogue != null && AdditionalCheck.IsDialogueDone())
            {
                // Загружаем следующую сцену (замените "NextSceneName" на имя вашей сцены)
                string sceneToLoad = "day7_m"; // Замените на актуальное имя сцены
            
                Debug.Log($"Переход на сцену: {sceneToLoad} из day7_2");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
        // Проверка для сцены day7_e - переход на day7_fin после диалога day7_e_3
        else if (currentSceneName == "day7_e")
        {
            // Проверяем, что диалог завершился и это диалог day7_e_3
            if (currentDialogue != null && currentDialogue.name == "day7_e_3" && AdditionalCheck.IsDialogueDone())
            {
                string sceneToLoad = "day7_fin";
                
                Debug.Log($"Переход на сцену: {sceneToLoad} из day7_e после завершения диалога {currentDialogue.name}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
    public DialogueLine? GetCurrentLine()
    {
        if (currentDialogue != null && currentIndex < currentDialogue.lines.Count)
        {
            return currentDialogue.lines[currentIndex];
        }
        return null;
    }
    
    public DialogueLine? GetLastLine()
    {
        if (currentDialogue != null)
        {
            if (currentDialogue.lines.Count == 0) return null;
            if (currentIndex < currentDialogue.lines.Count)
            {
                return currentDialogue.lines[currentIndex];
            }
            return currentDialogue.lines[currentDialogue.lines.Count - 1];
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