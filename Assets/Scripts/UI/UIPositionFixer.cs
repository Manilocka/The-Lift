using UnityEngine;
using UnityEngine.UI;

public class UIPositionFixer : MonoBehaviour
{
    [Header("UI Fix Settings")]
    public Canvas targetCanvas;
    public Camera uiCamera;
    
    [Header("Position Limits")]
    public float maxX = 1000f;
    public float maxY = 1000f;
    public float maxZ = 100f; // Уменьшили Z лимит
    
    void Start()
    {
        FixUIPositions();
    }
    
    void FixUIPositions()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponent<Canvas>();
            
        if (uiCamera == null)
            uiCamera = Camera.main;
        
        // Исправляем позиции всех UI элементов
        FixRectTransforms(transform);
        
        // Настраиваем Canvas для правильного отображения
        if (targetCanvas != null)
        {
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Если используется Screen Space Camera, настраиваем правильно
            if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas.worldCamera = uiCamera;
                targetCanvas.planeDistance = 1f;
            }
        }
        
        Debug.Log("UIPositionFixer: UI позиции исправлены");
    }
    
    void FixRectTransforms(Transform parent)
    {
        foreach (Transform child in parent)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Проверяем и исправляем позицию
                Vector3 position = rectTransform.anchoredPosition;
                
                // Проверяем на экстремальные значения
                bool needsReset = false;
                if (Mathf.Abs(position.x) > maxX || Mathf.Abs(position.y) > maxY || Mathf.Abs(position.z) > maxZ)
                {
                    needsReset = true;
                    Debug.LogWarning($"UIPositionFixer: ИСПРАВЛЯЕМ ЭКСТРЕМАЛЬНУЮ ПОЗИЦИЮ!");
                    Debug.LogWarning($"  - Объект: {child.name}");
                    Debug.LogWarning($"  - Позиция: {position}");
                    Debug.LogWarning($"  - Родитель: {child.parent?.name ?? "None"}");
                    Debug.LogWarning($"  - Компоненты: {GetComponentNames(child)}");
                }
                
                if (needsReset)
                {
                    // Сбрасываем в центр экрана
                    rectTransform.anchoredPosition = Vector3.zero;
                }
                else
                {
                    // Ограничиваем позицию разумными пределами
                    position.x = Mathf.Clamp(position.x, -maxX, maxX);
                    position.y = Mathf.Clamp(position.y, -maxY, maxY);
                    position.z = Mathf.Clamp(position.z, -maxZ, maxZ);
                    rectTransform.anchoredPosition = position;
                }
                
                // Проверяем размеры
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = Mathf.Clamp(sizeDelta.x, -2000f, 2000f);
                sizeDelta.y = Mathf.Clamp(sizeDelta.y, -2000f, 2000f);
                rectTransform.sizeDelta = sizeDelta;
                
                // Устанавливаем правильные якоря для экранного пространства
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            
            // Рекурсивно проверяем дочерние элементы
            FixRectTransforms(child);
        }
    }
    
    void Update()
    {
        // Периодически проверяем и исправляем позиции
        if (Time.frameCount % 30 == 0) // Каждые полсекунды
        {
            FixUIPositions();
        }
    }
    
    // Принудительное исправление при включении объекта
    void OnEnable()
    {
        FixUIPositions();
    }
    
    string GetComponentNames(Transform obj)
    {
        var components = obj.GetComponents<Component>();
        var names = new System.Collections.Generic.List<string>();
        
        foreach (var comp in components)
        {
            if (comp != null)
            {
                names.Add(comp.GetType().Name);
            }
        }
        
        return string.Join(", ", names);
    }
}
