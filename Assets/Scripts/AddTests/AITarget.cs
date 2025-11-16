using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AITarget : MonoBehaviour
{
    public Transform Target;
    public Transform Target2; // вторая цель (опционально)
    private NavMeshAgent m_Agent;
    private Vector3 lastElevatorPosition;
    private ElevatorMoving elevator;
    private NPCInLiftCheck npcLiftCheck;
    private Transform currentTarget; // текущая цель
    private bool hasEnteredElevator = false; // флаг входа в лифт
    private bool hasElevatorMoved = false; // флаг движения лифта после входа NPC
    private bool hasReachedTarget1 = false; // флаг достижения первой цели
    private bool isInElevator = false; // флаг нахождения в лифте
    private bool wasAgentEnabled = false; // был ли агент активен до входа в лифт
    
    // Для преследования игрока
    private Transform playerTransform;
    private bool shouldChasePlayer = false;
    private GameObject playerTargetObject; // временный объект для хранения позиции игрока
    
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        elevator = FindObjectOfType<ElevatorMoving>();
        npcLiftCheck = FindObjectOfType<NPCInLiftCheck>();
        
        // Проверяем, нужно ли преследовать игрока (в сцене day7_fin)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "day7_fin")
        {
            shouldChasePlayer = true;
            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                // Создаем временный объект для хранения позиции игрока
                playerTargetObject = new GameObject("PlayerTarget");
                playerTargetObject.transform.position = playerTransform.position;
                currentTarget = playerTargetObject.transform;
                StartCoroutine(StartChaseAfterDelay(2f));
            }
            else
            {
                Debug.LogError("AITarget: Игрок с тегом 'Player' не найден в сцене day7_fin!");
                shouldChasePlayer = false;
                currentTarget = Target;
            }
        }
        else
        {
            // Начинаем движение к первой цели
            currentTarget = Target;
        }
        
        if (elevator != null)
        {
            lastElevatorPosition = elevator.transform.position;
        }
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // Проверяем, находится ли NPC в лифте
        bool npcInElevator = npcLiftCheck != null && npcLiftCheck.IsNPCInsideElevator();
        
        // Если NPC впервые зашел в лифт, включаем флаг
        if (npcInElevator && !hasEnteredElevator)
        {
            hasEnteredElevator = true;
            isInElevator = true;
            Debug.Log("NPC впервые вошел в лифт");
        }
        
        // Проверяем, находится ли лифт в движении
        bool elevatorMoving = elevator != null && elevator.canMove;
        
        if (elevatorMoving && npcInElevator)
        {
            if (!wasAgentEnabled)
            {
                // Сохраняем состояние перед отключением
                wasAgentEnabled = true;
            }
            
            // Вычисляем, насколько сдвинулся лифт
            Vector3 elevatorDelta = elevator.transform.position - lastElevatorPosition;
            
            // Применяем это смещение к NPC
            transform.position += elevatorDelta;
            
            // Синхронизируем позицию NavMeshAgent с фактической позицией
            if (m_Agent != null)
            {
                m_Agent.enabled = false;
            }
            
            // Если NPC уже был в лифте и лифт сдвинулся, отмечаем что лифт поехал
            if (hasEnteredElevator && elevatorDelta.magnitude > 0.01f && !hasElevatorMoved)
            {
                hasElevatorMoved = true;
                Debug.Log("Лифт начал движение с NPC внутри");
            }
            
            lastElevatorPosition = elevator.transform.position;
        }
        else
        {
            // Проверяем, должен ли NPC ждать выхода из лифта
            bool shouldWaitForExit = isInElevator && hasElevatorMoved && !AdditionalCheck.CanNPCMove();
            
            // Когда лифт не движется, включаем обратно NavMeshAgent
            // НО только если NPC не должен ждать выхода из лифта
            // И только если NPC не находится в процессе выхода из лифта
            if (wasAgentEnabled && m_Agent != null && !m_Agent.enabled && !shouldWaitForExit && !isInElevator)
            {
                m_Agent.enabled = true;
                
                // Синхронизируем позицию агента с фактической позицией объекта
                m_Agent.Warp(transform.position);
                
                // Обновляем destination - используем текущую цель (может быть уже Target2)
                if (currentTarget != null)
                {
                    m_Agent.destination = currentTarget.position;
                    Debug.Log("NavMeshAgent重新激活, destination установлен на: " + currentTarget.name);
                }
                
                wasAgentEnabled = false;
                Debug.Log("NavMeshAgent重新激活, позиция синхронизирована");
            }
            // Если NPC должен ждать выхода, отключаем агент
            else if (shouldWaitForExit && m_Agent != null && m_Agent.enabled)
            {
                m_Agent.enabled = false;
                Debug.Log("NPC ждет выхода из лифта - NavMeshAgent отключен");
            }
            
            // Если лифт остановился и NPC был в лифте, проверяем, можно ли выходить
            if (!elevatorMoving && isInElevator && hasElevatorMoved)
            {
                // Для выхода NPC из лифта нужно, чтобы двери были открыты и диалог завершен
                // Это проверяется в AdditionalCheck.CanNPCMove()
                if (AdditionalCheck.CanNPCMove())
                {
                    Debug.Log("Лифт остановился, двери открыты, диалог завершен - NPC выходит");
                    isInElevator = false;
                    
                    // Переключаем на вторую цель ПЕРЕД включением NavMeshAgent
                    // Это важно, чтобы NPC сразу пошел к правильной цели
                    if (Target2 != null && !hasReachedTarget1)
                    {
                        currentTarget = Target2;
                        hasReachedTarget1 = true;
                        Debug.Log("Переключаемся на вторую цель при выходе из лифта: " + Target2.name);
                    }
                    // Если Target2 не назначен, оставляем текущую цель как есть
                    
                    // Включаем NavMeshAgent если он был отключен
                    if (m_Agent != null && !m_Agent.enabled)
                    {
                        m_Agent.enabled = true;
                        m_Agent.Warp(transform.position);
                        
                        // Устанавливаем destination на текущую цель (которая уже может быть Target2)
                        if (currentTarget != null)
                        {
                            m_Agent.destination = currentTarget.position;
                            Debug.Log("NavMeshAgent включен, destination установлен на: " + currentTarget.name);
                        }
                    }
                    else if (m_Agent != null && m_Agent.enabled && currentTarget != null)
                    {
                        // Обновляем destination если агент уже включен
                        m_Agent.destination = currentTarget.position;
                        Debug.Log("NavMeshAgent уже включен, destination обновлен на: " + currentTarget.name);
                    }
                }
                else
                {
                    Debug.Log("Лифт остановился, но двери еще не открыты или диалог не завершен - NPC ждет");
                }
            }
            
            if (elevator != null)
            {
                lastElevatorPosition = elevator.transform.position;
            }
        }

        // Если нужно преследовать игрока, обновляем цель
        if (shouldChasePlayer && playerTransform != null && currentTarget != null)
        {
            currentTarget.position = playerTransform.position;
        }
        
        // Основная логика движения
        if (AdditionalCheck.CanNPCMove() && m_Agent != null && m_Agent.enabled && currentTarget != null)
        {
            // Обновляем destination только если цель изменилась (или если преследуем игрока - всегда обновляем)
            if (shouldChasePlayer || Vector3.Distance(m_Agent.destination, currentTarget.position) > 0.1f)
            {
                m_Agent.destination = currentTarget.position;
            }
            
            // Если преследуем игрока, не проверяем достижение цели
            if (shouldChasePlayer)
            {
                return;
            }
            
            // Проверяем, достиг ли NPC текущей цели
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance < m_Agent.stoppingDistance + 0.5f)
            {
                Debug.Log("Достигнута целевая точка " + currentTarget.name + ", расстояние: " + distance);
                
                // Логика переключения на вторую цель
                if (currentTarget == Target && !hasReachedTarget1 && Target2 != null && hasElevatorMoved)
                {
                    Debug.Log("Переключаемся на вторую цель после движения лифта");
                    currentTarget = Target2;
                    hasReachedTarget1 = true;
                    
                    if (m_Agent.enabled)
                    {
                        m_Agent.destination = currentTarget.position;
                    }
                }
                else if (currentScene != "day7_fin" && currentTarget == Target2)
                    // IsLastTargetReached()
                {
                    // Достигли конечной цели - исчезаем
                    Debug.Log("NPC достиг конечной точки - исчезает");
                    HideNPC();
                }
                
            }
        }
    }

    // Метод для принудительного переключения на вторую цель
    // public void SwitchToTarget2()
    // {
    //     if (Target2 != null && !hasReachedTarget1)
    //     {
    //         currentTarget = Target2;
    //         hasReachedTarget1 = true;
    //         if (m_Agent != null && m_Agent.enabled)
    //         {
    //             m_Agent.destination = currentTarget.position;
    //         }
    //         Debug.Log("Принудительно переключено на вторую цель");
    //     }
    //     else
    //     {
    //         // Достигли конечной цели - исчезаем
    //         Debug.Log("NPC достиг конечной точки - исчезает");
    //         HideNPC();
    //     }
    // }

    // Метод для проверки текущего состояния
    public string GetCurrentState()
    {
        return $"Current Target: {currentTarget?.name}, In Elevator: {isInElevator}, Elevator Moved: {hasElevatorMoved}, Reached Target1: {hasReachedTarget1}";
    }
    
    void OnDestroy()
    {
        // Удаляем временный объект цели при уничтожении компонента
        if (playerTargetObject != null)
        {
            Destroy(playerTargetObject);
        }
    }
    private IEnumerator StartChaseAfterDelay(float delay)
    {
        // Выключаем агент, чтобы не двигался сразу
        if (m_Agent != null)
            m_Agent.enabled = false;

        yield return new WaitForSeconds(delay);

        // После задержки — включаем агент и начинаем движение
        if (m_Agent != null)
        {
            m_Agent.enabled = true;
            if (currentTarget != null)
            {
                m_Agent.destination = currentTarget.position;
            }
        }

        Debug.Log("NPC начал преследовать игрока через " + delay + " секунд.");
    }
    private void HideNPC()
    {
        Debug.Log("NPC исчезает: " + gameObject.name);
        
        // Отключаем NavMeshAgent
        if (m_Agent != null)
        {
            m_Agent.enabled = false;
        }
        
        // Отключаем игровой объект
        gameObject.SetActive(false);
    }
    private bool IsLastTargetReached()
    {
        if (Target2 != null && currentTarget == Target2)
        {
            return true;
        }
        else if (Target2 == null && currentTarget == Target)
        {
            return true;
        }
        return false;
    }
}