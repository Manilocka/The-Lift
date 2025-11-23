using UnityEngine;

/// <summary>
/// Helper attached to the PlayerInLiftCheck trigger to stop chase music as soon as the player steps inside.
/// </summary>
public class PlayerInLiftAudioProxy : MonoBehaviour
{
    [SerializeField] private bool stopChaseMusic = true;
    [SerializeField] private bool disableChaser = true;
    [SerializeField] private bool logEvents = false;
    [Header("Additional audio to stop when player enters lift")]
    [Tooltip("Assign any audio sources (scary_music, nachalo_sound, heartbeat_sound etc.) that should stop when player steps into the elevator.")]
    [SerializeField] private AudioSource[] extraSourcesToStop;

    [HideInInspector] public bool enableChaseStop = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!enableChaseStop || !other.CompareTag("Player"))
        {
            return;
        }

        if (stopChaseMusic)
        {
            Day7FinAudioBus.StopChaseMusic();
            if (logEvents)
            {
                Debug.Log("[PlayerInLiftAudioProxy] Chase music stopped.");
            }
        }

        // Stop any additional configured audio sources (e.g. heartbeat, scary music)
        if (extraSourcesToStop != null && extraSourcesToStop.Length > 0)
        {
            foreach (var src in extraSourcesToStop)
            {
                if (src == null) continue;
                if (src.isPlaying)
                {
                    src.Stop();
                    if (logEvents)
                    {
                        Debug.Log($"[PlayerInLiftAudioProxy] Stopped extra audio source: {src.gameObject.name}");
                    }
                }
            }
        }

        if (disableChaser)
        {
            NPCCatchesPlayer catcher = FindObjectOfType<NPCCatchesPlayer>();
            if (catcher != null)
            {
                catcher.enabled = false;
                NPCMovement move = catcher.GetComponent<NPCMovement>();
                if (move != null)
                {
                    move.enabled = false;
                }
            }
        }

        enableChaseStop = false; // trigger only once
    }
}


