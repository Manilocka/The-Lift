using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallElevator : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    [SerializeField] float maxDistance = 5f; // Максимальное расстояние активации (в единицах Unity)
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

            // Проверяем, попал ли луч в этот триггер
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                OnMouseDown();
                Debug.Log("Elevator call button pressed on floor" + floorNumber);
            }
        }
    }
    
    void OnMouseDown()
    {
        // Проверяем расстояние от камеры до триггера
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distance <= maxDistance && platform != null && !platform.canMove)
        {
            // Устанавливаем конечную точку (этаж) и включаем движение
            platform.endStop = floorNumber;
            platform.canMove = true;
        }
    }
}