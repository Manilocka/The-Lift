using UnityEngine;
using UnityEngine.SceneManagement;

// Инициализируемся максимально рано, чтобы другие скрипты могли читать значения
[DefaultExecutionOrder(-10000)]
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Runtime Settings")]
    public float volume = 100f;
    public float brightness = 50f;
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
        brightness = PlayerPrefs.HasKey("Brightness") ? PlayerPrefs.GetFloat("Brightness") : 50f;
        mouseSensitivity = PlayerPrefs.HasKey("MouseSensitivity") ? PlayerPrefs.GetFloat("MouseSensitivity") : 1f;
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
    }

    public void ApplyAll()
    {
        ApplyVolume(volume);
        ApplyBrightness(brightness);
        ApplyMouseSensitivity(mouseSensitivity);
    }

    public void ApplyVolume(float value)
    {
        volume = value;
        AudioListener.volume = value / 100f;
    }

    public void ApplyBrightness(float value)
    {
        brightness = value;
        // Пытаемся применить через UI Overlay, если он есть
        var applier = Object.FindObjectOfType<BrightnessApplier>();
        if (applier != null)
        {
            applier.Apply(value);
        }
        else
        {
            // Фолбэк: Camera.main background (если видимая)
            var cam = Camera.main;
            if (cam != null)
            {
                float brightnessValue = value / 100f;
                cam.backgroundColor = Color.Lerp(Color.black, Color.white, brightnessValue * 0.1f);
            }
        }
    }

    public void ApplyMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        // Экспортируем в PlayerPrefs для читающих её контроллеров
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}


