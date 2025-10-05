using UnityEngine;

public class NPCDialogue : MonoBehaviour {
    public DialogueSO dialogue;
    
    [Header("Настройки диалога")]
    public bool oneTimeOnly = true; // Диалог только один раз
    
    private bool hasBeenTalkedTo = false; // Флаг, что с этим NPC уже говорили
    
    // Проверяем, можно ли говорить с этим NPC
    public bool CanTalk()
    {
        if (!oneTimeOnly) return true;
        return !hasBeenTalkedTo;
    }
    
    // Отмечаем, что с этим NPC уже говорили
    public void MarkAsTalkedTo()
    {
        hasBeenTalkedTo = true;
        Debug.Log($"NPC {gameObject.name} отмечен как 'уже говорили'");
    }
    
    // Сброс состояния (для тестирования)
    public void ResetTalkState()
    {
        hasBeenTalkedTo = false;
        Debug.Log($"Состояние NPC {gameObject.name} сброшено");
    }
}