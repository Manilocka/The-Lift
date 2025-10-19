using UnityEngine;

public class ElevatorAudioManager : MonoBehaviour
{
    [Header("Button Sounds")]
    public AudioClip buttonPressSound; // Звук нажатия кнопки лифта
    public AudioSource buttonAudioSource; // AudioSource для звуков кнопок
    
    [Header("Elevator Movement")]
    public AudioClip elevatorMovementSound; // Звук движения лифта
    public AudioSource elevatorAudioSource; // AudioSource для звуков лифта
    
    
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
        if (elevatorAudioSource == null)
        {
            elevatorAudioSource = GetComponent<AudioSource>();
            if (elevatorAudioSource == null)
            {
                elevatorAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        elevatorAudioSource.playOnAwake = false;
        elevatorAudioSource.loop = true; // Звук движения должен зацикливаться
        
        // Инициализация AudioSource для кнопок
        if (buttonAudioSource == null)
        {
            buttonAudioSource = gameObject.AddComponent<AudioSource>();
        }
        buttonAudioSource.playOnAwake = false;
        buttonAudioSource.loop = false;
        
    }
    
    // Методы для воспроизведения звуков через AudioSource
    public void PlayButtonSound()
    {
        if (buttonPressSound != null && buttonAudioSource != null)
        {
            buttonAudioSource.clip = buttonPressSound;
            buttonAudioSource.Play();
        }
    }
    
    
    public void StartElevatorMovement()
    {
        if (elevatorMovementSound != null && elevatorAudioSource != null)
        {
            if (!elevatorAudioSource.isPlaying)
            {
                elevatorAudioSource.clip = elevatorMovementSound;
                elevatorAudioSource.Play();
            }
        }
    }
    
    public void StopElevatorMovement()
    {
        if (elevatorAudioSource != null && elevatorAudioSource.isPlaying)
        {
            elevatorAudioSource.Stop();
        }
    }
}
