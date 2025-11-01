using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        elevator = FindObjectOfType<ElevatorMoving>();
        npcLiftCheck = FindObjectOfType<NPCInLiftCheck>();
        // Начинаем движение к первой цели
        currentTarget = Target;
        if (elevator != null)
        {
            lastElevatorPosition = elevator.transform.position;
        }
    }

    void Update()
    {
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
            // Когда лифт не движется, включаем обратно NavMeshAgent
            if (wasAgentEnabled && m_Agent != null && !m_Agent.enabled)
            {
                m_Agent.enabled = true;
                
                // Синхронизируем позицию агента с фактической позицией объекта
                m_Agent.Warp(transform.position);
                
                // Обновляем destination
                if (currentTarget != null)
                {
                    m_Agent.destination = currentTarget.position;
                }
                
                wasAgentEnabled = false;
                Debug.Log("NavMeshAgent重新激活, позиция синхронизирована");
            }
            
            // Если лифт остановился и NPC был в лифте
            if (!elevatorMoving && isInElevator && hasElevatorMoved)
            {
                Debug.Log("Лифт остановился, NPC выходит");
                isInElevator = false;
                
                // Переключаем на вторую цель, если еще не переключились
                if (currentTarget == Target && Target2 != null && !hasReachedTarget1)
                {
                    currentTarget = Target2;
                    hasReachedTarget1 = true;
                    Debug.Log("Переключаемся на вторую цель: " + Target2.name);
                    
                    // Обновляем destination
                    if (m_Agent != null && m_Agent.enabled)
                    {
                        m_Agent.destination = currentTarget.position;
                    }
                }
            }
            
            if (elevator != null)
            {
                lastElevatorPosition = elevator.transform.position;
            }
        }

        // Основная логика движения
        if (AdditionalCheck.CanNPCMove() && m_Agent != null && m_Agent.enabled && currentTarget != null)
        {
            // Обновляем destination только если цель изменилась
            if (Vector3.Distance(m_Agent.destination, currentTarget.position) > 0.1f)
            {
                m_Agent.destination = currentTarget.position;
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
            }
        }
    }

    // Метод для принудительного переключения на вторую цель
    public void SwitchToTarget2()
    {
        if (Target2 != null && !hasReachedTarget1)
        {
            currentTarget = Target2;
            hasReachedTarget1 = true;
            if (m_Agent != null && m_Agent.enabled)
            {
                m_Agent.destination = currentTarget.position;
            }
            Debug.Log("Принудительно переключено на вторую цель");
        }
    }

    // Метод для проверки текущего состояния
    public string GetCurrentState()
    {
        return $"Current Target: {currentTarget?.name}, In Elevator: {isInElevator}, Elevator Moved: {hasElevatorMoved}, Reached Target1: {hasReachedTarget1}";
    }
}