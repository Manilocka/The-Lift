using UnityEngine;

public class ElevatorAudioManager : MonoBehaviour
{
    [Header("Elevator Movement")]
    public AudioSource elevatorAudioSource; // AudioSource для звуков движения лифта
    
    [Header("Elevator Arrival")]
    public AudioSource arrivalAudioSource; // AudioSource для звука прибытия на этаж
    
    
    // Singleton для доступа из других скриптов
    public static ElevatorAudioManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Инициализация AudioSource для движения лифта
        if (elevatorAudioSource != null)
        {
            elevatorAudioSource.playOnAwake = false;
            elevatorAudioSource.loop = true; // Звук движения должен зацикливаться
        }

        // Инициализация AudioSource для звука прибытия
        if (arrivalAudioSource != null)
        {
            arrivalAudioSource.playOnAwake = false;
            arrivalAudioSource.loop = false;
        }
    }
    
    // Методы для воспроизведения звуков через AudioSource
    
    public void StartElevatorMovement()
    {
        // Запускаем звук движения
        if (elevatorAudioSource != null && !elevatorAudioSource.isPlaying)
        {
            elevatorAudioSource.Play();
        }
    }
    
    public void StopElevatorMovement()
    {
        // Останавливаем звук движения
        if (elevatorAudioSource != null && elevatorAudioSource.isPlaying)
        {
            elevatorAudioSource.Stop();
        }
    }
    
    public void PlayArrivalSound()
    {
        // Воспроизводим звук прибытия на этаж
        if (arrivalAudioSource != null)
        {
            arrivalAudioSource.Play();
        }
    }
}
