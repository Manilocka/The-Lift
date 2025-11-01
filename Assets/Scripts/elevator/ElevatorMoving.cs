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
    

    ElevatorDoorController doorController;

    private Rigidbody rb;
    public int currentTarget;
    private bool reverse;
    public int rememberStartStop; // Переменная для запоминания начальной точки
    public int rememberEndStop;
    private bool wasMoving = false; // Флаг для отслеживания изменения состояния движения

    void Start()
    {
        doorController = GetComponentInChildren<ElevatorDoorController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физическую гравитацию и включаем кинематический режим
        rb.useGravity = false; // Отключаем гравитацию
        transform.position = stops[startStop].position; // Устанавливаем начальную позицию
        currentTarget = startStop;
        rememberEndStop = endStop;
        UpdateDirection();
        
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
        
        if (!canMove) return; // Проверяем, можно ли двигаться
        
        // Отладка состояния лифта
        if (canMove)
        {
            // Debug.Log($"Elevator moving: startStop = {startStop}, endStop = {endStop}, currentTarget = {currentTarget}");
        }
        if (midStop > Mathf.Min(startStop, endStop) && 
            midStop < Mathf.Max(startStop, endStop))
        {
            // Едем сначала до промежуточной остановки
            endStop = midStop;
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

        if (scene == "day7_e")
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
        endStop = rememberEndStop;
        currentTarget = endStop; // Устанавливаем цель как конечную остановку, а не стартовую
        UpdateDirection();
        canMove = true;
    

    }
}