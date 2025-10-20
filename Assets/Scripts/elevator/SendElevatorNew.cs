using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SendElevatorNew : MonoBehaviour
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
                OnMouseDown();
            }
        }
    }
    
    void OnMouseDown()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (platform != null && !platform.canMove)
        {
            
            if (scene == "day4_e")
            {
                platform.endStop = 1;
            }
            else
            {
                // Проверяем, не находится ли лифт уже на этом этаже
                if (platform.startStop == 3)
                {
                    platform.endStop = 0;
                }
                else
                {
                    // Лифт на другом этаже - устанавливаем конечную точку и запускаем движение
                    platform.endStop = 3;
                }
            }
            StartCoroutine(DelayedDoorClose(1f));
            StartCoroutine(DelayedStart(5f)); // Запускаем корутину для задержки старта лифта
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