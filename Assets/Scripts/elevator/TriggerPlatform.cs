using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlatform : MonoBehaviour
{
    ElevatorMoving platform;
    void Start()
    {
        platform = GetComponent<ElevatorMoving>();
    }
    private void OnTriggerEnter(Collider other)
    {
        platform.canMove = true;
    }
}
