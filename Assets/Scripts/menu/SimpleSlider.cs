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
    
    void Start()
    {
        SetupSlider();
    }
    
    void SetupSlider()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        if (slider != null)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = PlayerPrefs.GetFloat(settingKey, defaultValue);
            slider.onValueChanged.AddListener(OnValueChanged);
            
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
            case "Brightness":
                ApplyBrightness(value);
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
    
    void ApplyBrightness(float brightness)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplyBrightness(brightness);
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

