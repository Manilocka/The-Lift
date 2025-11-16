using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInLiftCheck : MonoBehaviour
{
    private bool isNPCInsideElevator = false;

    // Проверка входа в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC")) // предполагаем, что у игрока стоит тег "Player"
        {
            isNPCInsideElevator = true;
            Debug.Log("NPC entered the lift!");
        }
    }

    // Проверка выхода из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            isNPCInsideElevator = false;
            Debug.Log("NPC exited the lift!");
            
            // // Деактивируем NPC при выходе из лифта
            // if (other.gameObject != null)
            // {
            //     other.gameObject.SetActive(false);
            //     Debug.Log("NPC исчез после выхода из лифта: " + other.gameObject.name);
            // }
        }
    }

    // Публичный метод, чтобы другие скрипты могли узнать, внутри ли игрок
    public bool IsNPCInsideElevator()
    {
        return isNPCInsideElevator;
    }
}