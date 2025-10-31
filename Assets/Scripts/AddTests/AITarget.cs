using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITarget : MonoBehaviour
{
    public Transform Target;
    
    private NavMeshAgent m_Agent;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (AdditionalCheck.CanNPCMove())
        {
            // Просто постоянно следует за целью
            m_Agent.destination = Target.position;
        }

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            transform.SetParent(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            transform.SetParent(null);
        }
    }
}