using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlatform : MonoBehaviour
{
    ElevaorMoving platform;
    void Start()
    {
        platform = GetComponent<ElevaorMoving>();
    }
    private void OnTriggerEnter(Collider other)
    {
        platform.canMove = true;
    }
}
