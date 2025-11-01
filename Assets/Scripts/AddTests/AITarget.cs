using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITarget : MonoBehaviour
{
    public Transform Target;
    
    private NavMeshAgent m_Agent;
    private Vector3 lastElevatorPosition;
    private ElevatorMoving elevator;
    private NPCInLiftCheck npcLiftCheck;
    
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        elevator = FindObjectOfType<ElevatorMoving>();
        npcLiftCheck = FindObjectOfType<NPCInLiftCheck>();
        if (elevator != null)
        {
            lastElevatorPosition = elevator.transform.position;
        }
    }

    void Update()
    {
        // Проверяем, находится ли лифт в движении И NPC внутри лифта
        bool elevatorMoving = elevator != null && elevator.canMove;
        bool npcInElevator = npcLiftCheck != null && npcLiftCheck.IsNPCInsideElevator();
        
        if (elevatorMoving && npcInElevator)
        {
            // Вычисляем, насколько сдвинулся лифт
            Vector3 elevatorDelta = elevator.transform.position - lastElevatorPosition;
            
            // Применяем это смещение к NPC
            transform.position += elevatorDelta;
            
            // Отключаем обновление позиции NavMeshAgent, так как мы управляем вручную
            if (m_Agent != null)
            {
                m_Agent.updatePosition = false;
                m_Agent.updateRotation = false;
            }
            
            lastElevatorPosition = elevator.transform.position;
        }
        else
        {
            // Когда лифт не движется, включаем обратно NavMeshAgent
            if (m_Agent != null)
            {
                m_Agent.updatePosition = true;
                m_Agent.updateRotation = true;
            }
            
            if (elevator != null)
            {
                lastElevatorPosition = elevator.transform.position;
            }
        }

        if (AdditionalCheck.CanNPCMove() && m_Agent != null)
        {
            // Просто постоянно следует за целью
            m_Agent.destination = Target.position;
        }
    }

}