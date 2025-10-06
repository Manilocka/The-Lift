using UnityEngine;

public class PlayerInLiftCheck : MonoBehaviour
{
    private bool isPlayerInsideElevator = false;

    // Проверка входа в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // предполагаем, что у игрока стоит тег "Player"
        {
            isPlayerInsideElevator = true;
            Debug.Log("Player entered the lift!");
        }
    }

    // Проверка выхода из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideElevator = false;
            Debug.Log("Player exited the lift!");
        }
    }

    // Публичный метод, чтобы другие скрипты могли узнать, внутри ли игрок
    public bool IsPlayerInsideElevator()
    {
        return isPlayerInsideElevator;
    }
}
