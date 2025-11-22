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
        // СБРОС СОСТОЯНИЯ ПРИ ЗАГРУЗКЕ СЦЕНЫ - предотвращает наследование состояния из предыдущей сцены
        ResetSceneState();
        
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
    
    void ResetSceneState()
    {
        // Сбрасываем состояние диалогов при загрузке новой сцены
        if (DialogueManager.Instance != null)
        {
            DialogueManager.ResetDialogueDone();
            DialogueManager.ResetCompletedDialogues();
        }
        
        // Сбрасываем состояние лифта
        ElevatorMoving elevator = FindObjectOfType<ElevatorMoving>();
        if (elevator != null)
        {
            elevator.canMove = false;
        }
        
        // Сбрасываем состояние NPC
        NPCMovement[] npcs = FindObjectsOfType<NPCMovement>();
        foreach (NPCMovement npc in npcs)
        {
            npc.canMove = false;
        }
        
        // Останавливаем звуки лифта при загрузке сцены
        if (ElevatorAudioManager.Instance != null)
        {
            ElevatorAudioManager.Instance.StopElevatorMovement();
        }
        
        Debug.Log("AutoSceneSaver: Состояние сцены сброшено при загрузке");
    }
}



