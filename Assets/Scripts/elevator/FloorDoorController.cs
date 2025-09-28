using UnityEngine;

public class FloorDoorController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;  // Ссылка на левую дверь
    [SerializeField] private Transform rightDoor; // Ссылка на правую дверь
    [SerializeField] private float openPosition = 50f; // Расстояние открытия (в единицах Unity)
    [SerializeField] private float closePosition = 0f; // Начальная (закрытая) позиция
    [SerializeField] private float speed = 2f; // Скорость движения дверей
    [SerializeField] public int floorNumber = 0; // Номер этажа, на котором находятся эти двери

    public bool isOpen = false; // Состояние дверей
    private Vector3 leftDoorTarget;
    private Vector3 rightDoorTarget;

    void Start()
    {
        // Устанавливаем начальные позиции (закрыто)
        leftDoorTarget = leftDoor.localPosition;
        rightDoorTarget = rightDoor.localPosition;
    }

    void Update()
    {
        // Плавное перемещение дверей к целевой позиции
        leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorTarget, speed * Time.deltaTime);
        rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorTarget, speed * Time.deltaTime);
    }

    // Метод для открытия дверей
    public void OpenDoors()
    {
        Debug.Log("FloorDoorController.OpenDoors() called, isOpen: " + isOpen);
        // Убираем проверку isOpen - всегда открываем двери
        leftDoorTarget = leftDoor.localPosition + Vector3.left * openPosition;  // Движение влево
        rightDoorTarget = rightDoor.localPosition + Vector3.right * openPosition; // Движение вправо
        isOpen = true;
        Debug.Log("Floor doors opened on floor " + floorNumber);
    }

    // Метод для закрытия дверей
    public void CloseDoors()
    {
        Debug.Log("FloorDoorController.CloseDoors() called, isOpen: " + isOpen);
        // Убираем проверку isOpen - всегда закрываем двери
        leftDoorTarget = new Vector3(closePosition, leftDoor.localPosition.y, leftDoor.localPosition.z);  // Закрытие влево
        rightDoorTarget = new Vector3(closePosition, rightDoor.localPosition.y, rightDoor.localPosition.z); // Закрытие вправо
        isOpen = false;
        Debug.Log("Floor doors closed on floor " + floorNumber);
    }
}
