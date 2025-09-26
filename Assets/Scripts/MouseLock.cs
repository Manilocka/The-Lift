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
    public float maxInteractDistance = 30f;
    public float minInteractDistance = 15f;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;
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
        Debug.Log("MouseLock started. Camera: " + (mainCamera != null ? "Found" : "Not found"));
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
        Debug.Log("Raycast started. Position: " + ray.origin);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.collider?.name + " with tag: " + hit.collider?.tag);
            float distance = Vector3.Distance(playerBody.position, hit.collider.transform.position);
            Debug.Log("Distance to hit: " + distance);

            // Простая проверка цвета без строгого диапазона для теста
            if (hit.collider != null && hit.collider.CompareTag("NPC"))
            {
                crosshair.color = highlightColor;
                Debug.Log("NPC detected. Setting highlight color.");
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Left click on: " + hit.collider.name);
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
                        Debug.Log("No NPCDialogue or dialogue on: " + hit.collider.name);
                    }
                }
            }
            else
            {
                crosshair.color = defaultColor;
                Debug.Log("No NPC tag or not an NPC.");
            }
        }
        else
        {
            Debug.Log("Raycast missed");
            crosshair.color = defaultColor;
        }
    }
}