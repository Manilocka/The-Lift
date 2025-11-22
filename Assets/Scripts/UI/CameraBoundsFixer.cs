using UnityEngine;

public class CameraBoundsFixer : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;
    
    [Header("Safe Bounds")]
    public float safeOrthographicSize = 5f;
    public float safeNearClip = 0.1f;
    public float safeFarClip = 10f;
    
    [Header("Camera Position Limits")]
    public bool enablePositionLimits = true;
    public float maxCameraDistanceFromPlayer = 5f; // Максимальное расстояние камеры от игрока
    public float minCameraHeight = -10f; // Минимальная высота камеры
    public float maxCameraHeight = 50f; // Максимальная высота камеры
    
    private Transform playerTransform;
    
    void Start()
    {
        FixCameraSettings();
        
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            // Пытаемся найти через PlayerMovement
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerTransform = playerMovement.transform;
            }
        }
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
            
            // Ограничиваем позицию камеры, если включено
            if (enablePositionLimits && playerTransform != null)
            {
                ClampCameraPosition();
            }
        }
    }
    
    void ClampCameraPosition()
    {
        Vector3 cameraPos = targetCamera.transform.position;
        Vector3 playerPos = playerTransform.position;
        
        // Ограничиваем высоту камеры
        if (cameraPos.y < minCameraHeight)
        {
            cameraPos.y = minCameraHeight;
            targetCamera.transform.position = cameraPos;
        }
        else if (cameraPos.y > maxCameraHeight)
        {
            cameraPos.y = maxCameraHeight;
            targetCamera.transform.position = cameraPos;
        }
        
        // Ограничиваем расстояние камеры от игрока по горизонтали
        Vector3 horizontalOffset = cameraPos - playerPos;
        horizontalOffset.y = 0; // Игнорируем вертикальное смещение
        
        if (horizontalOffset.magnitude > maxCameraDistanceFromPlayer)
        {
            // Возвращаем камеру ближе к игроку
            Vector3 clampedOffset = horizontalOffset.normalized * maxCameraDistanceFromPlayer;
            cameraPos.x = playerPos.x + clampedOffset.x;
            cameraPos.z = playerPos.z + clampedOffset.z;
            targetCamera.transform.position = cameraPos;
        }
    }
}

