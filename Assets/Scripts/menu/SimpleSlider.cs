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
        // Применяем громкость
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            source.volume = volume / 100f;
        }
    }
    
    void ApplyBrightness(float brightness)
    {
        // Применяем яркость
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float brightnessValue = brightness / 100f;
            mainCamera.backgroundColor = Color.Lerp(Color.black, Color.white, brightnessValue * 0.1f);
        }
    }
    
    void ApplyMouseSensitivity(float sensitivity)
    {
        // Применяем чувствительность мыши
        // Это нужно будет настроить в скриптах управления игроком
        Debug.Log("SimpleSlider: Чувствительность мыши установлена на " + sensitivity);
    }
}
