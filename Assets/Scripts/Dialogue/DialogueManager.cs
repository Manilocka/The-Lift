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
    public NPCDialogue currentNPCDialogue { get; private set; } // Текущий NPC, с которым идет диалог
    // Camera focus/restore
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private bool cameraMoved = false;
    [Header("Camera Dialogue Settings")]
    [Tooltip("Скорость перехода камеры к точке фокуса (если 0 - мгновенно)")]
    public float cameraTransitionSpeed = 8f;
    [Tooltip("Плавный переход камеры при фокусировке")]
    public bool smoothCameraTransition = true;
    
    /// <summary>
    /// Устанавливает текущий NPC для диалога
    /// </summary>
    public void SetCurrentNPC(NPCDialogue npc)
    {
        currentNPCDialogue = npc;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerLiftCheck = FindObjectOfType<PlayerInLiftCheck>();
        elevator = FindObjectOfType<ElevatorMoving>();
        elevatorDoorController = FindObjectOfType<ElevatorDoorController>();
    }

    // Backward-compatible StartDialogue: no NPC
    public void StartDialogue(DialogueSO dialogue)
    {
        StartDialogue(dialogue, null);
    }

    // New overload: Start dialogue and optionally associate it with an NPC.
    public void StartDialogue(DialogueSO dialogue, NPCDialogue npc)
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

        // Если передан NPC — сохраняем его и блокируем управление/фокусируем камеру
        if (npc != null)
        {
            SetCurrentNPC(npc);

            MouseLock mouseLock = FindObjectOfType<MouseLock>();
            if (mouseLock != null)
            {
                mouseLock.canLook = false;
                if (mouseLock.crosshair != null) mouseLock.crosshair.gameObject.SetActive(false);
            }

            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.canMove = false;
            }

            TryFocusCameraOnNPC(npc);
        }

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
            // Восстанавливаем камеру только если диалог был с NPC
            if (currentNPCDialogue != null)
            {
                mouseLock.canLook = true;
                mouseLock.crosshair.gameObject.SetActive(true);
            }
        }
        // Восстанавливаем позицию камеры, если мы её меняли
        if (cameraMoved)
        {
            bool insideElevator = (playerLiftCheck != null && playerLiftCheck.IsPlayerInsideElevator());
            bool elevatorIsMoving = (elevator != null && elevator.canMove);

            // Если игрок внутри движущегося лифта — не возвращаем старую позицию камеры,
            // иначе восстанавливаем сохранённую позицию.
            if (insideElevator && elevatorIsMoving)
            {
                Debug.Log("Диалог завершён, но лифт движется — позиция камеры не восстанавливается, только разблокировка камеры");
                // Просто помечаем, что мы больше не держим сохранённую позицию
                cameraMoved = false;
            }
            else
            {
                RestoreSavedCamera();
            }
        }

        // Сбрасываем текущий NPC
        currentNPCDialogue = null;
        
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
        
        // Для сцены Final запускаем проверку перехода через корутину с небольшой задержкой
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName == "Final" && currentDialogue != null && currentDialogue.name == "Final")
        {
            StartCoroutine(CheckFinalSceneTransitionAfterDelay());
        }
        else
        {
            CheckForSceneTransition();
        }

        Debug.Log("Диалог полностью завершен, IsDialogueActive = false");
    }
    
    private IEnumerator CheckFinalSceneTransitionAfterDelay()
    {
        // Ждем небольшой задержки, чтобы убедиться, что все состояния обновлены
        yield return new WaitForSeconds(0.1f);
        CheckForSceneTransition();
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
        // Проверка для сцены day7_fin - переход на Final после поимки игрока
        else if (currentSceneName == "day7_fin")
        {
            // Переход на Final происходит через NPCCatchesPlayer, но можно добавить проверку здесь если нужно
        }
        // Проверка для сцены Final - переход на главное меню после завершения диалога Final
        else if (currentSceneName == "Final")
        {
            // Проверяем, что диалог завершился и это диалог Final
            // Для Final сцены проверяем просто завершение диалога, а не DialogueDone
            if (currentDialogue != null && currentDialogue.name == "Final" && !IsDialogueActive)
            {
                string sceneToLoad = "Game menu";
                
                Debug.Log($"Переход на главное меню из Final после завершения диалога {currentDialogue.name}");
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
    
    // Методы для получения текущего диалога и индекса (для внешних скриптов)
    public DialogueSO GetCurrentDialogue()
    {
        return currentDialogue;
    }
    
    public int GetCurrentIndex()
    {
        return currentIndex;
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

    private void TryFocusCameraOnNPC(NPCDialogue npc)
    {
        if (npc == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        // Сохраняем текущую позицию камеры
        savedCameraPosition = cam.transform.position;
        savedCameraRotation = cam.transform.rotation;
        cameraMoved = true;

        // Определяем точку фокуса
        Vector3 focusPos = (npc.cameraFocusPoint != null) ? npc.cameraFocusPoint.position : npc.transform.position;
        if (Mathf.Abs(npc.cameraHeightY) > Mathf.Epsilon)
        {
            focusPos.y = npc.cameraHeightY;
        }
        else
        {
            focusPos.y += npc.cameraHeightOffset;
        }

        // Вычисляем направление от фокуса к текущей камере (чтобы сохранить ориентацию)
        Vector3 dir = (cam.transform.position - focusPos).normalized;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = -npc.transform.forward;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.back;
        }

        Vector3 targetPos = focusPos + dir * npc.cameraDistance;
        Quaternion targetRot = Quaternion.LookRotation(focusPos - targetPos);

        if (smoothCameraTransition && cameraTransitionSpeed > 0f)
        {
            StartCoroutine(SmoothMoveCamera(cam.transform, savedCameraPosition, savedCameraRotation, targetPos, targetRot));
        }
        else
        {
            cam.transform.position = targetPos;
            cam.transform.rotation = targetRot;
        }
    }

    private void RestoreSavedCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (smoothCameraTransition && cameraTransitionSpeed > 0f)
        {
            StartCoroutine(SmoothMoveCamera(cam.transform, cam.transform.position, cam.transform.rotation, savedCameraPosition, savedCameraRotation, true));
        }
        else
        {
            cam.transform.position = savedCameraPosition;
            cam.transform.rotation = savedCameraRotation;
            cameraMoved = false;
        }
    }

    private IEnumerator SmoothMoveCamera(Transform camTransform, Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot, bool clearAfter = false)
    {
        float t = 0f;
        float speed = Mathf.Max(0.0001f, cameraTransitionSpeed);
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            camTransform.position = Vector3.Lerp(fromPos, toPos, Mathf.SmoothStep(0f, 1f, t));
            camTransform.rotation = Quaternion.Slerp(fromRot, toRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        camTransform.position = toPos;
        camTransform.rotation = toRot;
        if (clearAfter) cameraMoved = false;
    }
}