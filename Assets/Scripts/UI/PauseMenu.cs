using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")] 
    public GameObject pauseCanvas; // один Canvas паузы
    public Button resumeButton;
    public Button exitButton;
    
    [Header("Settings UI")]
    public Slider volumeSlider;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI volumeValueText;
    public TextMeshProUGUI mouseSensitivityValueText;

    [Header("Scenes")] 
    public string mainMenuSceneName = "Game menu";

    private bool isPaused = false;

    void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitToMainMenu);
        
        // Настройка слайдеров
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            LoadVolumeSettings();
        }
        
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivitySliderChanged);
            LoadMouseSensitivitySettings();
        }

        // Если вернулись из настроек по паузному сценарию — сразу открыть паузу
        if (PlayerPrefs.GetInt("ReturnOpenPause", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ReturnOpenPause");
            PlayerPrefs.Save();
            PauseGame();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame(); else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Всегда просто снимаем паузу, не перегружаем сцену и не меняем ReturnScene
        if (!isPaused)
        {
            // На случай если кто-то вызвал Continue после возврата из настроек
            if (pauseCanvas != null) pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            // Прячем курсор обратно
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Прячем курсор обратно
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LoadVolumeSettings()
    {
        if (volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Volume", 100f);
            volumeSlider.value = volume;
            UpdateVolumeDisplay(volume);
        }
    }
    
    void LoadMouseSensitivitySettings()
    {
        if (mouseSensitivitySlider != null)
        {
            float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            mouseSensitivitySlider.value = sensitivity;
            UpdateMouseSensitivityDisplay(sensitivity);
        }
    }
    
    public void OnVolumeChanged(float value)
    {
        UpdateVolumeDisplay(value);
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
        
        // Применяем настройку ко всем источникам звука
        ApplyVolumeToAllAudioSources(value);
        
        // Применяем громкость напрямую
        AudioListener.volume = value / 100f;
    }
    
    void ApplyVolumeToAllAudioSources(float volumePercent)
    {
        // Находим все AudioSource в сцене
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        float volumeMultiplier = volumePercent / 100f;
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Сохраняем оригинальную громкость если еще не сохранена
            if (!audioSource.gameObject.GetComponent<OriginalVolume>())
            {
                var originalVolume = audioSource.gameObject.AddComponent<OriginalVolume>();
                originalVolume.originalVolume = audioSource.volume;
            }
            
            // Применяем глобальную громкость
            var originalVol = audioSource.gameObject.GetComponent<OriginalVolume>();
            if (originalVol != null)
            {
                audioSource.volume = originalVol.originalVolume * volumeMultiplier;
            }
        }
        
        Debug.Log($"PauseMenu: Применена громкость {volumePercent}% к {allAudioSources.Length} источникам звука");
    }
    
    public void OnMouseSensitivityChanged(float value)
    {
        UpdateMouseSensitivityDisplay(value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        
        // Применяем настройку чувствительности мыши к MouseLock
        MouseLock mouseLock = FindObjectOfType<MouseLock>();
        if (mouseLock != null)
        {
            mouseLock.UpdateMouseSensitivity(value);
        }
        else
        {
            Debug.LogWarning("MouseLock не найден в сцене!");
        }
        
        Debug.Log($"Mouse sensitivity set to: {value}");
    }
    
    void UpdateVolumeDisplay(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value).ToString();
        }
    }
    
    void UpdateMouseSensitivityDisplay(float value)
    {
        if (mouseSensitivityValueText != null)
        {
            mouseSensitivityValueText.text = value.ToString("F1");
        }
    }
    
    // Округляем значение чувствительности до 0.1
    public void OnMouseSensitivitySliderChanged(float value)
    {
        // Округляем до ближайшего 0.1
        float roundedValue = Mathf.Round(value * 10f) / 10f;
        
        // Устанавливаем округленное значение обратно в слайдер
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = roundedValue;
        }
        
        // Применяем округленное значение
        OnMouseSensitivityChanged(roundedValue);
    }

    public void ExitToMainMenu()
    {
        Debug.Log("PauseMenu: Выход в главное меню");
        
        // Фиксируем текущую сцену как последнюю (на случай Продолжить)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCurrentScene();
        }
        
        // Сбрасываем паузу
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // Загружаем главное меню с плавным затемнением, если есть SceneFader
        Debug.Log($"PauseMenu: Загружаем сцену '{mainMenuSceneName}'");
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // PauseMenu теперь управляет одним Canvas; никаких массивов
}


