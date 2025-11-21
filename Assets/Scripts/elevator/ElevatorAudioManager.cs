using UnityEngine;

public class ElevatorAudioManager : MonoBehaviour
{
    [Header("Button Sounds")]
    public AudioClip buttonPressSound; // Звук нажатия кнопки лифта
    public AudioSource buttonAudioSource; // AudioSource для звуков кнопок
    
    [Header("Elevator Movement")]
    public AudioClip elevatorMovementSound; // Звук движения лифта
    public AudioSource elevatorAudioSource; // AudioSource для звуков лифта
    
    [Header("Elevator Arrival")]
    public AudioClip arrivalBellSound; // Звук звоночка при прибытии
    public AudioSource arrivalAudioSource; // AudioSource для звука прибытия
    
    
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
        // // Инициализация AudioSource для движения лифта
        // if (elevatorAudioSource == null)
        // {
        //     elevatorAudioSource = GetComponent<AudioSource>();
        //     if (elevatorAudioSource == null)
        //     {
        //         elevatorAudioSource = gameObject.AddComponent<AudioSource>();
        //     }
        // }
        if (elevatorAudioSource != null)
        {
            elevatorAudioSource.playOnAwake = false;
            elevatorAudioSource.loop = true; // Звук движения должен зацикливаться
        }

        // Инициализация AudioSource для кнопок
        // if (buttonAudioSource == null)
        // {
        //     buttonAudioSource = gameObject.AddComponent<AudioSource>();
        // }
        if (buttonAudioSource != null)
        {
            buttonAudioSource.playOnAwake = false;
            buttonAudioSource.loop = false;
        }

        // Инициализация AudioSource для звука прибытия
        // if (arrivalAudioSource == null)
        // {
        //     arrivalAudioSource = gameObject.AddComponent<AudioSource>();
        // }
        if (arrivalAudioSource != null)
        {
            arrivalAudioSource.playOnAwake = false;
            arrivalAudioSource.loop = false;
        }
        
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
        Debug.Log($"ELEVATOR START 1");
                Debug.Log($"elevatorMovementSound {elevatorMovementSound}");
                Debug.Log($"elevatorAudioSource {elevatorAudioSource}");

        if (elevatorAudioSource != null)
        {
            Debug.Log($"ELEVATOR START 2");
            if (!elevatorAudioSource.isPlaying)
            {
                Debug.Log($"ELEVATOR START 3");
                // elevatorAudioSource.clip = elevatorMovementSound;
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
    
    public void PlayArrivalSound()
    {
        if (arrivalBellSound != null && arrivalAudioSource != null)
        {
            arrivalAudioSource.clip = arrivalBellSound;
            arrivalAudioSource.Play();
        }
    }

    public void StopArrivalSound()
    {
        if (arrivalAudioSource != null && arrivalAudioSource.isPlaying)
        {
            arrivalAudioSource.Stop();
        }
    }
}
