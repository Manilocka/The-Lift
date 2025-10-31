using UnityEngine;

public class FloorDoorController : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float openPosition = 50f;
    [SerializeField] private float closePosition = 0f;
    [SerializeField] private float speed = 2f;
    [SerializeField] public int floorNumber = 0;
    [SerializeField] private AudioClip doorSoundClip;

    public bool isOpen = false;
    private Vector3 leftDoorTarget;
    private Vector3 rightDoorTarget;

    void Start()
    {
        leftDoorTarget = leftDoor.localPosition;
        rightDoorTarget = rightDoor.localPosition;
    }

    void Update()
    {
        leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorTarget, speed * Time.deltaTime);
        rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorTarget, speed * Time.deltaTime);
    }

    public void OpenDoors()
    {
        Debug.Log("FloorDoorController.OpenDoors() called, isOpen: " + isOpen);
        PlayDoorSound();
        
        leftDoorTarget = leftDoor.localPosition + Vector3.left * openPosition;
        rightDoorTarget = rightDoor.localPosition + Vector3.right * openPosition;
        isOpen = true;
        
        Debug.Log("Floor doors opening on floor " + floorNumber);
    }

    public void CloseDoors()
    {
        Debug.Log("FloorDoorController.CloseDoors() called, isOpen: " + isOpen);
        PlayDoorSound();
        
        leftDoorTarget = new Vector3(closePosition, leftDoor.localPosition.y, leftDoor.localPosition.z);
        rightDoorTarget = new Vector3(closePosition, rightDoor.localPosition.y, rightDoor.localPosition.z);
        isOpen = false;
        
        Debug.Log("Floor doors closing on floor " + floorNumber);
    }

    private void PlayDoorSound()
    {
        if (doorSoundClip != null)
        {
            // Вариант 1: Используем позицию главной камеры
            if (Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(doorSoundClip, Camera.main.transform.position, 1.0f);
                Debug.Log("Playing door sound at camera position on floor " + floorNumber);
            }
            // Вариант 2: Используем позицию самих дверей
            else
            {
                Vector3 soundPosition = (leftDoor.position + rightDoor.position) / 2f;
                AudioSource.PlayClipAtPoint(doorSoundClip, soundPosition, 1.0f);
                Debug.Log("Playing door sound at door position on floor " + floorNumber);
            }
        }
        else
        {
            Debug.LogError("Door sound clip is NULL on floor " + floorNumber);
        }
    }
}