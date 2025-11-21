using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SendElevatorNew : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    [SerializeField] private AudioSource buttonAudioSource; // AudioSource для звука кнопки
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
        if (scene == "day7_fin")
        {
            // В финальной сцене управление лифтом перехватывается специальной логикой
            Debug.Log("SendElevatorNew: управление отключено в day7_fin");
            return;
        }

        // Воспроизводим звук кнопки через AudioSource
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
        
        if (platform != null && !platform.canMove)
        {
            // Если лифт уже на этом этаже - просто открываем двери и не запускаем движение
            if (platform.startStop == floorNumber)
            {
                if (doorController != null && !doorController.isOpen)
                {
                    doorController.OpenDoors();
                }
                return;
            }
            
            if (scene == "day4_e")
            {
                platform.endStop = 1;
            }
            if (scene == "day7_e")
            {
                platform.endStop = 2;
            }
            else
            {
                // Проверяем, не находится ли лифт уже на этом этаже
                if (platform.startStop == 3)
                {
                    platform.endStop = 0;
                }
                else if (scene == "day4_e")
                {
                    // Лифт на другом этаже - устанавливаем конечную точку и запускаем движение
                    platform.endStop = 1;
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

        // Ждем, пока игрок не окажется внутри лифта.
        // Это предотвращает ситуацию: игрок нажал кнопку внутри, вышел, и лифт уехал без него.
        PlayerInLiftCheck liftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
        if (liftCheck != null)
        {
            // Если игрок уже внутри — сразу стартуем
            if (!liftCheck.IsPlayerInsideElevator())
            {
                Debug.Log("SendElevatorNew: Ждем пока игрок зайдет в лифт перед стартом движения...");
                // Ждем неопределённое время — пока игрок не войдёт
                while (!liftCheck.IsPlayerInsideElevator())
                {
                    yield return null;
                }
            }
        }

        if (platform != null)
        {
            platform.canMove = true; // Запускаем движение лифта после задержки (и после входа игрока, если требуется)
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