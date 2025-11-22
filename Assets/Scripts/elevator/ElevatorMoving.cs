using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorMoving : MonoBehaviour
{
    public bool canMove; // Переменная для включения/выключения движения
    [SerializeField] float speed = 120f; // Скорость движения лифта
    [SerializeField] public Transform[] stops; // Массив точек остановки
    [SerializeField] public int startStop; // Начальная точка
    [SerializeField] public int endStop; // Конечная точка
    [SerializeField] public int midStop = -10;
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueSO dialogueForMidStop; // Диалог для запуска при остановке на middlestop (например, для day6_e)
    

    ElevatorDoorController doorController;

    private Rigidbody rb;
    public int currentTarget;
    private bool reverse;
    public int rememberStartStop; // Переменная для запоминания начальной точки
    public int rememberEndStop;
    private bool wasMoving = false; // Флаг для отслеживания изменения состояния движения
    private bool waitForDoorsClosed = false; // Флаг, если мы закрываем двери перед стартом
    private bool midStopHandled = false; // Флаг, что midStop уже обработан

    void Start()
    {
        doorController = GetComponentInChildren<ElevatorDoorController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физическую гравитацию и включаем кинематический режим
        rb.useGravity = false; // Отключаем гравитацию
        
        // СБРОС СОСТОЯНИЯ ПРИ ЗАГРУЗКЕ СЦЕНЫ - предотвращает наследование состояния из предыдущей сцены
        canMove = false;
        waitForDoorsClosed = false;
        wasMoving = false;
        midStopHandled = false;
        
        transform.position = stops[startStop].position; // Устанавливаем начальную позицию
        currentTarget = startStop;
        rememberEndStop = endStop;
        rememberStartStop = startStop;
        UpdateDirection();
        
        // Останавливаем звуки лифта при загрузке сцены
        if (ElevatorAudioManager.Instance != null)
        {
            ElevatorAudioManager.Instance.StopElevatorMovement();
        }
    }

    void FixedUpdate() // Используем FixedUpdate для физики
    {
        // Управление звуком движения лифта через менеджер (только при изменении состояния)
        if (canMove != wasMoving)
        {
            if (ElevatorAudioManager.Instance != null)
            {
                if (canMove)
                {
                    // Лифт начал движение - запускаем звуки движения
                    ElevatorAudioManager.Instance.StartElevatorMovement();
                }
                else
                {
                    // Лифт остановился - останавливаем звуки движения
                    ElevatorAudioManager.Instance.StopElevatorMovement();
                }
            }
            wasMoving = canMove;
        }
        
        // Если движение запросили, но двери открыты — закроем их сначала и подождём
        if (canMove && !waitForDoorsClosed && doorController != null && !doorController.AreDoorsFullyClosed())
        {
            Debug.Log("ElevatorMoving: Movement requested while doors open. Closing doors before moving.");
            doorController.CloseDoors();
            canMove = false;
            waitForDoorsClosed = true;
        }
        
        // Если мы в процессе ожидания закрытия дверей — проверяем, закрылись ли они
        if (waitForDoorsClosed)
        {
            
            if (doorController != null && doorController.AreDoorsFullyClosed())
            {
                // Двери закрыты — разрешаем движение
                waitForDoorsClosed = false;
                canMove = true;
                Debug.Log("ElevatorMoving: Двери закрыты, продолжаем движение");

                // Если за время открытых дверей сменили конечную точку — обновим текущую цель
                if (currentTarget == startStop && startStop != endStop)
                {
                    currentTarget = endStop;
                    UpdateDirection();
                    Debug.Log($"ElevatorMoving: Обновлена цель после закрытия дверей: currentTarget = {currentTarget}, endStop = {endStop}");
                }
            }
            else
            {
                // Ждем, не выполняем движение
                return;
            }
        }

        // Проверяем, можно ли двигаться
        if (!canMove) return;
        
        // ИСПРАВЛЕНИЕ: Обрабатываем midStop только один раз в начале движения
        // Это предотвращает лишние поездки (сначала на первый этаж, потом за NPC, потом опять на первый)
        if (!midStopHandled && midStop >= 0 && 
            midStop > Mathf.Min(startStop, endStop) && 
            midStop < Mathf.Max(startStop, endStop))
        {
            // Сохраняем конечную цель и устанавливаем промежуточную
            rememberEndStop = endStop;
            endStop = midStop;
            midStopHandled = true;
            Debug.Log($"ElevatorMoving: Установлен midStop {midStop}, конечная цель {rememberEndStop}");
        }
        
        // Если мы на стартовой позиции и нужно ехать к конечной, обновляем цель
        if (currentTarget == startStop && startStop != endStop)
        {
            currentTarget = endStop;
            UpdateDirection();
            Debug.Log($"Updated currentTarget from {startStop} to {endStop}");
        }
        
        if (Vector3.Distance(transform.position, stops[currentTarget].position) < 0.1f) // Проверка достижения точки
        {
            if (currentTarget == endStop)
            {
                Debug.Log($"Elevator reached target floor {endStop}");
                currentTarget = endStop; // Останавливаемся на конечной точке
                canMove = false;
                rememberStartStop = startStop; 
                startStop = endStop;

                // Воспроизводим звук звоночка при прибытии
                if (ElevatorAudioManager.Instance != null)
                {
                    ElevatorAudioManager.Instance.PlayArrivalSound();
                }

                // Если мы достигли midStop (промежуточная остановка), открыть двери, подождать 3с, закрыть и продолжить до rememberEndStop
                if (midStop == currentTarget)
                {
                    StartCoroutine(HandleMidStopAndContinue());
                    return;
                }

                // Обычная остановка на конечном этаже: открыть двери и остаться ждать команды
                endStop = rememberEndStop;
                Debug.Log($"After reaching target: startStop = {startStop}, endStop = {endStop}");
                doorController.OpenDoors(); // Открываем двери
                return;
            }
            if (reverse)
            {
                currentTarget--;
                if (currentTarget < startStop) currentTarget = startStop; // Возвращаемся к началу
            }
            else
            {
                currentTarget++;
                if (currentTarget > endStop) currentTarget = endStop; // Останавливаемся на конце
            }
        }

        // Движение к текущей цели
        Vector3 targetPosition = stops[currentTarget].position;
        rb.MovePosition(Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    private void UpdateDirection()
    {
        if (endStop > currentTarget)
        {
            reverse = false;
        }
        else if (endStop < currentTarget)
        {
            reverse = true;
        }
    }

    private System.Collections.IEnumerator HandleMidStopAndContinue()
    {
        string scene = SceneManager.GetActiveScene().name;

        // Открываем двери
        if (doorController != null)
        {
            doorController.OpenDoors();
        }

        if (scene == "day6_e")
        {
            Debug.Log("Day6_e: Запускаем диалог при остановке на middlestop...");
            
            // Запускаем диалог, если он назначен
            if (dialogueForMidStop != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueForMidStop);
                Debug.Log($"Day6_e: Диалог '{dialogueForMidStop.name}' запущен");
                
                // Ждем завершения диалога
                while (!AdditionalCheck.IsDialogueDone())
                {
                    yield return null;
                }
                
                Debug.Log("Day6_e: Диалог завершен, ждем 3 секунды перед закрытием дверей");
            }
            else
            {
                Debug.LogWarning("Day6_e: Диалог не назначен или DialogueManager не найден!");
            }
            
            // Ждем 3 секунды перед закрытием дверей
            yield return new WaitForSeconds(3f);
        }
        else if (scene == "day7_e")
        {
            Debug.Log("Waiting for dialogue to complete in day7_e...");
        
            // Ждем завершения диалога
            while (!AdditionalCheck.IsDialogueDone())
            {
                yield return null;
            }

            Debug.Log("Dialogue completed in day7_e, waiting 3 seconds before closing doors");
        
            // Ждем 3 секунды перед закрытием дверей
            yield return new WaitForSeconds(3f);
        }
        else
        {
            // Для других сцен просто ждем 3 секунды
            yield return new WaitForSeconds(3f);
        }
    
        // Закрываем двери
        if (doorController != null)
        {
            doorController.CloseDoors();
        }

        // Ждем, пока двери фактически закроются
        while (doorController != null && !doorController.AreDoorsFullyClosed())
        {
            yield return null;
        }

        // Сбрасываем промежуточную остановку и продолжаем движение до rememberEndStop
        midStop = -10;
        midStopHandled = false; // Сбрасываем флаг для следующего использования
        endStop = rememberEndStop;
        currentTarget = endStop; // Устанавливаем цель как конечную остановку, а не стартовую
        UpdateDirection();
        canMove = true;
        Debug.Log($"ElevatorMoving: Продолжаем движение от midStop к конечной цели {endStop}");
    }
}