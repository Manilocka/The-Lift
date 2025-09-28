using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallElevator : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    ElevatorMoving platform;

    // Start is called before the first frame update
    void Start()
    {
        GameObject elevator = GameObject.FindGameObjectWithTag("Elevator");
        platform = elevator.GetComponent<ElevatorMoving>();
    }


    void OnTriggerStay(Collider other)
    {
        // Проверяем, что объект, находящийся в триггере, — это игрок
        if (other.CompareTag("Player"))
        {
            // Проверяем, была ли нажата левая кнопка мыши
            if (Input.GetMouseButtonDown(0))
            {
                if (platform != null)
                {
                    // Устанавливаем конечную точку (этаж) и включаем движение
                    if (platform.canMove == false)
                    {
                        platform.endStop = floorNumber;
                        platform.canMove = true;
                    }
                    
                }
            }
        }
    }
}
