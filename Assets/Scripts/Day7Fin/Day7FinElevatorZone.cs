using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the elevator safe zone flow that is specific for the day7_fin scene:
/// - opens the elevator doors on start
/// - detects when the player reaches the elevator
/// - instantly closes the doors to lock the player inside
/// </summary>
[RequireComponent(typeof(Collider))]
public class Day7FinElevatorZone : MonoBehaviour
{
    [Header("Scene binding")]
    [SerializeField] private string targetSceneName = "day7_fin";

    [Header("Door control")]
    [SerializeField] private ElevatorDoorController doorController;
    [SerializeField] private bool openDoorsOnStart = true;
    [SerializeField] private float doorOpenDelay = 0.25f;
    [SerializeField] private bool closeDoorsWhenPlayerEnters = true;

    public event Action PlayerReachedElevator;

    private bool playerInside;
    private bool initialized;

    public bool HasPlayerInside => playerInside;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != targetSceneName)
        {
            enabled = false;
            return;
        }

        if (doorController == null)
        {
            doorController = FindObjectOfType<ElevatorDoorController>();
        }

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (openDoorsOnStart && doorController != null)
        {
            StartCoroutine(OpenDoorsWithDelay());
        }
        else
        {
            initialized = true;
        }
    }

    private IEnumerator OpenDoorsWithDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(doorOpenDelay, 0f));

        doorController.StopAllCoroutines();
        doorController.OpenDoors();
        initialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized || playerInside || !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        if (closeDoorsWhenPlayerEnters && doorController != null)
        {
            doorController.StopAllCoroutines();
            doorController.CloseDoors();
        }

        PlayerReachedElevator?.Invoke();
    }
}

