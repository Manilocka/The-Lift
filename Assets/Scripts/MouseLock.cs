using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseLock : MonoBehaviour
{
    public float mouseSensitivity = 500f;
    public Transform playerBody;
    float xRotation = 0f;

    public Image crosshair;
    public float maxInteractDistance = 5f;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;
    private Camera mainCamera;
    private bool isHoveringNPC = false;
    public bool canLook = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera not found!");
        if (crosshair == null) Debug.LogError("Crosshair not assigned!");
        crosshair.color = defaultColor;
    }

    void Update()
    {
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
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance))
        {
            if (hit.collider != null && hit.collider.CompareTag("NPC"))
            {
                Debug.Log("NPC hit!");
                isHoveringNPC = true;
                crosshair.color = highlightColor;
                if (Input.GetMouseButtonDown(1) && isHoveringNPC) // Только при жёлтой точке
                {
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
                        canLook = false; // Фиксируем камеру
                        crosshair.gameObject.SetActive(false); // Убираем crosshair
                    }
                }
            }
            else
            {
                isHoveringNPC = false;
                crosshair.color = defaultColor;
            }
        }
        else
        {
            isHoveringNPC = false;
            crosshair.color = defaultColor;
        }
    }
}