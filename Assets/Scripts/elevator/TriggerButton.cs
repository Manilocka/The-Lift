using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    [SerializeField] private ElevatorDoorController doorController; // Ссылка на контроллер дверей

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Проверка клика левой кнопкой мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    if (doorController != null)
                    {
                        if (doorController.isOpen) // Если двери открыты, закрываем
                        {
                            doorController.CloseDoors();
                        }
                        else // Если закрыты, открываем
                        {
                            doorController.OpenDoors();
                        }
                    }
                }
            }
        }
    }
}