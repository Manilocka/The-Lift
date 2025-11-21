using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Overrides the elevator button behaviour for the day7_fin finale.
/// When the trapped player presses the button the elevator stays put,
/// Batya spawns inside, a breathing sound plays and we force the camera to look at him.
/// </summary>
public class Day7FinElevatorButtonSequence : MonoBehaviour
{
    [Header("Scene binding")]
    [SerializeField] private string targetSceneName = "day7_fin";

    [Header("Dependencies")]
    [SerializeField] private Day7FinElevatorZone elevatorZone;
    [SerializeField] private PlayerInLiftCheck playerInLiftCheck;
    [SerializeField] private ElevatorDoorController doorController;
    [SerializeField] private MouseLock mouseLock;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject runnerToHide;

    [Header("Batya spawn")]
    [SerializeField] private GameObject batyaPrefab;
    [SerializeField] private Transform batyaSpawnPoint;
    [SerializeField] private bool teleportExistingBatya = true;
    [SerializeField] private GameObject existingBatyaObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breathingClip;

    [Header("Timing")]
    [SerializeField] private float spawnDelay = 0.6f;

    [Header("Camera")]
    [SerializeField] private float cameraTurnDuration = 1.2f;
    [SerializeField] private float cameraApproachDistance = 1.5f;
    [SerializeField] private float cameraApproachDuration = 1.0f;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Scene Transition")]
    [SerializeField] private string finalSceneName = "Final";
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float fadeHoldDuration = 0.3f;

    private const string SceneStartDialoguePrefKey = "SceneStartDialogueShown";

    private GameObject spawnedBatya;
    private bool sequenceTriggered;
    private SendElevatorNew legacyButtonLogic;
    private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        legacyButtonLogic = GetComponent<SendElevatorNew>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != targetSceneName)
        {
            enabled = false;
            return;
        }

        if (elevatorZone == null)
        {
            elevatorZone = FindObjectOfType<Day7FinElevatorZone>();
        }

        if (playerInLiftCheck == null)
        {
            playerInLiftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
        }

        if (doorController == null && elevatorZone != null)
        {
            doorController = FindObjectOfType<ElevatorDoorController>();
        }
        else if (doorController == null)
        {
            doorController = FindObjectOfType<ElevatorDoorController>();
        }

        if (mouseLock == null)
        {
            mouseLock = FindObjectOfType<MouseLock>();
        }

        if (playerMovement == null && mouseLock != null && mouseLock.playerBody != null)
        {
            playerMovement = mouseLock.playerBody.GetComponent<PlayerMovement>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (legacyButtonLogic != null)
        {
            legacyButtonLogic.enabled = false;
        }

        if (teleportExistingBatya && existingBatyaObject == null)
        {
            NPCCatchesPlayer catcher = FindObjectOfType<NPCCatchesPlayer>();
            if (catcher != null)
            {
                existingBatyaObject = catcher.gameObject;
            }
        }

        if (runnerToHide == null)
        {
            NPCCatchesPlayer catcher = FindObjectOfType<NPCCatchesPlayer>();
            if (catcher != null)
            {
                runnerToHide = catcher.gameObject;
            }
        }
    }

    private void OnMouseDown()
    {
        if (!enabled || sequenceTriggered)
        {
            return;
        }

        if (!IsPlayerInsideLift())
        {
            Debug.LogWarning("Day7FinElevatorButtonSequence: Player must be inside the elevator.");
            return;
        }

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        sequenceTriggered = true;

        HideRunner();

        ElevatorMoving elevator = doorController != null ? doorController.GetComponentInParent<ElevatorMoving>() : null;
        if (elevator != null)
        {
            elevator.canMove = false;
        }

        if (doorController != null && doorController.isOpen)
        {
            doorController.StopAllCoroutines();
            doorController.CloseDoors();
        }

        ElevatorAudioManager.Instance?.StopElevatorMovement();
        ElevatorAudioManager.Instance?.StopArrivalSound();

        yield return new WaitForSeconds(Mathf.Max(spawnDelay, 0f));

        if (!teleportExistingBatya || !TeleportExistingBatya())
        {
            SpawnBatya();
        }
        AlignBatyaLookToPlayer();

        PlayBreathing();
        yield return FocusCameraAndFadeSequence();
    }

    private void SpawnBatya()
    {
        if (spawnedBatya != null || batyaSpawnPoint == null)
        {
            return;
        }

        if (batyaPrefab == null)
        {
            Debug.LogError("Day7FinElevatorButtonSequence: batyaPrefab is not assigned.");
            return;
        }

        spawnedBatya = Instantiate(batyaPrefab, batyaSpawnPoint.position, batyaSpawnPoint.rotation);
    }

    private bool TeleportExistingBatya()
    {
        if (existingBatyaObject == null || batyaSpawnPoint == null)
        {
            return false;
        }

        spawnedBatya = existingBatyaObject;

        CharacterController cc = spawnedBatya.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        NPCMovement npcMovement = spawnedBatya.GetComponent<NPCMovement>();
        if (npcMovement != null)
        {
            npcMovement.enabled = false;
        }

        NPCCatchesPlayer catcher = spawnedBatya.GetComponent<NPCCatchesPlayer>();
        if (catcher != null)
        {
            catcher.enabled = false;
        }

        spawnedBatya.transform.position = batyaSpawnPoint.position;
        spawnedBatya.transform.rotation = batyaSpawnPoint.rotation;

        if (cc != null)
        {
            cc.enabled = true;
        }

        return true;
    }

    private void PlayBreathing()
    {
        if (audioSource != null && breathingClip != null)
        {
            audioSource.PlayOneShot(breathingClip);
        }
    }

    private IEnumerator FocusCameraAndFadeSequence()
    {
        if (mouseLock == null || spawnedBatya == null)
        {
            yield break;
        }

        if (playerMovement == null && mouseLock.playerBody != null)
        {
            playerMovement = mouseLock.playerBody.GetComponent<PlayerMovement>();
        }

        mouseLock.canLook = false;
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        Transform playerBody = mouseLock.playerBody;
        Vector3 flatDirection = spawnedBatya.transform.position - playerBody.position;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            playerBody.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        }

        Transform cameraTransform = mouseLock.transform;
        Vector3 focusPoint = spawnedBatya.transform.position + lookAtOffset;
        Vector3 direction = (focusPoint - cameraTransform.position).normalized;
        Quaternion startRot = cameraTransform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 startPos = cameraTransform.position;
        Vector3 targetPos = focusPoint - direction * Mathf.Max(cameraApproachDistance, 0.05f);

        float turnTime = Mathf.Max(0.05f, cameraTurnDuration);
        float approachTime = Mathf.Max(0.05f, cameraApproachDuration);
        float elapsed = 0f;

        while (elapsed < turnTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / turnTime);
            cameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < approachTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / approachTime);
            cameraTransform.position = Vector3.Lerp(startPos, targetPos, t);
            cameraTransform.rotation = targetRot;
            yield return null;
        }

        cameraTransform.position = targetPos;
        cameraTransform.rotation = targetRot;

        yield return FadeToBlackAndLoadScene();
    }

    private void AlignBatyaLookToPlayer()
    {
        if (spawnedBatya == null)
        {
            return;
        }

        Transform playerBody = null;
        if (mouseLock != null)
        {
            playerBody = mouseLock.playerBody;
        }

        if (playerBody == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerBody = playerObj.transform;
            }
        }

        if (playerBody == null)
        {
            return;
        }

        Vector3 lookDirection = playerBody.position - spawnedBatya.transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        spawnedBatya.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }

    private IEnumerator FadeToBlackAndLoadScene()
    {
        CanvasGroup group = GetFadeCanvas();
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeDuration));
            group.alpha = t;
            yield return null;
        }
        group.alpha = 1f;

        yield return new WaitForSeconds(Mathf.Max(0f, fadeHoldDuration));

        if (!string.IsNullOrEmpty(finalSceneName))
        {
            DialogueManager.Instance?.ResetAllDialogueProgress();
            PlayerPrefs.DeleteKey(SceneStartDialoguePrefKey);
            PlayerPrefs.Save();
            SceneManager.LoadScene(finalSceneName);
        }
    }

    private void HideRunner()
    {
        if (runnerToHide == null)
        {
            return;
        }

        runnerToHide.SetActive(false);
    }

    private bool IsPlayerInsideLift()
    {
        if (playerInLiftCheck != null)
        {
            return playerInLiftCheck.IsPlayerInsideElevator();
        }

        if (elevatorZone != null)
        {
            return elevatorZone.HasPlayerInside;
        }

        return true; // fallback, чтобы не блокировать сцену при отсутствии триггера
    }

    private CanvasGroup GetFadeCanvas()
    {
        if (fadeCanvasGroup != null)
        {
            return fadeCanvasGroup;
        }

        GameObject canvasObj = new GameObject("Day7FinFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image img = panelObj.AddComponent<Image>();
        img.color = Color.black;

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        return fadeCanvasGroup;
    }
}

