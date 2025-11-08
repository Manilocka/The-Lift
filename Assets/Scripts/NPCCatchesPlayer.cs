using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPCCatchesPlayer : MonoBehaviour
{
    [Header("Настройки обнаружения")]
    public float catchDistance = 2f; // Расстояние, на котором NPC ловит игрока
    public bool useTrigger = true; // Использовать триггер вместо проверки расстояния
    
    [Header("Настройки темного экрана")]
    public GameObject darkScreenPanel; // Панель с темным экраном (UI Image)
    public float fadeInDuration = 0.5f; // Длительность появления темного экрана
    public float fadeOutDuration = 0.5f; // Длительность исчезновения (если нужно)
    
    [Header("Отладка")]
    public bool debugMode = false;
    
    private Transform playerTransform;
    private Image darkScreenImage;
    private bool hasCaughtPlayer = false;
    private string targetScene = "day7_fin";
    
    void Start()
    {
        // Проверяем, что мы в нужной сцене
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != targetScene)
        {
            if (debugMode)
                Debug.Log($"NPCCatchPlayer: Не в сцене {targetScene}, текущая сцена: {currentScene}. Скрипт отключен.");
            enabled = false;
            return;
        }
        
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("NPCCatchPlayer: Игрок с тегом 'Player' не найден!");
            enabled = false;
            return;
        }
        
        // Настраиваем темный экран
        SetupDarkScreen();
    }
    
    void SetupDarkScreen()
    {
        // Если панель не назначена, создаем её автоматически
        if (darkScreenPanel == null)
        {
            // Ищем Canvas в сцене
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Создаем Canvas если его нет
                GameObject canvasObj = new GameObject("DarkScreenCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // Высокий приоритет, чтобы быть поверх всего
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Создаем панель для темного экрана
            darkScreenPanel = new GameObject("DarkScreenPanel");
            darkScreenPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform rectTransform = darkScreenPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Добавляем Image компонент
            darkScreenImage = darkScreenPanel.AddComponent<Image>();
            darkScreenImage.color = new Color(0, 0, 0, 0); // Начинаем с прозрачного
            
            darkScreenPanel.SetActive(false);
            
            if (debugMode)
                Debug.Log("NPCCatchPlayer: Темный экран создан автоматически.");
        }
        else
        {
            // Получаем Image компонент из назначенной панели
            darkScreenImage = darkScreenPanel.GetComponent<Image>();
            if (darkScreenImage == null)
            {
                darkScreenImage = darkScreenPanel.AddComponent<Image>();
            }
            
            // Устанавливаем начальную прозрачность
            Color startColor = darkScreenImage.color;
            startColor.a = 0;
            darkScreenImage.color = startColor;
            
            darkScreenPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Если уже поймали игрока, не проверяем снова
        if (hasCaughtPlayer)
            return;
        
        // Если используем триггер, проверка происходит в OnTriggerEnter
        if (useTrigger)
            return;
        
        // Проверяем расстояние до игрока
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= catchDistance)
            {
                CatchPlayer();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Если не используем триггер, выходим
        if (!useTrigger)
            return;
        
        // Проверяем, что это игрок
        if (other.CompareTag("Player"))
        {
            CatchPlayer();
        }
    }
    
    void CatchPlayer()
    {
        if (hasCaughtPlayer)
            return;
        
        hasCaughtPlayer = true;
        
        if (debugMode)
            Debug.Log("NPCCatchPlayer: Игрок пойман! Показываем темный экран.");
        
        // Останавливаем движение игрока
        PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
        
        // Показываем темный экран
        ShowDarkScreen();
    }
    
    void ShowDarkScreen()
    {
        if (darkScreenPanel == null || darkScreenImage == null)
        {
            Debug.LogError("NPCCatchPlayer: Темный экран не настроен!");
            return;
        }
        
        darkScreenPanel.SetActive(true);
        StartCoroutine(FadeToBlack());
    }
    
    IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        Color startColor = darkScreenImage.color;
        Color targetColor = new Color(0, 0, 0, 1); // Полностью черный
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / fadeInDuration);
            darkScreenImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // Убеждаемся, что экран полностью черный
        darkScreenImage.color = targetColor;
        
        if (debugMode)
            Debug.Log("NPCCatchPlayer: Темный экран полностью показан.");
    }
    
    // Метод для скрытия темного экрана (если понадобится)
    public void HideDarkScreen()
    {
        if (darkScreenPanel != null && darkScreenImage != null)
        {
            StartCoroutine(FadeFromBlack());
        }
    }
    
    IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        Color startColor = darkScreenImage.color;
        Color targetColor = new Color(0, 0, 0, 0); // Прозрачный
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / fadeOutDuration);
            darkScreenImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        darkScreenImage.color = targetColor;
        darkScreenPanel.SetActive(false);
    }
    
    void OnDrawGizmosSelected()
    {
        // Рисуем радиус обнаружения в редакторе
        if (!useTrigger)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
        }
    }
}

