using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Контроллер финальной сцены. Автоматически запускает диалог "Final" при загрузке сцены.
/// </summary>
public class FinalSceneController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("Диалог, который будет запущен при старте сцены. Если не назначен, будет искаться диалог с именем 'Final'")]
    public DialogueSO dialogueToShow;
    
    [Tooltip("Задержка перед запуском диалога (в секундах)")]
    public float delayInSeconds = 1f;
    
    [Header("Random Number Display")]
    [Tooltip("Текстовое поле для отображения случайных чисел")]
    public TextMeshProUGUI randomNumberText;
    
    [Tooltip("Включить отображение случайных чисел")]
    public bool enableRandomNumbers = false;
    
    [Tooltip("Интервал обновления чисел (в секундах)")]
    public float updateInterval = 1f;
    
    [Tooltip("Минимальное значение (включительно)")]
    public int minValue = 10000000;
    
    [Tooltip("Максимальное значение (включительно)")]
    public int maxValue = 100000000;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Coroutine randomNumberCoroutine;
    
    void Start()
    {
        StartCoroutine(InitializeAndShowDialogue());
        
        // Запускаем обновление случайных чисел, если включено
        if (enableRandomNumbers && randomNumberText != null)
        {
            randomNumberCoroutine = StartCoroutine(UpdateRandomNumbers());
        }
    }
    
    void OnDestroy()
    {
        // Останавливаем корутину при уничтожении объекта
        if (randomNumberCoroutine != null)
        {
            StopCoroutine(randomNumberCoroutine);
        }
    }
    
    private IEnumerator UpdateRandomNumbers()
    {
        while (true)
        {
            // Генерируем случайное число от minValue до maxValue
            int randomNumber = Random.Range(minValue, maxValue + 1);
            
            // Обновляем текст
            if (randomNumberText != null)
            {
                randomNumberText.text = randomNumber.ToString();
            }
            
            if (showDebugLogs)
                Debug.Log($"FinalSceneController: Обновлено случайное число: {randomNumber}");
            
            // Ждем указанный интервал
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    private IEnumerator InitializeAndShowDialogue()
    {
        // Ждем один кадр для инициализации всех систем
        yield return null;
        
        // Ждем указанное количество секунд
        if (delayInSeconds > 0)
        {
            if (showDebugLogs)
                Debug.Log($"FinalSceneController: Ждем {delayInSeconds} секунд перед показом диалога...");
            yield return new WaitForSeconds(delayInSeconds);
        }
        
        // Если диалог не назначен, пытаемся найти его по имени "Final"
        if (dialogueToShow == null)
        {
            if (showDebugLogs)
                Debug.Log("FinalSceneController: Диалог не назначен, ищем диалог 'Final'...");
            
            // Ищем диалог в Resources или используем другой способ
            // В Unity обычно диалоги хранятся как ScriptableObject, поэтому нужно назначить вручную
            if (showDebugLogs)
                Debug.LogWarning("FinalSceneController: Диалог не назначен! Пожалуйста, назначьте диалог 'Final' в инспекторе.");
            yield break;
        }
        
        // Проверяем все необходимые компоненты
        if (DialogueManager.Instance == null)
        {
            if (showDebugLogs)
                Debug.LogError("FinalSceneController: DialogueManager не найден на сцене! Убедитесь, что DialogueManager присутствует.");
            yield break;
        }
        
        if (DialogueManager.Instance.ui == null)
        {
            if (showDebugLogs)
                Debug.LogError("FinalSceneController: DialogueUI не назначен в DialogueManager!");
            yield break;
        }
        
        // Запускаем диалог
        DialogueManager.Instance.StartDialogue(dialogueToShow);
        
        if (showDebugLogs)
            Debug.Log($"FinalSceneController: Диалог '{dialogueToShow.name}' успешно запущен через {delayInSeconds} секунд!");
    }
}

