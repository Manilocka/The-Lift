using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendElevatorButton : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    ElevatorMoving platform;
    ElevatorDoorController doorController;
    

    void Start()
    {
        GameObject elevator = GameObject.FindGameObjectWithTag("Elevator");
        platform = elevator.GetComponent<ElevatorMoving>();
        doorController = elevator.GetComponentInChildren<ElevatorDoorController>();
    }

    void Update()
    {
        // Проверяем нажатие левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Проверяем, попал ли луч в этот триггер
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                if (platform != null && !platform.canMove)
                {
                    // Проверяем, не находится ли лифт уже на этом этаже
                    if (platform.startStop == floorNumber)
                    {
                        // Лифт уже на нужном этаже - просто открываем двери
                        if (doorController != null && !doorController.isOpen)
                        {
                            doorController.OpenDoors();
                        }
                    }
                    else
                    {
                        // Лифт на другом этаже - устанавливаем конечную точку и запускаем движение
                        platform.endStop = floorNumber;
                        StartCoroutine(DelayedDoorClose(2f));
                        StartCoroutine(DelayedStart(7f)); // Запускаем корутину для задержки старта лифта
                    }
                }
            }
        }
    }

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (platform != null)
        {
            platform.canMove = true; // Запускаем движение лифта после задержки
        }
    }
    private IEnumerator DelayedDoorClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (doorController != null)
        {
            doorController.CloseDoors(); // Закрываем двери после задержки
        }
    }
}