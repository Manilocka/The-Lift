using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// При первом проходе через BatyaRoomNoEscape показывает черный экран и блокирует движение игрока.
/// Диалоги продолжают работать нормально.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BatyaRoomNoEscapeBlocker : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "day7_e";
    
    [Header("Dark Screen Settings")]
    [Tooltip("Панель для черного экрана (если не назначена, будет создана автоматически)")]
    [SerializeField] private GameObject darkScreenPanel;
    
    private Image darkScreenImage;
    private bool hasTriggered = false; // Флаг, что уже сработало
    private PlayerMovement playerMovement;
    
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Проверяем, что мы в правильной сцене
        if (currentScene != targetSceneName)
        {
            enabled = false;
            return;
        }
        
        // Убеждаемся, что коллайдер - триггер
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Находим компонент движения игрока заранее
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        
        Debug.Log($"BatyaRoomNoEscapeBlocker: Инициализирован в сцене {targetSceneName}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return; // Уже сработало, игнорируем
        }
        
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        Debug.Log("BatyaRoomNoEscapeBlocker: Игрок прошел через BatyaRoomNoEscape в первый раз!");
        hasTriggered = true;
        
        // Отключаем движение игрока
        if (playerMovement == null)
        {
            playerMovement = other.GetComponent<PlayerMovement>();
        }
        
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
            Debug.Log("BatyaRoomNoEscapeBlocker: Движение игрока отключено");
        }
        else
        {
            Debug.LogWarning("BatyaRoomNoEscapeBlocker: PlayerMovement не найден на игроке!");
        }
        
        // Показываем черный экран с небольшой задержкой, чтобы диалог успел запуститься
        StartCoroutine(ShowDarkScreenWithDelay());
    }
    
    IEnumerator ShowDarkScreenWithDelay()
    {
        // Небольшая задержка, чтобы диалог успел запуститься и отобразиться
        yield return new WaitForSeconds(0.2f);
        ShowDarkScreen();
    }
    
    void ShowDarkScreen()
    {
        Debug.Log("BatyaRoomNoEscapeBlocker: Показываем черный экран...");
        
        if (darkScreenPanel == null || darkScreenImage == null)
        {
            CreateDarkScreen();
        }
        
        if (darkScreenPanel != null && darkScreenImage != null)
        {
            darkScreenPanel.SetActive(true);
            darkScreenImage.color = Color.black;
            
            // Настраиваем Canvas
            Canvas canvas = darkScreenPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 0; // Низкий порядок, чтобы диалоги были поверх
                canvas.enabled = true;
            }
            
            Canvas.ForceUpdateCanvases();
            Debug.Log("BatyaRoomNoEscapeBlocker: Черный экран показан");
            
            // Запускаем проверку
            StartCoroutine(VerifyDarkScreen());
        }
        else
        {
            Debug.LogError("BatyaRoomNoEscapeBlocker: Не удалось создать черный экран!");
        }
    }
    
    void CreateDarkScreen()
    {
        Debug.Log("BatyaRoomNoEscapeBlocker: Создаем черный экран...");
        
        if (darkScreenPanel != null)
        {
            darkScreenImage = darkScreenPanel.GetComponent<Image>();
            if (darkScreenImage == null)
            {
                darkScreenImage = darkScreenPanel.AddComponent<Image>();
            }
            return;
        }
        
        // Создаем Canvas
        GameObject canvasObj = new GameObject("BatyaRoomDarkScreenCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0; // Низкий порядок, чтобы диалоги были поверх
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Создаем панель
        darkScreenPanel = new GameObject("BatyaRoomDarkScreen");
        darkScreenPanel.transform.SetParent(canvasObj.transform, false);
        
        // Настраиваем RectTransform
        RectTransform rt = darkScreenPanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Добавляем Image
        darkScreenImage = darkScreenPanel.AddComponent<Image>();
        darkScreenImage.color = Color.black;
        
        // Отключаем raycast, чтобы не мешать диалогам
        darkScreenImage.raycastTarget = false;
        
        Debug.Log("BatyaRoomNoEscapeBlocker: Черный экран создан");
    }
    
    IEnumerator VerifyDarkScreen()
    {
        yield return new WaitForEndOfFrame();
        
        if (darkScreenPanel != null && darkScreenImage != null)
        {
            if (darkScreenImage.color.a < 0.9f)
            {
                darkScreenImage.color = Color.black;
            }
            
            if (!darkScreenPanel.activeSelf)
            {
                darkScreenPanel.SetActive(true);
            }
        }
    }
    
    // Метод для сброса (если нужно)
    public void Reset()
    {
        hasTriggered = false;
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }
        if (darkScreenPanel != null)
        {
            darkScreenPanel.SetActive(false);
        }
        Debug.Log("BatyaRoomNoEscapeBlocker: Сброшен");
    }
}

