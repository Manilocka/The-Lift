using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDiagnostics : MonoBehaviour
{
    [Header("Diagnostics Settings")]
    public bool enableDiagnostics = true;
    public bool logOnStart = true;
    public bool logOnUpdate = false;
    
    [Header("Extreme Position Detection")]
    public float extremeThreshold = 1000f;
    
    void Start()
    {
        if (logOnStart && enableDiagnostics)
        {
            DiagnoseAllUI();
        }
    }
    
    void Update()
    {
        if (logOnUpdate && enableDiagnostics)
        {
            if (Time.frameCount % 60 == 0) // Каждую секунду
            {
                CheckForExtremePositions();
            }
        }
    }
    
    [ContextMenu("Diagnose All UI")]
    public void DiagnoseAllUI()
    {
        Debug.Log("=== UI DIAGNOSTICS START ===");
        
        // Находим все Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Найдено Canvas: {allCanvases.Length}");
        
        foreach (Canvas canvas in allCanvases)
        {
            DiagnoseCanvas(canvas);
        }
        
        // Находим все RectTransform
        RectTransform[] allRectTransforms = FindObjectsOfType<RectTransform>();
        Debug.Log($"Найдено RectTransform: {allRectTransforms.Length}");
        
        int extremeCount = 0;
        foreach (RectTransform rect in allRectTransforms)
        {
            if (HasExtremePosition(rect))
            {
                extremeCount++;
                LogExtremePosition(rect);
            }
        }
        
        Debug.Log($"Элементов с экстремальными позициями: {extremeCount}");
        Debug.Log("=== UI DIAGNOSTICS END ===");
    }
    
    void DiagnoseCanvas(Canvas canvas)
    {
        Debug.Log($"Canvas: {canvas.name}");
        Debug.Log($"  - Render Mode: {canvas.renderMode}");
        Debug.Log($"  - World Camera: {canvas.worldCamera?.name ?? "None"}");
        Debug.Log($"  - Plane Distance: {canvas.planeDistance}");
        Debug.Log($"  - Sorting Order: {canvas.sortingOrder}");
        
        // Проверяем Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Debug.Log($"  - UI Scale Mode: {scaler.uiScaleMode}");
            Debug.Log($"  - Reference Resolution: {scaler.referenceResolution}");
        }
    }
    
    bool HasExtremePosition(RectTransform rect)
    {
        Vector3 position = rect.anchoredPosition;
        return Mathf.Abs(position.x) > extremeThreshold || 
               Mathf.Abs(position.y) > extremeThreshold || 
               Mathf.Abs(position.z) > extremeThreshold;
    }
    
    void LogExtremePosition(RectTransform rect)
    {
        Vector3 position = rect.anchoredPosition;
        Debug.LogWarning($"ЭКСТРЕМАЛЬНАЯ ПОЗИЦИЯ: {rect.name}");
        Debug.LogWarning($"  - Позиция: {position}");
        Debug.LogWarning($"  - Родитель: {rect.parent?.name ?? "None"}");
        Debug.LogWarning($"  - Размер: {rect.sizeDelta}");
        Debug.LogWarning($"  - Якоря: min={rect.anchorMin}, max={rect.anchorMax}");
        Debug.LogWarning($"  - Компоненты: {GetComponentNames(rect)}");
    }
    
    void CheckForExtremePositions()
    {
        RectTransform[] allRectTransforms = FindObjectsOfType<RectTransform>();
        int extremeCount = 0;
        
        foreach (RectTransform rect in allRectTransforms)
        {
            if (HasExtremePosition(rect))
            {
                extremeCount++;
            }
        }
        
        if (extremeCount > 0)
        {
            Debug.LogWarning($"Обнаружено {extremeCount} элементов с экстремальными позициями");
        }
    }
    
    string GetComponentNames(Transform obj)
    {
        var components = obj.GetComponents<Component>();
        var names = new List<string>();
        
        foreach (var comp in components)
        {
            if (comp != null)
            {
                names.Add(comp.GetType().Name);
            }
        }
        
        return string.Join(", ", names);
    }
    
    [ContextMenu("Fix All Extreme Positions")]
    public void FixAllExtremePositions()
    {
        Debug.Log("=== ИСПРАВЛЕНИЕ ЭКСТРЕМАЛЬНЫХ ПОЗИЦИЙ ===");
        
        RectTransform[] allRectTransforms = FindObjectsOfType<RectTransform>();
        int fixedCount = 0;
        
        foreach (RectTransform rect in allRectTransforms)
        {
            if (HasExtremePosition(rect))
            {
                Vector3 oldPosition = rect.anchoredPosition;
                rect.anchoredPosition = Vector3.zero;
                fixedCount++;
                
                Debug.Log($"✅ ИСПРАВЛЕНО: {rect.name}");
                Debug.Log($"   Было: {oldPosition}");
                Debug.Log($"   Стало: (0, 0, 0)");
                Debug.Log($"   Родитель: {rect.parent?.name ?? "None"}");
            }
        }
        
        Debug.Log($"=== ИСПРАВЛЕНО {fixedCount} ЭЛЕМЕНТОВ ===");
    }
}
