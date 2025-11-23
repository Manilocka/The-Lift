using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueColliderInteraction : MonoBehaviour
{
    [Header("Настройки диалога")]
    public DialogueSO dialogue;           // Диалог, который появится при взаимодействии
    public bool oneTimeOnly = true;       // Можно ли активировать только один раз
    public bool triggerOnEnter = true;    // Срабатывать при входе в коллайдер
    public bool triggerOnExit = false;    // Срабатывать при выходе из коллайдера
    
    [Header("Настройки задержки")]
    [Tooltip("Задержка перед показом диалога (секунды)")] 
    public float delayBeforeDialogue = 0f;
    
    [Header("Отладка")]
    [SerializeField] bool logDebug = true;

    private bool alreadyActivated = false;
    private bool isPlayerInside = false;

    void Start()
    {
        // Убеждаемся, что коллайдер настроен как триггер
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            // Debug.LogWarning($"DialogueColliderInteraction: Коллайдер на {gameObject.name} не настроен как Trigger!");
        }
    }

    // Проверка входа в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (triggerOnEnter)
            {
                StartCoroutine(ShowDialogueWithDelay());
            }
        }
    }

    // Проверка выхода из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (triggerOnExit)
            {
                StartCoroutine(ShowDialogueWithDelay());
            }
        }
    }

    private IEnumerator ShowDialogueWithDelay()
    {
        // Ждем указанную задержку
        if (delayBeforeDialogue > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDialogue);
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        // if (logDebug) Debug.Log($"DialogueColliderInteraction: Попытка показать диалог на {gameObject.name}");
        
        // Если диалог уже был запущен и oneTimeOnly включён — выходим
        if (oneTimeOnly && alreadyActivated)
        {
            // if (logDebug) Debug.Log($"DialogueColliderInteraction: уже активирован ранее на {gameObject.name}");
            return;
        }

        if (dialogue == null)
        {
            // Debug.LogWarning($"DialogueColliderInteraction: Диалог не назначен на объекте {gameObject.name}");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            // Debug.LogWarning("DialogueColliderInteraction: DialogueManager не найден на сцене!");
            return;
        }

        // Если сейчас уже идёт другой диалог — игнорируем
        if (DialogueManager.Instance.IsDialogueActive)
        {
            // if (logDebug) Debug.Log("DialogueColliderInteraction: Диалог уже активен, игнорируем");
            return;
        }

        // Запускаем диалог
        DialogueManager.Instance.StartDialogue(dialogue);
        alreadyActivated = true;
        
        // if (logDebug) Debug.Log($"DialogueColliderInteraction: Диалог запущен на {gameObject.name}");
    }

    // Публичные методы для внешнего управления
    public void SetDialogue(DialogueSO newDialogue)
    {
        dialogue = newDialogue;
    }

    public void ResetActivation()
    {
        alreadyActivated = false;
    }

    public bool IsPlayerInside()
    {
        return isPlayerInside;
    }

    public bool HasBeenActivated()
    {
        return alreadyActivated;
    }

    // Метод для принудительного показа диалога (можно вызвать из других скриптов)
    public void ForceShowDialogue()
    {
        ShowDialogue();
    }
}
