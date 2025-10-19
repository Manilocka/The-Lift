using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EmergencyUIFixer : MonoBehaviour
{
    [Header("Emergency UI Fix")]
    public bool autoFixOnStart = true;
    public bool fixOnUpdate = true;
    
    [Header("Extreme Position Detection")]
    public float extremeThreshold = 10000f;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            EmergencyFixAllUI();
        }
    }
    
    void Update()
    {
        if (fixOnUpdate)
        {
            // Проверяем каждые 10 кадров
            if (Time.frameCount % 10 == 0)
            {
                CheckAndFixExtremePositions();
            }
        }
    }
    
    [ContextMenu("Emergency Fix All UI")]
    public void EmergencyFixAllUI()
    {
        Debug.Log("EmergencyUIFixer: Начинаем экстренное исправление UI");
        
        // Находим все Canvas в сцене
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in allCanvases)
        {
            FixCanvasUI(canvas);
        }
        
        Debug.Log("EmergencyUIFixer: Экстренное исправление завершено");
    }
    
    void FixCanvasUI(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Исправляем настройки Canvas
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Исправляем все UI элементы в Canvas
        FixAllRectTransforms(canvas.transform);
    }
    
    void FixAllRectTransforms(Transform parent)
    {
        foreach (Transform child in parent)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 position = rectTransform.anchoredPosition;
                
                // Проверяем на экстремальные значения
                if (Mathf.Abs(position.x) > extremeThreshold || 
                    Mathf.Abs(position.y) > extremeThreshold || 
                    Mathf.Abs(position.z) > extremeThreshold)
                {
                    Debug.LogWarning($"EmergencyUIFixer: Исправляем экстремальную позицию {position} для {child.name}");
                    
                    // Сбрасываем позицию
                    rectTransform.anchoredPosition = Vector3.zero;
                    
                    // Устанавливаем правильные якоря
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                }
            }
            
            // Рекурсивно проверяем дочерние элементы
            FixAllRectTransforms(child);
        }
    }
    
    void CheckAndFixExtremePositions()
    {
        // Находим все RectTransform в сцене
        RectTransform[] allRectTransforms = FindObjectsOfType<RectTransform>();
        
        foreach (RectTransform rectTransform in allRectTransforms)
        {
            Vector3 position = rectTransform.anchoredPosition;
            
            if (Mathf.Abs(position.x) > extremeThreshold || 
                Mathf.Abs(position.y) > extremeThreshold || 
                Mathf.Abs(position.z) > extremeThreshold)
            {
                Debug.LogWarning($"EmergencyUIFixer: Обнаружена экстремальная позиция {position} для {rectTransform.name}");
                
                // Сбрасываем позицию
                rectTransform.anchoredPosition = Vector3.zero;
            }
        }
    }
    
    // Публичный метод для вызова из других скриптов
    public void ForceFixUI()
    {
        EmergencyFixAllUI();
    }
}

