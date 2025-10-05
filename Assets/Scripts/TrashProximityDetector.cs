using UnityEngine;

public class TrashProximityDetector : MonoBehaviour
{
    [Header("Настройки")]
    public float detectionRadius = 2f; // Радиус обнаружения
    public DialogueSO trashDialogue; // Диалог "Я не могу пройти, здесь мусор"
    public float dialogueDuration = 3f; // Длительность показа диалога
    
    private bool isDialogueActive = false;
    private float lastDialogueTime = 0f;
    private float dialogueCooldown = 5f; // Кулдаун между диалогами
    
    void Update()
    {
        CheckForTrash();
    }
    
    private void CheckForTrash()
    {
        // Проверяем кулдаун
        if (Time.time - lastDialogueTime < dialogueCooldown)
        {
            return;
        }
        
        // Ищем все объекты с тегом "trash" в радиусе
        Collider[] trashObjects = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider trash in trashObjects)
        {
            if (trash.CompareTag("trash"))
            {
                // Показываем диалог только если он еще не активен
                if (!isDialogueActive)
                {
                    ShowTrashDialogue();
                    break; // Показываем диалог только один раз
                }
            }
        }
    }
    
    private void ShowTrashDialogue()
    {
        if (trashDialogue != null && DialogueManager.Instance != null)
        {
            isDialogueActive = true;
            lastDialogueTime = Time.time;
            
            DialogueManager.Instance.StartDialogue(trashDialogue);
            
            // Автоматически скрываем диалог через указанное время
            StartCoroutine(HideDialogueAfterTime());
        }
    }
    
    private System.Collections.IEnumerator HideDialogueAfterTime()
    {
        yield return new WaitForSeconds(dialogueDuration);
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue();
        }
        
        // Небольшая задержка перед возможностью показать диалог снова
        yield return new WaitForSeconds(1f);
        isDialogueActive = false;
    }
    
    // Показываем радиус обнаружения в сцене
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
