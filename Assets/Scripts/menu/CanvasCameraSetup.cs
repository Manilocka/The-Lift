using UnityEngine;
using UnityEngine.UI;

public class CanvasCameraSetup : MonoBehaviour
{
    [Header("Canvas Setup")]
    public Canvas canvas;
    public Camera uiCamera;
    
    [Header("Camera Settings")]
    public float cameraDistance = 1f;
    public float orthographicSize = 5f;
    
    void Start()
    {
        SetupCanvasForCamera();
    }
    
    void SetupCanvasForCamera()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
            
        if (uiCamera == null)
            uiCamera = Camera.main;
        
        // Настраиваем Canvas для работы с камерой
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = cameraDistance;
            
            Debug.Log("CanvasCameraSetup: Canvas настроен для работы с камерой");
        }
        
        // Настраиваем камеру для UI
        if (uiCamera != null)
        {
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = orthographicSize;
            uiCamera.nearClipPlane = 0.1f;
            uiCamera.farClipPlane = 10f;
            
            Debug.Log("CanvasCameraSetup: Камера настроена для UI");
        }
    }
    
    void Update()
    {
        // Постоянно проверяем настройки
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
        }
    }
}
