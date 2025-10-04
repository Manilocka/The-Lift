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
    public float maxInteractDistance = 1000f; // Увеличено до 1000 для лучшей видимости
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
        
        // Debug: Log the interactable layer mask
        Debug.Log("Interactable Layer Mask: " + interactableLayer.value);
        Debug.Log("Max Interact Distance: " + maxInteractDistance);
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
        
        // Debug: Always try a regular raycast first to see if we're hitting anything
        if (Physics.Raycast(ray, out RaycastHit debugHit, maxInteractDistance))
        {
            Debug.Log("Raycast hit: " + debugHit.collider.name + " on layer " + debugHit.collider.gameObject.layer + " (Collider type: " + debugHit.collider.GetType().Name + ")");
        }
        
        // Try with LayerMask first
        if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit hit, maxInteractDistance, interactableLayer))
        {
            Debug.Log("SphereCast with LayerMask hit: " + hit.collider.name);
            crosshair.color = highlightColor;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clicked Interactable: " + hit.collider.name);
                
                // Проверяем на Door компонент
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                {
                    door.Interact();
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
                else
                {
                    Debug.Log("Interactable has no DialogueSO: " + hit.collider.name);
                }
            }
        }
        // Дополнительная проверка для дверей через обычный Raycast (для Mesh Collider)
        else if (Physics.Raycast(ray, out RaycastHit doorHit, maxInteractDistance, interactableLayer))
        {
            Door door = doorHit.collider.GetComponent<Door>();
            if (door != null)
            {
                Debug.Log("Raycast hit Door: " + doorHit.collider.name);
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clicked Door via Raycast: " + doorHit.collider.name);
                    door.Interact();
                }
            }
        }
        // Fallback: Try without LayerMask if the above doesn't work
        else if (Physics.SphereCast(ray, sphereCastRadius, out RaycastHit fallbackHit, maxInteractDistance))
        {
            Debug.Log("SphereCast without LayerMask hit: " + fallbackHit.collider.name);
            
            // Проверяем на Door компонент
            Door door = fallbackHit.collider.GetComponent<Door>();
            if (door != null)
            {
                crosshair.color = highlightColor;
                
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clicked Door (fallback): " + fallbackHit.collider.name);
                    door.Interact();
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
                    Debug.Log("Clicked Interactable (fallback): " + fallbackHit.collider.name);
                    
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
                    else
                    {
                        Debug.Log("Interactable has no DialogueSO (fallback): " + fallbackHit.collider.name);
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
