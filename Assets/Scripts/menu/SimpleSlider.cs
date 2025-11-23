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
            
            // ВАЖНО: Для MouseSensitivity дефолтное значение 0.5
            if (settingKey == "MouseSensitivity")
            {
                defaultValue = 0.5f;
                // Минимальное значение не может быть 0
                if (minValue <= 0f)
                {
                    minValue = 0.1f;
                    slider.minValue = minValue;
                }
            }
            
            // Значение: дефолт только при первом запуске, дальше — только сохранённые
            float loadedValue = PlayerPrefs.HasKey(settingKey) ? PlayerPrefs.GetFloat(settingKey) : defaultValue;
            
            // ВАЖНО: Для MouseSensitivity значение никогда не может быть 0
            if (settingKey == "MouseSensitivity" && loadedValue <= 0f)
            {
                loadedValue = 0.5f; // Дефолтное значение
                PlayerPrefs.SetFloat(settingKey, loadedValue);
                PlayerPrefs.Save();
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
        // ВАЖНО: Для MouseSensitivity значение никогда не может быть 0
        if (settingKey == "MouseSensitivity" && value <= 0f)
        {
            value = 0.1f; // Минимальное значение
            if (slider != null)
            {
                slider.value = value;
            }
            Debug.LogWarning($"SimpleSlider: Попытка установить чувствительность мыши 0, устанавливаем минимальное значение 0.1");
        }
        
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
    }
    
    
    void ApplyMouseSensitivity(float sensitivity)
    {
        // ВАЖНО: Значение никогда не может быть 0
        if (sensitivity <= 0f)
        {
            sensitivity = 0.1f; // Минимальное значение
            Debug.LogWarning($"SimpleSlider: Попытка применить чувствительность мыши 0, используем минимальное значение 0.1");
        }
        
        // Применяем чувствительность мыши к MouseLock
        MouseLock mouseLock = FindObjectOfType<MouseLock>();
        if (mouseLock != null)
        {
            mouseLock.UpdateMouseSensitivity(sensitivity);
        }
        else
        {
            Debug.LogWarning("MouseLock не найден в сцене!");
        }
        
        Debug.Log($"Mouse sensitivity applied: {sensitivity}");
    }
}

