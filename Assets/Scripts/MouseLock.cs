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
    public float maxInteractDistance = 30f;
    public float minInteractDistance = 15f;

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
    }

    void Update()
    {
        // 🔹 Stop raycasting & looking around if dialogue is active
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        // --- Mouse look ---
        if (canLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // --- Raycast from screen center ---
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance))
        {
            if (hit.collider != null && hit.collider.CompareTag("NPC"))
                {
                crosshair.color = highlightColor;

                if (Input.GetMouseButtonDown(0) && crosshair.enabled)
                {
                    Debug.Log("Clicked NPC: " + hit.collider.name);

                    NPCDialogue npcDialogue = hit.collider.GetComponent<NPCDialogue>();
                    if (npcDialogue != null && npcDialogue.dialogue != null)
                    {
                        // Make NPC face player
                        Vector3 lookDirection = playerBody.position - hit.collider.transform.position;
                        lookDirection.y = 0;
                        hit.collider.transform.rotation = Quaternion.LookRotation(lookDirection);

                        // Start DialogueSO conversation
                        DialogueManager.Instance.StartDialogue(npcDialogue.dialogue);

                        // Disable movement & looking
                        PlayerMovement playerMovement = playerBody.GetComponent<PlayerMovement>();
                        if (playerMovement != null)
                        {
                            playerMovement.canMove = false;
                        }
                        canLook = false;

                        // Hide crosshair during dialogue
                        crosshair.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("NPC has no DialogueSO: " + hit.collider.name);
                    }
                }
            }
            else
            {
                crosshair.color = defaultColor;
            }
        }
        else
        {
            crosshair.color = defaultColor;
        }
    }
}
