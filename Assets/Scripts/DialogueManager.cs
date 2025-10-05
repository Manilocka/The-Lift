using UnityEditor;
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
    
    public void StartDialogueWithMain(DialogueSO dialogue)
    {
        IsDialogueActive = true;
        mainDialogue = dialogue; // Инициализируем основной диалог
        currentDialogue = dialogue;
        mainDialogueIndex = 0;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
        Debug.Log("started dialog with main reference");
    }
    
    public void StartMainDialogue(DialogueSO dialogue)
    {
        IsDialogueActive = true;
        mainDialogue = dialogue; // Сохраняем основной диалог
        currentDialogue = dialogue;
        mainDialogueIndex = 0;
        currentIndex = 0;
        selectedChoice = 0;
        ui.Show();
        DisplayLine();
        Debug.Log("started main dialog");
    }

    void DisplayLine()
    {
        // Проверяем, что диалог активен и индекс валидный
        if (!IsDialogueActive || currentDialogue == null)
        {
            Debug.LogWarning("Dialogue is not active or dialogue is null");
            return;
        }

        if (currentIndex >= currentDialogue.lines.Count)
        {
            Debug.Log("Dialogue ended - reached end of lines");
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentIndex];
        hasChoices = line.choices.Count > 0;
        
        Debug.Log($"Displaying line {currentIndex}: '{line.text}' (hasChoices: {hasChoices})");
        
        ui.ShowLine(line, OnChoiceSelected);
        
        if (hasChoices)
        {
            UpdateChoiceSelection(); // Обновляем выделение опций
        }
    }

    public void Continue()
    {
        if (!IsDialogueActive || currentDialogue == null)
        {
            Debug.LogWarning("Cannot continue - dialogue is not active");
            return;
        }

        currentIndex++;
        selectedChoice = 0; // Сбрасываем выбор при переходе
        
        // Если это основной диалог, обновляем индекс основного диалога
        if (currentDialogue == mainDialogue)
        {
            mainDialogueIndex = currentIndex;
        }
        
        Debug.Log($"Continuing dialogue to index {currentIndex}");
        DisplayLine();
    }

    void OnChoiceSelected(int choice)
    {
        // Получаем текущую строку диалога
        var currentLine = currentDialogue.lines[currentIndex];
        
        // Проверяем, что выбор существует
        if (choice >= 0 && choice < currentLine.choices.Count)
        {
            var selectedChoice = currentLine.choices[choice];
            
            // Проверяем, есть ли реплики для этого выбора
            if (selectedChoice.dialogueLines != null && selectedChoice.dialogueLines.Count > 0)
            {
                // Если основной диалог не установлен, устанавливаем текущий
                if (mainDialogue == null)
                {
                    mainDialogue = currentDialogue;
                    Debug.Log("Устанавливаем текущий диалог как основной");
                }
                
                // Сохраняем индекс основного диалога
                mainDialogueIndex = currentIndex;
                
                // Создаем временный диалог из реплик выбора
                var tempDialogue = ScriptableObject.CreateInstance<DialogueSO>();
                tempDialogue.lines = selectedChoice.dialogueLines;
                
                Debug.Log($"Показываем {selectedChoice.dialogueLines.Count} реплик для выбора '{selectedChoice.choiceText}'");
                Debug.Log($"Сохраняем индекс основного диалога: {mainDialogueIndex}");
                
                // Запускаем временный диалог
                StartDialogue(tempDialogue);
                
                // Автоматически продолжаем диалог после выбора
                StartCoroutine(ContinueAfterChoice());
            }
            else
            {
                Debug.LogWarning($"Нет реплик для выбора '{selectedChoice.choiceText}'. Завершаем диалог.");
                EndDialogue();
            }
        }
        else
        {
            Debug.LogError($"Неверный индекс выбора: {choice}. Доступно выборов: {currentLine.choices.Count}");
        }
    }
    
    private System.Collections.IEnumerator ContinueAfterChoice()
    {
        Debug.Log("Начинаем ожидание окончания временного диалога...");
        
        // Ждем, пока текущий диалог не закончится
        while (IsDialogueActive)
        {
            yield return null;
        }
        
        Debug.Log("Временный диалог закончился, ждем 0.5 секунды...");
        
        // Небольшая задержка для плавного перехода
        yield return new WaitForSeconds(0.5f);
        
        // Проверяем, что основной диалог существует
        if (mainDialogue == null)
        {
            Debug.LogError("Основной диалог не найден! Завершаем диалог.");
            EndDialogue();
            yield break;
        }
        
        // Возвращаемся к основному диалогу
        currentDialogue = mainDialogue;
        mainDialogueIndex++;
        
        Debug.Log($"Индекс основного диалога: {mainDialogueIndex}, Всего элементов в основном диалоге: {mainDialogue.lines.Count}");
        
        if (mainDialogueIndex < mainDialogue.lines.Count)
        {
            Debug.Log($"Возвращаемся к основному диалогу, переходим к элементу: {mainDialogueIndex}");
            currentIndex = mainDialogueIndex;
            IsDialogueActive = true;
            ui.Show();
            DisplayLine();
        }
        else
        {
            Debug.Log("Основной диалог завершен - больше нет элементов");
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