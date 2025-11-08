using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneSaver : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Сохраняем текущую сцену через SaveManager, если он есть
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCurrentScene();
        }
        else
        {
            // Fallback: сохраняем напрямую
            string name = scene.name;
            if (name != "Game menu" && name != "Settings")
            {
                PlayerPrefs.SetString("LastScene", name);
                if (!PlayerPrefs.HasKey("GameProgress"))
                {
                    PlayerPrefs.SetInt("GameProgress", 1);
                }
                PlayerPrefs.Save();
            }
        }
    }
}



