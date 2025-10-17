using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSlider : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider;
    public TextMeshProUGUI valueText;
    public string settingKey = "";
    public float defaultValue = 50f;
    public float minValue = 0f;
    public float maxValue = 100f;
    public string valueFormat = "F0";
    
    void Awake()
    {
        SetupSlider();
    }
    
    void Start()
    {
        // На всякий случай продублируем применение, когда все Awake уже отработали
        if (slider != null)
        {
            ApplySetting(slider.value);
            UpdateValueDisplay(slider.value);
        }
    }
    
    void SetupSlider()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        if (slider != null)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            // Значение: дефолт только при первом запуске, дальше — только сохранённые
            float loadedValue;
            if (SettingsManager.Instance != null)
            {
                // Берём из менеджера, он уже загрузил из PlayerPrefs
                switch (settingKey)
                {
                    case "Volume":
                        loadedValue = SettingsManager.Instance.volume;
                        break;
                    case "MouseSensitivity":
                        loadedValue = SettingsManager.Instance.mouseSensitivity;
                        break;
                    default:
                        loadedValue = PlayerPrefs.HasKey(settingKey) ? PlayerPrefs.GetFloat(settingKey) : defaultValue;
                        break;
                }
            }
            else
            {
                loadedValue = PlayerPrefs.HasKey(settingKey) ? PlayerPrefs.GetFloat(settingKey) : defaultValue;
            }
            // Ставим значение без нотификации, чтобы не поймать каскады при инициализации
            slider.SetValueWithoutNotify(loadedValue);
            slider.onValueChanged.AddListener(OnValueChanged);
            
            // Обновим отображение и немедленно применим загруженное значение
            UpdateValueDisplay(slider.value);
            ApplySetting(slider.value);
            
            // Если ключа ещё не было, фиксируем дефолт как сохранённый только один раз
            if (!string.IsNullOrEmpty(settingKey) && !PlayerPrefs.HasKey(settingKey))
            {
                PlayerPrefs.SetFloat(settingKey, slider.value);
                PlayerPrefs.Save();
            }
            
            Debug.Log("SimpleSlider: Слайдер настроен - " + gameObject.name);
        }
        else
        {
            Debug.LogError("SimpleSlider: Slider компонент не найден на " + gameObject.name);
        }
    }
    
    public void OnValueChanged(float value)
    {
        // Сохраняем значение
        if (!string.IsNullOrEmpty(settingKey))
        {
            PlayerPrefs.SetFloat(settingKey, value);
            PlayerPrefs.Save();
        }
        
        // Обновляем отображение
        UpdateValueDisplay(value);
        
        // Применяем настройку
        ApplySetting(value);
        
        Debug.Log("SimpleSlider: Значение изменено на " + value + " для " + settingKey);
    }
    
    void UpdateValueDisplay(float value)
    {
        if (valueText != null)
        {
            valueText.text = value.ToString(valueFormat);
        }
    }
    
    void ApplySetting(float value)
    {
        // Применяем настройку в зависимости от ключа
        switch (settingKey)
        {
            case "Volume":
                ApplyVolume(value);
                break;
            case "MouseSensitivity":
                ApplyMouseSensitivity(value);
                break;
        }
    }
    
    void ApplyVolume(float volume)
    {
        // Применяем глобальную громкость
        AudioListener.volume = volume / 100f;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplyVolume(volume);
        }
    }
    
    
    void ApplyMouseSensitivity(float sensitivity)
    {
        // Применяем чувствительность мыши через менеджер
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplyMouseSensitivity(sensitivity);
        }
    }
}

