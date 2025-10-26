using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueOnClick : MonoBehaviour
{
    [Header("Настройки диалога")]
    public DialogueSO dialogue;           // Диалог, который появится при клике
    public bool oneTimeOnly = false;      // Можно ли активировать только один раз
    [Tooltip("Автоматически закрывать диалог через заданное время")] public bool autoClose = true;
    [Tooltip("Время до закрытия диалога, сек")] public float autoCloseSeconds = 2f;
    [Header("Отладка")]
    [SerializeField] bool logDebug = true;

    private bool alreadyActivated = false;

    public void OnMouseDown()
    {
       ShowDialogue();
    }

    public void ShowDialogue()
    {
        Debug.Log("SDFWSOFBNJSJDKFSBKFJBJKFBSAKJDFBJSKF");
        
        // Если диалог уже был запущен и oneTimeOnly включён — выходим
        if (oneTimeOnly && alreadyActivated)
        {
            if (logDebug) Debug.Log($"DialogueOnClick: уже активирован ранее на {gameObject.name}");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning($"DialogueOnClick: Диалог не назначен на объекте {gameObject.name}");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueOnClick: DialogueManager не найден на сцене!");
            return;
        }

        // Если сейчас уже идёт другой диалог — игнорируем
        if (DialogueManager.Instance.IsDialogueActive)
        {
            if (logDebug) Debug.Log("DialogueOnClick: Диалог уже активен — пропускаем запуск");
            return;
        }

        if (logDebug) Debug.Log($"DialogueOnClick: Запускаем диалог '{dialogue.name}'");
        DialogueManager.Instance.StartDialogue(dialogue);
        alreadyActivated = true;

        if (autoClose)
        {
            StartCoroutine(AutoCloseCoroutine());
        }
    }

    IEnumerator AutoCloseCoroutine()
    {
        yield return new WaitForSeconds(autoCloseSeconds);
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (logDebug) Debug.Log("DialogueOnClick: Автозакрытие диалога");
            DialogueManager.Instance.EndDialogue();
        }
    }
}