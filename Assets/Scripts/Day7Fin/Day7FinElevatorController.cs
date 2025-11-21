using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Гарантирует корректное состояние лифта в сцене day7_fin:
/// - фиксирует позицию на заданном этаже;
/// - держит двери открытыми до момента входа игрока;
/// - после входа игрока быстро закрывает двери.
/// </summary>
[DefaultExecutionOrder(-200)]
public class Day7FinElevatorController : MonoBehaviour
{
    [Header("Привязки")]
    [SerializeField] private string targetSceneName = "day7_fin";
    [SerializeField] private ElevatorMoving elevator;
    [SerializeField] private ElevatorDoorController doorController;
    [SerializeField] private PlayerInLiftCheck playerInLiftCheck;
    [SerializeField] private Rigidbody elevatorRigidbody;

    [Header("Позиция / этаж")]
    [SerializeField] private int targetFloorIndex = 3;
    [SerializeField] private Transform targetFloorOverride;

    [Header("Двери")]
    [SerializeField] private bool forceDoorInstantly = true;
    [SerializeField] private float initialDoorOpenDelay = 0.15f;
    [SerializeField] private bool closeDoorsAfterPlayerEnter = true;
    [SerializeField] private float doorCloseDelayAfterEnter = 0.2f;

    [Header("Отладка")]
    [SerializeField] private bool debugLogs = false;

    private bool playerInside;

    private void Awake()
    {
        if (!IsTargetScene())
        {
            enabled = false;
            return;
        }

        CacheReferences();
        ApplyFloorSetup();
    }

    private void Start()
    {
        if (!enabled)
        {
            return;
        }

        StartCoroutine(SetupRoutine());
    }

    private bool IsTargetScene()
    {
        return SceneManager.GetActiveScene().name == targetSceneName;
    }

    private void CacheReferences()
    {
        if (elevator == null)
        {
            elevator = GetComponent<ElevatorMoving>();
        }

        if (doorController == null && elevator != null)
        {
            doorController = elevator.GetComponentInChildren<ElevatorDoorController>(true);
        }

        if (playerInLiftCheck == null)
        {
            playerInLiftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
        }

        if (elevatorRigidbody == null && elevator != null)
        {
            elevatorRigidbody = elevator.GetComponent<Rigidbody>();
        }
    }

    private void ApplyFloorSetup()
    {
        if (elevator == null)
        {
            Log("ElevatorMoving не найден, прерываем настройку.");
            return;
        }

        Vector3 desiredPosition = elevator.transform.position;
        int resolvedIndex = targetFloorIndex;

        if (targetFloorOverride != null)
        {
            desiredPosition = targetFloorOverride.position;
        }
        else if (elevator.stops != null && elevator.stops.Length > 0)
        {
            resolvedIndex = Mathf.Clamp(targetFloorIndex, 0, elevator.stops.Length - 1);
            desiredPosition = elevator.stops[resolvedIndex].position;
        }
        else
        {
            Log("Массив точек остановки пуст, используем текущую позицию.");
        }

        elevator.transform.position = desiredPosition;
        if (elevatorRigidbody != null)
        {
            elevatorRigidbody.position = desiredPosition;
            elevatorRigidbody.velocity = Vector3.zero;
            elevatorRigidbody.angularVelocity = Vector3.zero;
        }

        elevator.startStop = resolvedIndex;
        elevator.endStop = resolvedIndex;
        elevator.currentTarget = resolvedIndex;
        elevator.rememberStartStop = resolvedIndex;
        elevator.rememberEndStop = resolvedIndex;
        elevator.canMove = false;

        Log($"Лифт зафиксирован на остановке #{resolvedIndex} (позиция {desiredPosition}).");
    }

    private IEnumerator SetupRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(initialDoorOpenDelay, 0f));
        KeepDoorsOpen();

        if (closeDoorsAfterPlayerEnter)
        {
            yield return MonitorPlayerEntry();
        }
    }

    private void KeepDoorsOpen()
    {
        if (doorController == null)
        {
            Log("DoorController не найден, не можем открыть двери.");
            return;
        }

        doorController.StopAllCoroutines();

        if (forceDoorInstantly)
        {
            doorController.ForceOpenInstantly();
            Log("Двери принудительно открыты (ForceOpenInstantly).");
        }
        else
        {
            doorController.OpenDoors();
            Log("Вызван стандартный OpenDoors().");
        }
    }

    private IEnumerator MonitorPlayerEntry()
    {
        while (playerInLiftCheck == null)
        {
            playerInLiftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
            if (playerInLiftCheck == null)
            {
                Log("Ожидание PlayerInLiftCheck...");
                yield return null;
            }
        }

        while (!playerInside)
        {
            if (playerInLiftCheck.IsPlayerInsideElevator())
            {
                playerInside = true;
                Log("Игрок внутри лифта, закрываем двери.");
                yield return new WaitForSeconds(Mathf.Max(doorCloseDelayAfterEnter, 0f));
                CloseDoors();
                yield break;
            }

            yield return null;
        }
    }

    private void CloseDoors()
    {
        if (doorController == null)
        {
            return;
        }

        doorController.StopAllCoroutines();

        if (forceDoorInstantly)
        {
            doorController.ForceCloseInstantly();
            Log("Двери принудительно закрыты (ForceCloseInstantly).");
        }
        else
        {
            doorController.CloseDoors();
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
        {
            Debug.Log($"[Day7FinElevatorController] {message}", this);
        }
    }
}