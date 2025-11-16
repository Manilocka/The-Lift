using System.Collections;
using UnityEngine;

/// <summary>
/// Контроллер камеры для диалогов. Фокусирует камеру на лице NPC только когда говорит NPC.
/// </summary>
public class DialogueCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Скорость плавного перемещения камеры")]
    public float cameraMoveSpeed = 2f;
    
    [Tooltip("Расстояние камеры от NPC по умолчанию (если не задано в NPCDialogue)")]
    public float defaultCameraDistance = 2.5f;
    
    [Tooltip("Смещение камеры по оси Y относительно высоты NPC (если не задана в NPCDialogue)")]
    public float defaultCameraHeightOffset = 0.1f;
    
    private Camera mainCamera;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isCameraFocused = false;
    private NPCDialogue currentNPCDialogue;
    private Transform playerBody;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("DialogueCameraController: Main Camera не найден!");
        }
        
        // Находим тело игрока
        MouseLock mouseLock = FindObjectOfType<MouseLock>();
        if (mouseLock != null)
        {
            playerBody = mouseLock.playerBody;
        }
    }
    
    /// <summary>
    /// Фокусирует камеру на NPC, если текущая строка диалога говорит NPC
    /// </summary>
    public void FocusCameraOnNPCIfNeeded(NPCDialogue npcDialogue, DialogueLine currentLine)
    {
        // Если говорит игрок или это мысль, не фокусируем камеру
        if (currentLine.isPlayer || currentLine.isThought)
        {
            ReleaseCamera();
            return;
        }
        
        // Если говорит NPC, фокусируем камеру
        if (npcDialogue != null && !isCameraFocused)
        {
            StartCoroutine(FocusCameraOnNPC(npcDialogue));
        }
    }
    
    /// <summary>
    /// Освобождает камеру (возвращает в исходное положение)
    /// </summary>
    public void ReleaseCamera()
    {
        if (isCameraFocused)
        {
            StartCoroutine(ReturnCameraToOriginal());
        }
    }
    
    private IEnumerator FocusCameraOnNPC(NPCDialogue npcDialogue)
    {
        if (mainCamera == null || npcDialogue == null)
            yield break;
        
        currentNPCDialogue = npcDialogue;
        
        // Сохраняем исходное положение камеры (только один раз, при первом фокусе)
        if (!isCameraFocused)
        {
            originalCameraParent = mainCamera.transform.parent;
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
            
            // Отвязываем камеру от родителя, если она привязана
            if (originalCameraParent != null)
            {
                mainCamera.transform.SetParent(null);
            }
        }
        
        // Получаем позицию NPC для фокуса
        Vector3 npcPosition = GetNPCPosition(npcDialogue);
        
        // Определяем расстояние камеры от NPC
        float distance = npcDialogue.cameraDistance > 0 ? npcDialogue.cameraDistance : defaultCameraDistance;
        
        // Определяем целевую высоту камеры
        float targetHeightY;
        if (npcDialogue.cameraHeightY != 0f)
        {
            // Используем заданную высоту
            targetHeightY = npcDialogue.cameraHeightY;
        }
        else
        {
            // Используем высоту NPC + смещение
            float heightOffset = npcDialogue.cameraHeightOffset != 0f ? npcDialogue.cameraHeightOffset : defaultCameraHeightOffset;
            targetHeightY = npcPosition.y + heightOffset;
        }
        
        isCameraFocused = true;
        
        // Вычисляем направление от NPC к игроку (или к текущей позиции камеры)
        Vector3 directionFromNPC;
        if (playerBody != null)
        {
            // Направление от NPC к игроку
            directionFromNPC = (playerBody.position - npcPosition).normalized;
        }
        else
        {
            // Направление от NPC к текущей позиции камеры
            directionFromNPC = (mainCamera.transform.position - npcPosition).normalized;
        }
        
        // Если игрок слишком близко, используем направление от NPC к камере
        if (directionFromNPC.magnitude < 0.1f)
        {
            directionFromNPC = -mainCamera.transform.forward;
        }
        
        directionFromNPC.y = 0; // Оставляем только горизонтальное направление
        directionFromNPC.Normalize();
        
        // Позиция камеры: на фиксированном расстоянии от NPC, в направлении от NPC
        Vector3 targetPosition = npcPosition - directionFromNPC * distance;
        targetPosition.y = targetHeightY;
        
        // Вычисляем направление камеры (смотрит на NPC)
        Vector3 lookDirection = (npcPosition - targetPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        
        // Плавно перемещаем камеру
        float elapsedTime = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        // Устанавливаем финальную позицию
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }
    
    /// <summary>
    /// Получает позицию NPC для фокуса камеры
    /// </summary>
    private Vector3 GetNPCPosition(NPCDialogue npcDialogue)
    {
        // Если задана точка фокуса, используем её
        if (npcDialogue.cameraFocusPoint != null)
        {
            return npcDialogue.cameraFocusPoint.position;
        }
        
        // Иначе используем центр NPC
        return npcDialogue.transform.position;
    }
    
    private IEnumerator ReturnCameraToOriginal()
    {
        if (mainCamera == null || !isCameraFocused)
            yield break;
        
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRotation, originalCameraRotation, t);
            
            yield return null;
        }
        
        // Восстанавливаем исходное положение
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
        
        if (originalCameraParent != null)
        {
            mainCamera.transform.SetParent(originalCameraParent);
        }
        
        isCameraFocused = false;
        currentNPCDialogue = null;
    }
    
}

