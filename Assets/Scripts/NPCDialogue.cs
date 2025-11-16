using UnityEngine;

public class NPCDialogue : MonoBehaviour {
    public DialogueSO dialogue;
    
    [Header("Настройки диалога")]
    public bool oneTimeOnly = true; // Диалог только один раз
    
    [Header("Camera Focus")]
    [Tooltip("Расстояние камеры от NPC во время диалога")]
    public float cameraDistance = 2.5f;
    
    [Tooltip("Высота камеры во время диалога (координата Y). Если 0, будет использована высота центра NPC + смещение")]
    public float cameraHeightY = 0f;
    
    [Tooltip("Смещение камеры по высоте относительно NPC (используется если cameraHeightY = 0)")]
    public float cameraHeightOffset = 0.1f;
    
    [Tooltip("Точка фокуса камеры (лицо/голова NPC). Опционально - если не назначена, используется центр NPC")]
    public Transform cameraFocusPoint;
    
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