using UnityEngine;

public class FloorDoorController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float openPosition = 50f;
    [SerializeField] private float closePosition = 0f;
    [SerializeField] private float speed = 2f;
    [SerializeField] public int floorNumber = 0;
    [SerializeField] private AudioSource doorAudioSource; // AudioSource для звука дверей

    public bool isOpen = false;
    private Vector3 leftDoorTarget;
    private Vector3 rightDoorTarget;
    private bool isInitialized = false; // Флаг для предотвращения звуков при инициализации

    void Start()
    {
        leftDoorTarget = leftDoor.localPosition;
        rightDoorTarget = rightDoor.localPosition;
        
        // Убеждаемся, что звук не воспроизводится автоматически
        if (doorAudioSource != null)
        {
            doorAudioSource.playOnAwake = false;
            // Останавливаем звук, если он случайно играет
            if (doorAudioSource.isPlaying)
            {
                doorAudioSource.Stop();
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
        leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorTarget, speed * Time.deltaTime);
        rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorTarget, speed * Time.deltaTime);
    }

    public void OpenDoors()
    {
        Debug.Log("FloorDoorController.OpenDoors() called, isOpen: " + isOpen);
        
        // Воспроизводим звук только если двери были закрыты И инициализация завершена
        // Это предотвращает звук при загрузке сцены
        if (!isOpen && isInitialized)
        {
            PlayDoorSound();
        }
        
        leftDoorTarget = new Vector3(-openPosition, leftDoor.localPosition.y, leftDoor.localPosition.z);
        rightDoorTarget = new Vector3(openPosition, rightDoor.localPosition.y, rightDoor.localPosition.z);
        isOpen = true;
        
        Debug.Log("Floor doors opening on floor " + floorNumber);
    }

    public void CloseDoors()
    {
        Debug.Log("FloorDoorController.CloseDoors() called, isOpen: " + isOpen);
        
        // Воспроизводим звук только если двери были открыты И инициализация завершена
        // Это предотвращает звук при загрузке сцены
        if (isOpen && isInitialized)
        {
            PlayDoorSound();
        }
        
        leftDoorTarget = new Vector3(closePosition, leftDoor.localPosition.y, leftDoor.localPosition.z);
        rightDoorTarget = new Vector3(closePosition, rightDoor.localPosition.y, rightDoor.localPosition.z);
        isOpen = false;
        
        Debug.Log("Floor doors closing on floor " + floorNumber);
    }

    private void PlayDoorSound()
    {
        if (doorAudioSource != null)
        {
            doorAudioSource.Play();
        }
    }
}