using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPCCatchesPlayer : MonoBehaviour
{
    [Header("Настройки обнаружения")]
    public float catchDistance = 80f;
    public bool useTrigger = true;
    
    [Header("Настройки темного экрана")]
    public GameObject darkScreenPanel;
    public float fadeInDuration = 0.5f;
    
    [Header("Отладка")]
    public bool debugMode = true;
    
    private Transform playerTransform;
    private Image darkScreenImage;
    private bool hasCaughtPlayer = false;
    private string targetScene = "day7_fin";
    
    void Start()
    {
        Debug.Log("=== NPCCatchesPlayer START ===");
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"NPCCatchesPlayer: Текущая сцена: '{currentScene}', целевая сцена: '{targetScene}'");
        
        if (currentScene != targetScene)
        {
            Debug.Log($"NPCCatchesPlayer: Не в целевой сцеце! Отключаем скрипт.");
            enabled = false;
            return;
        }
        
        Debug.Log($"NPCCatchesPlayer: Мы в целевой сцене! Продолжаем инициализацию...");
        
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"NPCCatchesPlayer: Игрок найден: {player.name}, позиция: {playerTransform.position}");
        }
        else
        {
            Debug.LogError("NPCCatchesPlayer: Игрок с тегом 'Player' не найден!");
            enabled = false;
            return;
        }
        
        // Проверяем триггер
        if (useTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log($"NPCCatchesPlayer: Коллайдер найден: {col.name}, isTrigger: {col.isTrigger}");
            }
            else
            {
                Debug.LogError("NPCCatchesPlayer: useTrigger = true, но коллайдер не найден!");
            }
        }
        
        // Создаем темный экран
        CreateDarkScreenImmediately();
        Debug.Log("=== NPCCatchesPlayer START ЗАВЕРШЕН ===");
    }
    
    void CreateDarkScreenImmediately()
    {
        Debug.Log("NPCCatchesPlayer: Создаем темный экран...");
        
        if (darkScreenPanel != null)
        {
            Debug.Log("NPCCatchesPlayer: Используем назначенную панель");
            darkScreenImage = darkScreenPanel.GetComponent<Image>();
            if (darkScreenImage == null)
            {
                darkScreenImage = darkScreenPanel.AddComponent<Image>();
                Debug.Log("NPCCatchesPlayer: Добавлен Image компонент к существующей панели");
            }
        }
        else
        {
            Debug.Log("NPCCatchesPlayer: Создаем новый Canvas и панель");
            
            // Создаем Canvas
            GameObject canvasObj = new GameObject("EmergencyDarkScreenCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Создаем панель
            darkScreenPanel = new GameObject("EmergencyDarkScreen");
            darkScreenPanel.transform.SetParent(canvasObj.transform, false);
            
            // Настраиваем RectTransform
            RectTransform rt = darkScreenPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            // Добавляем Image
            darkScreenImage = darkScreenPanel.AddComponent<Image>();
            darkScreenImage.color = new Color(0, 0, 0, 0); // Начинаем прозрачным
            darkScreenImage.raycastTarget = false;
            
            Debug.Log("NPCCatchesPlayer: Canvas и панель созданы");
        }
        
        // Начальное состояние
        darkScreenImage.color = new Color(0, 0, 0, 0);
        darkScreenPanel.SetActive(false); // Сначала неактивна
        
        Debug.Log($"NPCCatchesPlayer: Темный экран готов. Panel: {darkScreenPanel.name}, Image: {darkScreenImage != null}");
    }
    
    void Update()
    {
        if (hasCaughtPlayer)
            return;
        
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (debugMode && Time.frameCount % 60 == 0) // Логируем каждую секунду
            {
                Debug.Log($"NPCCatchesPlayer: Расстояние до игрока: {distance:F2}, порог: {catchDistance}");
            }
            
            if (distance <= catchDistance)
            {
                Debug.Log($"NPCCatchesPlayer: Игрок в зоне обнаружения! Расстояние: {distance:F2}");
                CatchPlayer();
            }
        }
        else if (debugMode && Time.frameCount % 60 == 0)
        {
            Debug.LogWarning("NPCCatchesPlayer: playerTransform is null!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!hasCaughtPlayer && other.CompareTag("Player"))
        {
            Debug.Log($"NPCCatchesPlayer: OnTriggerEnter с игроком! Объект: {other.gameObject.name}");
            CatchPlayer();
        }
    }
    
    void CatchPlayer()
    {
        if (hasCaughtPlayer)
        {
            Debug.Log("NPCCatchesPlayer: Игрок уже пойман, игнорируем");
            return;
        }
        
        hasCaughtPlayer = true;
        Debug.Log("=== NPCCatchesPlayer: ИГРОК ПОЙМАН! ===");
        
        // Останавливаем игрока
        if (playerTransform != null)
        {
            PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.canMove = false;
                Debug.Log("NPCCatchesPlayer: Движение игрока остановлено");
            }
            else
            {
                Debug.LogWarning("NPCCatchesPlayer: PlayerMovement не найден на игроке");
            }
        }
        
        // Показываем темный экран
        ShowDarkScreenNOW();
    }
    
    void ShowDarkScreenNOW()
    {
        Debug.Log("NPCCatchesPlayer: Показываем темный экран...");
        
        if (darkScreenPanel == null || darkScreenImage == null)
        {
            Debug.LogError("NPCCatchesPlayer: Компоненты темного экрана не найдены! Пересоздаем...");
            CreateDarkScreenImmediately();
            
            if (darkScreenPanel == null || darkScreenImage == null)
            {
                Debug.LogError("NPCCatchesPlayer: Не удалось создать темный экран!");
                return;
            }
        }
        
        // Активируем панель
        darkScreenPanel.SetActive(true);
        
        // Устанавливаем черный цвет
        darkScreenImage.color = Color.black;
        
        // Настраиваем Canvas
        Canvas canvas = darkScreenPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
            canvas.enabled = true;
            Debug.Log($"NPCCatchesPlayer: Canvas настроен: {canvas.name}, sortingOrder: {canvas.sortingOrder}");
        }
        
        // Принудительное обновление
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"NPCCatchesPlayer: Темный экран показан! Panel.active: {darkScreenPanel.activeSelf}, Color: {darkScreenImage.color}");
        
        // Запускаем проверку
        StartCoroutine(VerifyDarkScreen());
    }
    
    IEnumerator VerifyDarkScreen()
    {
        Debug.Log("NPCCatchesPlayer: Запуск проверки темного экрана...");
        
        yield return new WaitForEndOfFrame();
        
        if (darkScreenPanel != null && darkScreenImage != null)
        {
            Debug.Log($"NPCCatchesPlayer: Проверка - Panel.active: {darkScreenPanel.activeSelf}, Color: {darkScreenImage.color}");
            
            if (darkScreenImage.color.a < 0.9f)
            {
                Debug.LogWarning("NPCCatchesPlayer: Alpha низкий! Исправляем...");
                darkScreenImage.color = Color.black;
            }
        }
        else
        {
            Debug.LogError("NPCCatchesPlayer: Темный экран пропал при проверке!");
        }
        
        Debug.Log("NPCCatchesPlayer: Проверка завершена");
    }
    
    // Метод для принудительного тестирования
    [ContextMenu("Тест поимки игрока")]
    public void TestCatchPlayer()
    {
        Debug.Log("=== ТЕСТОВАЯ ПОИМКА ИГРОКА ===");
        CatchPlayer();
    }
    
    void OnDrawGizmosSelected()
    {
        if (!useTrigger)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, catchDistance);
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("NPCCatchesPlayer: Скрипт уничтожен");
    }
}