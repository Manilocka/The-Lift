using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [Header("Save Settings")]
    public int currentLevel = 0;
    public bool hasSaveData = false;
    public string lastSceneName = string.Empty;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadGameData()
    {
        // Загружаем данные сохранения
        currentLevel = PlayerPrefs.GetInt("GameProgress", 0);
        hasSaveData = currentLevel > 0;
        lastSceneName = PlayerPrefs.GetString("LastScene", string.Empty);
        
        Debug.Log($"Загружены данные сохранения: уровень {currentLevel}, есть сохранение: {hasSaveData}");
    }
    
    public void SaveGame(int level)
    {
        currentLevel = level;
        PlayerPrefs.SetInt("GameProgress", level);
        PlayerPrefs.Save();
        hasSaveData = true;
        
        Debug.Log($"Игра сохранена: уровень {level}");
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("GameProgress");
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();
        currentLevel = 0;
        hasSaveData = false;
        lastSceneName = string.Empty;
        
        Debug.Log("Сохранение удалено");
    }
    
    public bool HasSaveData()
    {
        return hasSaveData;
    }
    
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SaveCurrentScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        // Не сохраняем служебные сцены меню/настроек
        if (scene == "Game menu" || scene == "Settings") return;
        PlayerPrefs.SetString("LastScene", scene);
        PlayerPrefs.Save();
        lastSceneName = scene;
        hasSaveData = true;
        if (currentLevel == 0)
        {
            // Флаг наличия прогресса, если используется только сцена
            PlayerPrefs.SetInt("GameProgress", 1);
            PlayerPrefs.Save();
            currentLevel = 1;
        }
        Debug.Log($"Сцена сохранена: {scene}");
    }

    public string GetLastScene()
    {
        if (string.IsNullOrEmpty(lastSceneName))
        {
            lastSceneName = PlayerPrefs.GetString("LastScene", string.Empty);
        }
        return lastSceneName;
    }
}
