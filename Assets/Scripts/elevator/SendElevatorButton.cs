using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendElevatorButton : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    [SerializeField] private AudioSource buttonAudioSource; // AudioSource для звука кнопки
    ElevatorMoving platform;
    ElevatorDoorController doorController;
    [Header("Options")]
    [Tooltip("Если true, кнопка будет ждать, пока игрок не окажется внутри лифта, прежде чем он начнёт ехать (используется для внутренних кнопок)")]
    [SerializeField] private bool requirePlayerInsideToStart = false;
    
    

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
        // Воспроизводим звук кнопки через AudioSource
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
        
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
                StartCoroutine(DelayedDoorClose(1f));
                StartCoroutine(DelayedStart(5f)); // Запускаем корутину для задержки старта лифта
            }
        }
    }

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Если для этой кнопки требуется, чтобы игрок находился внутри лифта перед стартом, ждем этого
        if (requirePlayerInsideToStart)
        {
            PlayerInLiftCheck liftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
            if (liftCheck != null)
            {
                while (!liftCheck.IsPlayerInsideElevator())
                {
                    yield return null;
                }
            }
        }

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