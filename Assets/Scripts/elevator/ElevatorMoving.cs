using OpenCover.Framework.Model;
using UnityEngine;

public class ElevatorMoving : MonoBehaviour
{
    public bool canMove; // Переменная для включения/выключения движения
    [SerializeField] float speed = 120f; // Скорость движения лифта
    [SerializeField] Transform[] stops; // Массив точек остановки
    [SerializeField] public int startStop = 0; // Начальная точка
    [SerializeField] public int endStop = 1; // Конечная точка
    ElevatorDoorController doorController;

    private Rigidbody rb;
    private int currentTarget;
    private bool reverse;

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
        if (Vector3.Distance(transform.position, stops[currentTarget].position) < 0.1f) // Проверка достижения точки
        {
            if (currentTarget == endStop)
            {
                currentTarget = endStop; // Останавливаемся на конечной точке
                canMove = false;
                startStop = endStop;
                doorController.OpenDoors(); // Открываем двери
                return;
            }
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