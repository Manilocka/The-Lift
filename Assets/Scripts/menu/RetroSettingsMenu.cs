using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class RetroSettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button backButton;
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Slider mouseSensitivitySlider;
    
    [Header("Text Elements")]
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI brightnessText;
    public TextMeshProUGUI mouseSensitivityText;
    public TextMeshProUGUI volumeValueText;
    public TextMeshProUGUI brightnessValueText;
    public TextMeshProUGUI mouseSensitivityValueText;
    
    [Header("Retro Effects")]
    public Image scanlinesOverlay;
    public Image glitchOverlay;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip selectSound;
    public AudioClip confirmSound;
    
    [Header("Retro Settings")]
    public float glitchIntensity = 0.1f;
    public float scanlineSpeed = 1f;
    public Color retroGreen = new Color(0.2f, 1f, 0.2f, 1f);
    
    [Header("Settings")]
    public string mainMenuSceneName = "Game menu";
    
    private int currentSelection = 0;
    private Button[] menuButtons;
    private Slider[] settingsSliders;
    private bool isNavigating = false;
    
    void Start()
    {
        InitializeSettings();
        SetupRetroEffects();
        StartCoroutine(GlitchEffect());
    }
    
    void InitializeSettings()
    {
        // Инициализация массивов
        menuButtons = new Button[] { backButton };
        settingsSliders = new Slider[] { volumeSlider, brightnessSlider, mouseSensitivitySlider };
        
        // Настройка кнопок
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i;
            menuButtons[i].onClick.AddListener(() => SelectOption(index));
        }
        
        // Настройка слайдеров
        SetupSliders();
        
        // Загружаем сохраненные настройки
        LoadSettings();
        
        // Установка начального выделения
        UpdateSelection(0);
    }
    
    void SetupSliders()
    {
        // Настройка слайдера громкости
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 100f;
            volumeSlider.value = 100f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            Debug.Log("RetroSettingsMenu: Volume slider настроен");
        }
        
        // Настройка слайдера яркости
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 100f;
            brightnessSlider.value = 50f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            Debug.Log("RetroSettingsMenu: Brightness slider настроен");
        }
        
        // Настройка слайдера чувствительности мыши
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = 0.1f;
            mouseSensitivitySlider.maxValue = 2f;
            mouseSensitivitySlider.value = 1f;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            Debug.Log("RetroSettingsMenu: Mouse sensitivity slider настроен");
        }
    }
    
    void LoadSettings()
    {
        // Загружаем сохраненные настройки
        float volume = PlayerPrefs.GetFloat("Volume", 100f);
        float brightness = PlayerPrefs.GetFloat("Brightness", 50f);
        float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        
        if (volumeSlider != null)
            volumeSlider.value = volume;
        if (brightnessSlider != null)
            brightnessSlider.value = brightness;
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = mouseSensitivity;
        
        // Обновляем отображение значений
        UpdateValueDisplays();
        
        Debug.Log("RetroSettingsMenu: Настройки загружены");
    }
    
    void SaveSettings()
    {
        // Сохраняем настройки
        if (volumeSlider != null)
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        if (brightnessSlider != null)
            PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        if (mouseSensitivitySlider != null)
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
        
        PlayerPrefs.Save();
        Debug.Log("RetroSettingsMenu: Настройки сохранены");
    }
    
    void Update()
    {
        HandleInput();
        UpdateRetroEffects();
    }
    
    void HandleInput()
    {
        if (isNavigating) return;
        
        // Навигация стрелками
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            NavigateUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            NavigateDown();
        }
        
        // Выбор опции
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption(currentSelection);
        }
        
        // Навигация по слайдерам
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            AdjustCurrentSlider(-0.1f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            AdjustCurrentSlider(0.1f);
        }
    }
    
    void NavigateUp()
    {
        PlaySound(selectSound);
        currentSelection = (currentSelection - 1 + (menuButtons.Length + settingsSliders.Length)) % (menuButtons.Length + settingsSliders.Length);
        UpdateSelection(currentSelection);
    }
    
    void NavigateDown()
    {
        PlaySound(selectSound);
        currentSelection = (currentSelection + 1) % (menuButtons.Length + settingsSliders.Length);
        UpdateSelection(currentSelection);
    }
    
    void UpdateSelection(int index)
    {
        // Простое выделение - только моргающий текст
        currentSelection = index;
    }
    
    void AdjustCurrentSlider(float delta)
    {
        if (currentSelection < settingsSliders.Length && settingsSliders[currentSelection] != null)
        {
            settingsSliders[currentSelection].value += delta;
            PlaySound(selectSound);
        }
    }
    
    public void SelectOption(int optionIndex)
    {
        if (isNavigating) return;
        
        Debug.Log("RetroSettingsMenu: Выбрана опция " + optionIndex);
        StartCoroutine(SelectOptionCoroutine(optionIndex));
    }
    
    IEnumerator SelectOptionCoroutine(int optionIndex)
    {
        isNavigating = true;
        PlaySound(confirmSound);
        
        // Эффект мерцания при выборе
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.05f);
            yield return new WaitForSeconds(0.05f);
        }
        
        // Выполнение действия
        switch (optionIndex)
        {
            case 0: // Назад
                Debug.Log("RetroSettingsMenu: Возврат в главное меню");
                SaveSettings();
                ReturnToMainMenu();
                break;
        }
        
        isNavigating = false;
    }
    
    void ReturnToMainMenu()
    {
        Debug.Log("RetroSettingsMenu: Загрузка сцены " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    // Обработчики слайдеров
    public void OnVolumeChanged(float value)
    {
        UpdateValueDisplays();
        // Применяем громкость к аудио
        if (audioSource != null)
        {
            audioSource.volume = value / 100f;
        }
        Debug.Log("RetroSettingsMenu: Громкость изменена на " + value);
    }
    
    public void OnBrightnessChanged(float value)
    {
        UpdateValueDisplays();
        // Применяем яркость
        ApplyBrightness(value);
        Debug.Log("RetroSettingsMenu: Яркость изменена на " + value);
    }
    
    public void OnMouseSensitivityChanged(float value)
    {
        UpdateValueDisplays();
        // Применяем чувствительность мыши
        ApplyMouseSensitivity(value);
        Debug.Log("RetroSettingsMenu: Чувствительность мыши изменена на " + value);
    }
    
    void ApplyBrightness(float brightness)
    {
        // Применяем яркость к камере или пост-процессингу
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Простое изменение яркости через цвет камеры
            float brightnessValue = brightness / 100f;
            mainCamera.backgroundColor = Color.Lerp(Color.black, Color.white, brightnessValue * 0.1f);
        }
    }
    
    void ApplyMouseSensitivity(float sensitivity)
    {
        // Применяем чувствительность мыши
        // Это нужно будет настроить в скриптах управления игроком
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }
    
    void UpdateValueDisplays()
    {
        if (volumeValueText != null && volumeSlider != null)
        {
            volumeValueText.text = Mathf.RoundToInt(volumeSlider.value).ToString();
        }
        
        if (brightnessValueText != null && brightnessSlider != null)
        {
            brightnessValueText.text = Mathf.RoundToInt(brightnessSlider.value).ToString();
        }
        
        if (mouseSensitivityValueText != null && mouseSensitivitySlider != null)
        {
            mouseSensitivityValueText.text = mouseSensitivitySlider.value.ToString("F1");
        }
    }
    
    void SetupRetroEffects()
    {
        // Настройка эффекта сканирования
        if (scanlinesOverlay != null)
        {
            scanlinesOverlay.color = new Color(0, 1, 0, 0.1f);
        }
    }
    
    void UpdateRetroEffects()
    {
        // Анимация сканирования
        if (scanlinesOverlay != null)
        {
            float offset = Time.time * scanlineSpeed;
            scanlinesOverlay.material.SetFloat("_Offset", offset);
        }
    }
    
    IEnumerator GlitchEffect()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            
            if (glitchOverlay != null)
            {
                StartCoroutine(GlitchFlash());
            }
        }
    }
    
    IEnumerator GlitchFlash()
    {
        if (glitchOverlay != null)
        {
            glitchOverlay.color = new Color(1, 0, 0, glitchIntensity);
            yield return new WaitForSeconds(0.1f);
            glitchOverlay.color = new Color(0, 1, 0, glitchIntensity);
            yield return new WaitForSeconds(0.1f);
            glitchOverlay.color = Color.clear;
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

