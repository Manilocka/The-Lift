using UnityEngine;

public class DialogueReset : MonoBehaviour
{
    [Header("Управление диалогами")]
    [SerializeField] private KeyCode resetKey = KeyCode.R; // Клавиша для сброса
    
    void Update()
    {
        // Сброс прогресса диалогов по нажатию клавиши
        if (Input.GetKeyDown(resetKey))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ResetAllDialogueProgress();
                Debug.Log("Прогресс всех диалогов сброшен!");
            }
            
            // Сброс состояния всех NPC
            ResetAllNPCs();
        }
    }
    
    private void ResetAllNPCs()
    {
        // Находим все NPC в сцене и сбрасываем их состояние
        NPCDialogue[] allNPCs = FindObjectsOfType<NPCDialogue>();
        foreach (NPCDialogue npc in allNPCs)
        {
            npc.ResetTalkState();
        }
        Debug.Log($"Состояние {allNPCs.Length} NPC сброшено!");
    }
    
    void OnGUI()
    {
        // Показываем подсказку на экране
        GUI.Label(new Rect(10, 10, 300, 20), $"Нажмите {resetKey} для сброса прогресса диалогов");
    }
}
