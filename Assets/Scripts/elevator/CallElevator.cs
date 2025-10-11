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
            Debug.Log($"CallElevator: Setting endStop from {platform.endStop} to {floorNumber}");
            platform.endStop = floorNumber;
            Debug.Log($"CallElevator: After setting - startStop = {platform.startStop}, endStop = {platform.endStop}");
            platform.canMove = true;
        }
    }
}