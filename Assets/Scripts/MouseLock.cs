using System.Collections;
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

    void Start()
    {
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
    }

    void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            return;
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

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        // Try with LayerMask first
        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, maxInteractDistance, interactableLayer))
        {
            crosshair.color = highlightColor;

            if (Input.GetMouseButtonDown(0))
            {
                // Проверяем на Door компонент
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                {
                    door.Interact();
                    return;
                }
                
                // Проверяем на кнопки лифта
                CallElevator callElevator = hit.collider.GetComponent<CallElevator>();
                if (callElevator != null)
                {
                    // Вызываем Update() кнопки для обработки клика
                    callElevator.SendMessage("OnMouseDown");
                    return;
                }
                
                SendElevatorButton sendButton = hit.collider.GetComponent<SendElevatorButton>();
                if (sendButton != null)
                {
                    // Вызываем Update() кнопки для обработки клика
                    sendButton.SendMessage("OnMouseDown");
                    return;
                }
                
                TriggerButton triggerButton = hit.collider.GetComponent<TriggerButton>();
                if (triggerButton != null)
                {
                    // Вызываем Update() кнопки для обработки клика
                    triggerButton.SendMessage("OnMouseDown");
                    return;
                }
                
                // Проверяем на NPCDialogue компонент
                NPCDialogue npcDialogue = hit.collider.GetComponent<NPCDialogue>();
                if (npcDialogue != null && npcDialogue.dialogue != null)
                {
                    Vector3 lookDirection = playerBody.position - hit.collider.transform.position;
                    lookDirection.y = 0;
                    hit.collider.transform.rotation = Quaternion.LookRotation(lookDirection);

                    DialogueManager.Instance.StartDialogue(npcDialogue.dialogue);

                    PlayerMovement playerMovement = playerBody.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.canMove = false;
                    }
                    canLook = false;
                    crosshair.gameObject.SetActive(false);
                }
            }
        }
        // Дополнительная проверка для дверей и кнопок через обычный Raycast (для Mesh Collider)
        else if (Physics.Raycast(ray, out RaycastHit doorHit, maxInteractDistance, interactableLayer))
        {
            Door door = doorHit.collider.GetComponent<Door>();
            if (door != null)
            {
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    door.Interact();
                }
            }
            else
            {
                // Проверяем на кнопки лифта
                CallElevator callElevator = doorHit.collider.GetComponent<CallElevator>();
                SendElevatorButton sendButton = doorHit.collider.GetComponent<SendElevatorButton>();
                TriggerButton triggerButton = doorHit.collider.GetComponent<TriggerButton>();
                
                if (callElevator != null || sendButton != null || triggerButton != null)
                {
                    crosshair.color = highlightColor;
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (callElevator != null) callElevator.SendMessage("OnMouseDown");
                        if (sendButton != null) sendButton.SendMessage("OnMouseDown");
                        if (triggerButton != null) triggerButton.SendMessage("OnMouseDown");
                    }
                }
            }
        }
        // Fallback: Try without LayerMask if the above doesn't work
        else if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit fallbackHit, maxInteractDistance))
        {
            // Проверяем на Door компонент
            Door door = fallbackHit.collider.GetComponent<Door>();
            if (door != null)
            {
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    door.Interact();
                }
                return;
            }
            
            // Проверяем на кнопки лифта
            CallElevator callElevator = fallbackHit.collider.GetComponent<CallElevator>();
            SendElevatorButton sendButton = fallbackHit.collider.GetComponent<SendElevatorButton>();
            TriggerButton triggerButton = fallbackHit.collider.GetComponent<TriggerButton>();
            
            if (callElevator != null || sendButton != null || triggerButton != null)
            {
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    if (callElevator != null) callElevator.SendMessage("OnMouseDown");
                    if (sendButton != null) sendButton.SendMessage("OnMouseDown");
                    if (triggerButton != null) triggerButton.SendMessage("OnMouseDown");
                }
                return;
            }
            
            // Проверяем на NPCDialogue компонент
            NPCDialogue npcDialogue = fallbackHit.collider.GetComponent<NPCDialogue>();
            if (npcDialogue != null)
            {
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    if (npcDialogue.dialogue != null)
                    {
                        Vector3 lookDirection = playerBody.position - fallbackHit.collider.transform.position;
                        lookDirection.y = 0;
                        fallbackHit.collider.transform.rotation = Quaternion.LookRotation(lookDirection);

                        DialogueManager.Instance.StartDialogue(npcDialogue.dialogue);

                        PlayerMovement playerMovement = playerBody.GetComponent<PlayerMovement>();
                        if (playerMovement != null)
                        {
                            playerMovement.canMove = false;
                        }
                        canLook = false;
                        crosshair.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            // If the SphereCast hits nothing on the interactable layer, reset the crosshair.
            crosshair.color = defaultColor;
        }
    }
}
