using System.Collections;
using UnityEngine;

public class SceneStartDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueSO dialogueToShow;
    [SerializeField] private float delayInSeconds = 1f;
    [SerializeField] private bool showEveryTime = true; // Показывать каждый раз или только один раз
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private const string PLAYER_PREFS_KEY = "SceneStartDialogueShown";
    
    void Start()
    {
        // Ждем один кадр, чтобы все системы инициализировались
        StartCoroutine(InitializeAndShowDialogue());
    }
    
    private IEnumerator InitializeAndShowDialogue()
    {
        // Ждем один кадр для инициализации всех систем
        yield return null;
        
        // Проверяем, нужно ли показывать диалог
        if (!showEveryTime && PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            if (showDebugLogs)
                Debug.Log("SceneStartDialogue: Диалог уже показывался ранее, пропускаем.");
            yield break;
        }
        
        // Ждем указанное количество секунд
        if (delayInSeconds > 0)
        {
            if (showDebugLogs)
                Debug.Log($"SceneStartDialogue: Ждем {delayInSeconds} секунд перед показом диалога...");
            yield return new WaitForSeconds(delayInSeconds);
        }
        
        // Проверяем все необходимые компоненты
        if (dialogueToShow == null)
        {
            if (showDebugLogs)
                Debug.LogError("SceneStartDialogue: Диалог не назначен! Перетащите DialogueSO в поле 'Dialogue To Show'.");
            yield break;
        }
        
        if (DialogueManager.Instance == null)
        {
            if (showDebugLogs)
                Debug.LogError("SceneStartDialogue: DialogueManager не найден на сцене! Убедитесь, что DialogueManager присутствует.");
            yield break;
        }
        
        if (DialogueManager.Instance.ui == null)
        {
            if (showDebugLogs)
                Debug.LogError("SceneStartDialogue: DialogueUI не назначен в DialogueManager!");
            yield break;
        }
        
        // Запускаем диалог
        DialogueManager.Instance.StartDialogue(dialogueToShow);
        
        if (showDebugLogs)
            Debug.Log($"SceneStartDialogue: Диалог '{dialogueToShow.name}' успешно запущен через {delayInSeconds} секунд!");
        
        // Отмечаем, что диалог был показан (если нужно)
        if (!showEveryTime)
        {
            PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 1);
            PlayerPrefs.Save();
        }
    }
    
    // Метод для сброса флага (для тестирования)
    [ContextMenu("Reset Dialogue Flag")]
    public void ResetDialogueFlag()
    {
        PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
        PlayerPrefs.Save();
        Debug.Log("SceneStartDialogue: Флаг диалога сброшен. Диалог будет показан при следующей загрузке сцены.");
    }
    
    // Метод для принудительного запуска диалога
    [ContextMenu("Force Show Dialogue")]
    public void ForceShowDialogue()
    {
        if (dialogueToShow != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToShow);
            Debug.Log("SceneStartDialogue: Диалог принудительно запущен!");
        }
        else
        {
            Debug.LogError("SceneStartDialogue: Невозможно запустить диалог - отсутствуют необходимые компоненты!");
        }
    }
}
