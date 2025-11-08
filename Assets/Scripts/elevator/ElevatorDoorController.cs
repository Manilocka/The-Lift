using System.Collections;
using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;  // Ссылка на левую дверь
    [SerializeField] private Transform rightDoor; // Ссылка на правую дверь
    [SerializeField] private float openPosition = 50f; // Расстояние открытия (в единицах Unity)
    [SerializeField] private float closePosition = 0f; // Начальная (закрытая) позиция
    [SerializeField] private float speed = 23f; // Скорость движения дверей
    [SerializeField] private AudioSource doorOpenAudioSource; // AudioSource для звука открытия дверей
    [SerializeField] private AudioSource doorCloseAudioSource; // AudioSource для звука закрытия дверей

    public bool isOpen = false; // Состояние дверей
    private Vector3 leftDoorTarget;
    private Vector3 rightDoorTarget;
    private bool isInitialized = false; // Флаг для предотвращения звуков при инициализации

    void Start()
    {
        // Устанавливаем начальные позиции (закрыто)
        leftDoorTarget = leftDoor.localPosition;
        rightDoorTarget = rightDoor.localPosition;
        
        // Убеждаемся, что звуки не воспроизводятся автоматически
        if (doorOpenAudioSource != null)
        {
            doorOpenAudioSource.playOnAwake = false;
            // Останавливаем звук, если он случайно играет
            if (doorOpenAudioSource.isPlaying)
            {
                doorOpenAudioSource.Stop();
            }
        }
        if (doorCloseAudioSource != null)
        {
            doorCloseAudioSource.playOnAwake = false;
            // Останавливаем звук, если он случайно играет
            if (doorCloseAudioSource.isPlaying)
            {
                doorCloseAudioSource.Stop();
            }
        }
        
        // Помечаем, что инициализация завершена
        // Используем корутину, чтобы это произошло после первого кадра
        StartCoroutine(DelayedInitialization());
    }
    
    System.Collections.IEnumerator DelayedInitialization()
    {
        // Ждем один кадр, чтобы все компоненты успели инициализироваться
        yield return null;
        isInitialized = true;
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
        if (!isOpen && AdditionalCheck.ElevatorDoorsCanBeOpened())
        {
            leftDoorTarget = leftDoor.localPosition + Vector3.left * openPosition;  // Движение влево
            rightDoorTarget = rightDoor.localPosition + Vector3.right * openPosition; // Движение вправо
            isOpen = true;
            
            // Воспроизводим звук открытия дверей только после инициализации
            // Это предотвращает звук при загрузке сцены
            if (isInitialized && doorOpenAudioSource != null)
            {
                doorOpenAudioSource.Play();
            }
            

            if (AdditionalCheck.ElevatorDoorsCanBeOpened())
            {
                Debug.Log("!!!!!! ElevatorDoorsCanBeOpened!!!");
            }
            else
            {
                Debug.Log("XXXXXX ElevatorDoorsCanBeOpened FALSE XXXX");
            }

            // Открываем внешние двери на текущем этаже
            OpenFloorDoors();
            
            if (AdditionalCheck.ElevatorDoorsCanBeClosed()) 
            {
                StartCoroutine(CloseDoorsAfterDelay(10f)); // Запускаем корутину для закрытия через 10 секунд
            }
        }
    }

    // Метод для открытия дверей с настраиваемой задержкой автозакрытия
    public void OpenDoorsWithAutoClose(float delay)
    {
        if (!isOpen && AdditionalCheck.ElevatorDoorsCanBeOpened())
        {
            leftDoorTarget = leftDoor.localPosition + Vector3.left * openPosition;
            rightDoorTarget = rightDoor.localPosition + Vector3.right * openPosition;
            isOpen = true;
            
            // Воспроизводим звук открытия дверей только после инициализации
            // Это предотвращает звук при загрузке сцены
            if (isInitialized && doorOpenAudioSource != null)
            {
                doorOpenAudioSource.Play();
            }
            

            // Открываем внешние двери на текущем этаже
            OpenFloorDoors();

            StartCoroutine(CloseDoorsAfterDelay(delay));
        }
    }

    // Метод для закрытия дверей
    public void CloseDoors()
    {
        if (isOpen)
        {
            leftDoorTarget = new Vector3(closePosition, leftDoor.localPosition.y, leftDoor.localPosition.z);  // Закрытие влево
            rightDoorTarget = new Vector3(closePosition, rightDoor.localPosition.y, rightDoor.localPosition.z); // Закрытие вправо
            isOpen = false;
            
            // Воспроизводим звук закрытия дверей только после инициализации
            // Это предотвращает звук при загрузке сцены
            if (isInitialized && doorCloseAudioSource != null)
            {
                doorCloseAudioSource.Play();
            }
            
            
            // Закрываем внешние двери на текущем этаже
            CloseFloorDoors();
        }
    }

    // Корутина для задержки закрытия дверей
    public IEnumerator CloseDoorsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseDoors();
    }

    // Проверка, что двери полностью закрыты (а не просто начали закрываться)
    public bool AreDoorsFullyClosed()
    {
        bool leftClosed = Mathf.Approximately(leftDoor.localPosition.x, closePosition);
        bool rightClosed = Mathf.Approximately(rightDoor.localPosition.x, closePosition);
        return !isOpen && leftClosed && rightClosed;
    }

    // Метод для открытия внешних дверей на текущем этаже
    private void OpenFloorDoors()
    {
        // Получаем текущий этаж лифта
        ElevatorMoving elevator = GetComponentInParent<ElevatorMoving>();
        if (elevator != null)
        {
            int currentFloor = elevator.startStop; // Текущий этаж лифта
            Debug.Log("=== OPENING FLOOR DOORS ===");
            Debug.Log("Elevator is on floor: " + currentFloor);
            
            // Находим все объекты с тегом DoorOutElevator
            GameObject[] floorDoors = GameObject.FindGameObjectsWithTag("DoorOutElevator");
            Debug.Log("Found " + floorDoors.Length + " floor doors");
            
            foreach (GameObject door in floorDoors)
            {
                // Проверяем, находится ли дверь на том же этаже
                FloorDoorController floorDoorController = door.GetComponent<FloorDoorController>();
                if (floorDoorController != null)
                {
                    Debug.Log("Checking door: " + door.name + " on floor: " + floorDoorController.floorNumber + ", isOpen: " + floorDoorController.isOpen);
                    if (floorDoorController.floorNumber == currentFloor)
                    {
                        Debug.Log("✓ MATCH! Opening floor door on floor " + currentFloor);
                        floorDoorController.OpenDoors();
                    }
                    else
                    {
                        Debug.Log("✗ NO MATCH! Door floor " + floorDoorController.floorNumber + " != elevator floor " + currentFloor);
                    }
                }
                else
                {
                    Debug.Log("ERROR: No FloorDoorController found on " + door.name);
                }
            }
            Debug.Log("=== END OPENING FLOOR DOORS ===");
        }
        else
        {
            Debug.Log("ERROR: No ElevatorMoving component found!");
        }
    }

    // Метод для закрытия внешних дверей на текущем этаже
    private void CloseFloorDoors()
    {
        // Получаем текущий этаж лифта
        ElevatorMoving elevator = GetComponentInParent<ElevatorMoving>();
        if (elevator != null)
        {
            int currentFloor = elevator.startStop; // Текущий этаж лифта
            
            // Находим все объекты с тегом DoorOutElevator
            GameObject[] floorDoors = GameObject.FindGameObjectsWithTag("DoorOutElevator");
            foreach (GameObject door in floorDoors)
            {
                // Проверяем, находится ли дверь на том же этаже
                FloorDoorController floorDoorController = door.GetComponent<FloorDoorController>();
                if (floorDoorController != null && floorDoorController.floorNumber == currentFloor)
                {
                    floorDoorController.CloseDoors();
                }
            }
        }
    }
}