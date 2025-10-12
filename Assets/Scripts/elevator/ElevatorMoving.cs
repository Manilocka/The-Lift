using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;

public class ElevatorMoving : MonoBehaviour
{
    
    //sss
    public bool canMove; // Переменная для включения/выключения движения
    [SerializeField] float speed = 120f; // Скорость движения лифта
    [SerializeField] Transform[] stops; // Массив точек остановки
    [SerializeField] public int startStop; // Начальная точка
    [SerializeField] public int endStop; 

    ElevatorDoorController doorController;

    private Rigidbody rb;
    private int currentTarget;
    private bool reverse;
    public int rememberStartStop = -10; // Переменная для запоминания начальной точки

    void Start()
    {
        doorController = GetComponentInChildren<ElevatorDoorController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физическую гравитацию и включаем кинематический режим
        rb.useGravity = false; // Отключаем гравитацию
        transform.position = stops[startStop].position; // Устанавливаем начальную позицию
        currentTarget = startStop;
        UpdateDirection();

    }

    void FixedUpdate() // Используем FixedUpdate для физики
    {
        if (!canMove) return; // Проверяем, можно ли двигаться
        
        // Проверяем, что endStop не превышает количество доступных этажей
        if (endStop >= stops.Length)
        {
            endStop = stops.Length - 1; // Устанавливаем максимально доступный этаж
            Debug.LogWarning($"endStop превышает количество этажей! Устанавливаем максимально доступный: {endStop}");
        }
        
        if (Vector3.Distance(transform.position, stops[currentTarget].position) < 0.1f) // Проверка достижения точки
        {
            if (currentTarget == endStop)
            {
                Debug.Log($"Лифт достиг конечной точки: этаж {endStop}");
                currentTarget = endStop; // Останавливаемся на конечной точке
                canMove = false;
                rememberStartStop = startStop; 
                startStop = endStop;
                Debug.Log("Открываем двери лифта...");
                doorController.OpenDoors(); // Открываем двери
                return;
            }
            UpdateDirection();
            if (reverse)
            {
                currentTarget--;
                if (currentTarget < startStop) currentTarget = startStop; // Возвращаемся к началу
            }
            else
            {
                currentTarget++;
                if (currentTarget > endStop) currentTarget = endStop; // Останавливаемся на конце
            }
        }

        // Движение к текущей цели
        Vector3 targetPosition = stops[currentTarget].position;
        rb.MovePosition(Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime));
    }

    private void UpdateDirection()
    {
        if (endStop > currentTarget)
        {
            reverse = false;
        }
        else if (endStop < currentTarget)
        {
            reverse = true;
        }
    }
}