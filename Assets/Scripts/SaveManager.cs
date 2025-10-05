using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [Header("Save Settings")]
    public int currentLevel = 0;
    public bool hasSaveData = false;
    
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
        PlayerPrefs.Save();
        currentLevel = 0;
        hasSaveData = false;
        
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
}
