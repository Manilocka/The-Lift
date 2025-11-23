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
            // Останавливаем звук при загрузке сцены
            elevatorAudioSource.Stop();
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
        
        // Подписываемся на событие загрузки сцены для сброса состояния
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Останавливаем все звуки лифта при загрузке новой сцены
        StopElevatorMovement();
        StopArrivalSound();
        
        // ВАЖНО: Проверяем, есть ли в новой сцене лифт, и если нет - полностью останавливаем звуки
        // Это предотвращает наследование звуков из предыдущих сцен
        ElevatorMoving elevator = FindObjectOfType<ElevatorMoving>();
        if (elevator == null)
        {
            // В сцене нет лифта - убеждаемся, что все звуки остановлены
            if (elevatorAudioSource != null && elevatorAudioSource.isPlaying)
            {
                elevatorAudioSource.Stop();
            }
            if (arrivalAudioSource != null && arrivalAudioSource.isPlaying)
            {
                arrivalAudioSource.Stop();
            }
            Debug.Log("ElevatorAudioManager: В сцене нет лифта, все звуки остановлены");
        }
        else
        {
            // ИСПРАВЛЕНИЕ: Если в сцене есть лифт, убеждаемся, что AudioSource правильно настроены
            // Это особенно важно для day7 сцен, где звук может не работать
            if (elevatorAudioSource != null)
            {
                elevatorAudioSource.playOnAwake = false;
                elevatorAudioSource.loop = true;
                // Убеждаемся, что clip назначен
                if (elevatorAudioSource.clip == null && elevatorMovementSound != null)
                {
                    elevatorAudioSource.clip = elevatorMovementSound;
                    Debug.Log($"ElevatorAudioManager: Назначен clip для звука лифта в сцене {scene.name}");
                }
            }
            Debug.Log($"ElevatorAudioManager: В сцене {scene.name} найден лифт, AudioSource настроены");
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
        if (elevatorAudioSource != null)
        {
            if (!elevatorAudioSource.isPlaying)
            {
                if (elevatorMovementSound != null)
                {
                    elevatorAudioSource.clip = elevatorMovementSound;
                }
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
