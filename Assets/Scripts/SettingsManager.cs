using UnityEngine;
using UnityEngine.SceneManagement;

// Инициализируемся максимально рано, чтобы другие скрипты могли читать значения
[DefaultExecutionOrder(-10000)]
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Runtime Settings")]
    public float volume = 100f;
    public float mouseSensitivity = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPrefs();
            ApplyAll();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Пере-применяем настройки при каждой загрузке сцены
        ApplyAll();
    }

    public void LoadFromPrefs()
    {
        // Дефолты только если ключей нет; после первого раза используем сохранённые значения
        volume = PlayerPrefs.HasKey("Volume") ? PlayerPrefs.GetFloat("Volume") : 100f;
        mouseSensitivity = PlayerPrefs.HasKey("MouseSensitivity") ? PlayerPrefs.GetFloat("MouseSensitivity") : 1f;
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
    }

    public void ApplyAll()
    {
        ApplyVolume(volume);
        ApplyMouseSensitivity(mouseSensitivity);
    }

    public void ApplyVolume(float value)
    {
        volume = value;
        AudioListener.volume = value / 100f;
        
        // Применяем громкость ко всем AudioSource в сцене
        ApplyVolumeToAllAudioSources(value);
    }
    
    void ApplyVolumeToAllAudioSources(float volumePercent)
    {
        // Находим все AudioSource в сцене
        AudioSource[] allAudioSources = Object.FindObjectsOfType<AudioSource>();
        
        float volumeMultiplier = volumePercent / 100f;
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Получаем или создаем компонент OriginalVolume
            OriginalVolume originalVol = audioSource.GetComponent<OriginalVolume>();
            if (originalVol == null)
            {
                originalVol = audioSource.gameObject.AddComponent<OriginalVolume>();
                originalVol.originalVolume = audioSource.volume;
            }
            
            // Применяем глобальную громкость
            audioSource.volume = originalVol.originalVolume * volumeMultiplier;
        }
    }


    public void ApplyMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        // Экспортируем в PlayerPrefs для читающих её контроллеров и применяем к стандартным контроллерам, если есть
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        var look = Object.FindObjectOfType<MonoBehaviour>();
        // Здесь можно добавить конкретные контроллеры, если ты назовёшь класс
    }
}


