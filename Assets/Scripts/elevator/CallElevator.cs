using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallElevator : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    [SerializeField] float maxDistance = 5f; // Максимальное расстояние активации (в единицах Unity)
    [SerializeField] private AudioSource buttonAudioSource; // AudioSource для звука кнопки
    ElevatorMoving platform;
    

    

    void Start()
    {
        GameObject elevator = GameObject.FindGameObjectWithTag("Elevator");
        platform = elevator.GetComponent<ElevatorMoving>();
    }

    void Update()
    {
        // Проверяем нажатие левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Проверяем, попал ли луч в этот триггер.
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                OnMouseDown();
                Debug.Log("Elevator call button pressed on floor" + floorNumber);
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
        if (!AdditionalCheck.CanCallElevator()){return;}
        
        // Проверяем расстояние от камеры до триггера
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distance <= maxDistance && platform != null)
        {
            // Если лифт уже на нужном этаже и не движется, просто открываем двери
            if (!platform.canMove && platform.startStop == floorNumber)
            {
                ElevatorDoorController doorController = platform.GetComponentInChildren<ElevatorDoorController>();
                if (doorController != null && !doorController.isOpen)
                {
                    Debug.Log($"CallElevator: Лифт уже на этаже {floorNumber}, открываем двери");
                    doorController.OpenDoors();
                }
            }
            else if (!platform.canMove)
            {
                // Устанавливаем конечную точку (этаж) и включаем движение
                platform.endStop = floorNumber;
                platform.canMove = true;
            }
        }
    }
}