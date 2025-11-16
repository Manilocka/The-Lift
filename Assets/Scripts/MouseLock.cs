using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseLock : MonoBehaviour
{
    [Header("Mouse Look")]
    public float mouseSensitivity = 500f;
    public Transform playerBody;
    private float xRotation = 0f;

    [Header("Crosshair")]
    public Image crosshair;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;


    [Header("Interaction")]
    public float maxInteractDistance = 170.0f;
    public float sphereCastRadius = 0.5f;
    // --- THIS IS THE FIX ---
    // We create a LayerMask to tell the SphereCast which layers to check.
    // This will let us ignore the Player layer.
    public LayerMask interactableLayer;

    private Camera mainCamera;
    public bool canLook = true;
    private Dictionary<Type, Action<MonoBehaviour, RaycastHit>> clickActions = new Dictionary<Type, Action<MonoBehaviour, RaycastHit>>();

    void Start()
    {
        clickActions.Add(typeof(Door), (obj, hit) => ((Door)obj).Interact());
        clickActions.Add(typeof(FloorDoorController), (obj, hit) => ((FloorDoorController)obj).OpenDoors());
        clickActions.Add(typeof(CallElevator), (obj, hit) => sendClick(obj));
        clickActions.Add(typeof(SendElevatorButton), (obj, hit) => sendClick(obj));
        clickActions.Add(typeof(SendElevatorNew), (obj, hit) => sendClick(obj));
        clickActions.Add(typeof(NPCDialogue), (obj, hit) => interactNpc((NPCDialogue)obj, hit));
        clickActions.Add(typeof(LevelChange), (obj, hit) => sendClick(obj));

        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please assign a camera with the tag 'MainCamera'.");
        }
        if (crosshair == null)
        {
            Debug.LogError("Crosshair Image not assigned! Please drag the Image into the Inspector.");
        }
        crosshair.color = defaultColor;

        // Принудительно устанавливаем дистанцию
        maxInteractDistance = 170.0f;
        
        // Загружаем настройки чувствительности мыши
        LoadMouseSensitivitySettings();
    }

    void Update()
    {
        // Проверяем интерактивность и обновляем цвет crosshair ПЕРЕД блокировкой диалога
        bool isInteractable = hasInteractable(out RaycastHit hit);
        crosshair.color = isInteractable ? highlightColor : defaultColor;

        // Блокируем взаимодействие во время диалога
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            // Если диалог с NPC, блокируем камеру
            // Если диалог не с NPC (мысли, монологи), камера остается активной
            if (DialogueManager.Instance.currentNPCDialogue != null)
            {
                // Диалог с NPC - блокируем камеру
                return;
            }
            else
            {
                // Диалог не с NPC - камера остается активной, но взаимодействие блокируем
                // Продолжаем выполнение для обработки движения камеры
            }
        }

        if (canLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        if (!isInteractable) return;

        if (!Input.GetMouseButtonDown(0)) return;

        var interactable = hit.collider.GetComponent<MonoBehaviour>();

        if (clickActions.TryGetValue(interactable.GetType(), out Action<MonoBehaviour, RaycastHit> action)) {
            action.Invoke(interactable, hit);
        }
    }

    bool hasInteractable(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.SphereCast(ray, sphereCastRadius, out hit, maxInteractDistance, interactableLayer))
        {
            return true;
        }

        else if (Physics.Raycast(ray, out hit, maxInteractDistance, interactableLayer))
        {
            return true;
        }

        else if (Physics.SphereCast(ray, sphereCastRadius, out hit, maxInteractDistance))
        {
            var found = hit.collider.GetComponent<MonoBehaviour>();
            if (found == null) return false;
            return clickActions.ContainsKey(found.GetType());
        }
        return false;
    }

    static void sendClick(MonoBehaviour obj)
    {
        obj.SendMessage("OnMouseDown");
    }

    void interactNpc(NPCDialogue npcDialogue, RaycastHit hit)
    {
        if (npcDialogue != null && npcDialogue.dialogue != null)
        {
            // Проверяем, не активен ли уже диалог
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                // Debug.Log("Диалог уже активен, игнорируем повторное взаимодействие");
                return;
            }

            // Проверяем, можно ли говорить с этим NPC
            if (!npcDialogue.CanTalk())
            {
                Debug.Log($"С NPC {npcDialogue.gameObject.name} уже говорили, показываем tiredDialogue");
                
                // Сохраняем ссылку на текущий NPC в DialogueManager (чтобы камера блокировалась)
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetCurrentNPC(npcDialogue);
                }
                
                // Показываем tiredDialogue вместо блокировки
                if (DialogueManager.Instance != null && DialogueManager.Instance.tiredDialogue != null)
                {
                    DialogueManager.Instance.StartTiredDialogue();
                }
                else
                {
                    Debug.LogError("tiredDialogue не назначен в DialogueManager!");
                }
            }
                else
                {
                    Debug.Log($"Начинаем диалог с NPC: {npcDialogue.dialogue.name}");

                    // Поворачиваем NPC к игроку только если NPC готов говорить
                    Vector3 lookDirection = playerBody.position - hit.collider.transform.position;
                    lookDirection.y = 0;
                    hit.collider.transform.rotation = Quaternion.LookRotation(lookDirection);

                    // Сохраняем ссылку на текущий NPC в DialogueManager
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.SetCurrentNPC(npcDialogue);
                    }

                    DialogueManager.Instance.StartDialogue(npcDialogue.dialogue);

                    // Отмечаем, что с этим NPC уже говорили
                    npcDialogue.MarkAsTalkedTo();
                }

            PlayerMovement playerMovement = playerBody.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.canMove = false;
            }
            canLook = false;
            crosshair.gameObject.SetActive(false);
            return;
        }
    }
    
    void LoadMouseSensitivitySettings()
    {
        // Загружаем чувствительность мыши из PlayerPrefs
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivity = savedSensitivity * 500f; // Конвертируем из диапазона 0-2 в 0-1000
        Debug.Log($"MouseLock: Загружена чувствительность мыши: {savedSensitivity} (конвертировано в {mouseSensitivity})");
    }
    
    public void UpdateMouseSensitivity(float sensitivity)
    {
        // Обновляем чувствительность мыши
        mouseSensitivity = sensitivity * 500f; // Конвертируем из диапазона 0-2 в 0-1000
        Debug.Log($"MouseLock: Обновлена чувствительность мыши: {sensitivity} (конвертировано в {mouseSensitivity})");
    }
}
