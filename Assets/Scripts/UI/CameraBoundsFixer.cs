using UnityEngine;

public class CameraBoundsFixer : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;
    
    [Header("Safe Bounds")]
    public float safeOrthographicSize = 5f;
    public float safeNearClip = 0.1f;
    public float safeFarClip = 10f;
    
    void Start()
    {
        FixCameraSettings();
    }
    
    void FixCameraSettings()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
            
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        if (targetCamera != null)
        {
            // Устанавливаем безопасные настройки камеры
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = safeOrthographicSize;
            targetCamera.nearClipPlane = safeNearClip;
            targetCamera.farClipPlane = safeFarClip;
            
            // Устанавливаем правильный viewport
            targetCamera.rect = new Rect(0, 0, 1, 1);
            
            Debug.Log("CameraBoundsFixer: Настройки камеры исправлены");
        }
    }
    
    void Update()
    {
        // Постоянно проверяем настройки камеры
        if (targetCamera != null)
        {
            // Исправляем viewport если он неправильный
            if (targetCamera.rect.x != 0 || targetCamera.rect.y != 0 || 
                targetCamera.rect.width != 1 || targetCamera.rect.height != 1)
            {
                targetCamera.rect = new Rect(0, 0, 1, 1);
            }
            
            // Исправляем orthographic size если он слишком большой
            if (targetCamera.orthographicSize > 20f)
            {
                targetCamera.orthographicSize = safeOrthographicSize;
            }
        }
    }
}
