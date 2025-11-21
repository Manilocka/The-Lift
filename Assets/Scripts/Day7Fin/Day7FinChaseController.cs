using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the pursuit fail-state logic that is unique for the day7_fin scene.
/// When the pursuing NPC catches the player we play an audio cue and reload the scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Day7FinChaseController : MonoBehaviour
{
    [Header("Scene binding")]
    [SerializeField] private string targetSceneName = "day7_fin";

    [Header("Chase setup")]
    [SerializeField] private Transform player;
    [SerializeField] private float catchDistance = 40f;
    [SerializeField] private bool useTriggerOnly = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private float reloadDelay = 1.25f;

    private bool playerCaught;
    private bool playerIsSafe;
    private Day7FinElevatorZone elevatorZone;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        Day7FinAudioBus.RegisterChaseSource(audioSource);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != targetSceneName)
        {
            enabled = false;
            return;
        }

        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        elevatorZone = FindObjectOfType<Day7FinElevatorZone>();
        if (elevatorZone != null)
        {
            elevatorZone.PlayerReachedElevator += HandlePlayerReachedElevator;
        }
    }

    private void Update()
    {
        if (!enabled || playerCaught || playerIsSafe || useTriggerOnly)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        var distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            CatchPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled || playerCaught || playerIsSafe)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        CatchPlayer();
    }

    private void CatchPlayer()
    {
        if (playerCaught || playerIsSafe)
        {
            return;
        }

        playerCaught = true;

        if (audioSource != null && catchClip != null)
        {
            audioSource.PlayOneShot(catchClip);
        }

        StartCoroutine(ReloadSceneAfterDelay());
    }

    private IEnumerator ReloadSceneAfterDelay()
    {
        if (reloadDelay > 0f)
        {
            yield return new WaitForSeconds(reloadDelay);
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void HandlePlayerReachedElevator()
    {
        playerIsSafe = true;
    }

    private void OnDestroy()
    {
        if (elevatorZone != null)
        {
            elevatorZone.PlayerReachedElevator -= HandlePlayerReachedElevator;
        }
    }
}

