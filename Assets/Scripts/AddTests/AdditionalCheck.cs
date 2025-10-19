using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditionalCheck : MonoBehaviour
{
    public static string SceneCheck()
    {
        // Получаем имя текущей активной сцены
        string currentScene = SceneManager.GetActiveScene().name;

        // Возвращаем имя сцены
        return currentScene;
    }

    public static bool IsDialogueDone()
    {
        // Проверяем, существует ли объект DialogueManager в сцене
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            // Возвращаем значение свойства dialogueDone
            return DialogueManager.DialogueDone;
        }
        else
        {
            Debug.LogWarning("DialogueManager not found in the scene.");
            return false; // Или true, в зависимости от логики вашего приложения
        }
    }



    public static bool CanNPCMove()
    {
        string scene = SceneManager.GetActiveScene().name;
        //Debug.Log("Current scene: '" + scene + "'");  // Посмотрим точное имя


        // Пример: в сцене "d1_e" движение разрешено только если someCondition == true
        ElevatorMoving elevator = FindObjectOfType<ElevatorMoving>();
        ElevatorDoorController doors = Object.FindObjectOfType<ElevatorDoorController>();

        if (scene == "day1_e" && IsDialogueDone() && elevator != null && Vector3.Distance(elevator.transform.position, elevator.stops[elevator.currentTarget].position) < 0.1f)
        {
            return true;
        }
        if (scene == "day2_m")
        {
            if (elevator.midStop >= 0 &&
                Vector3.Distance(elevator.transform.position, elevator.stops[elevator.midStop].position) < 0.1f && doors.isOpen)
            {
                return true;
            }

            if (elevator.currentTarget == 0 && doors.isOpen && IsDialogueDone())
            {
                return true;
            }


        }
        if (scene == "day3_m")
        {
            if (elevator.midStop >= 0 &&
                Vector3.Distance(elevator.transform.position, elevator.stops[elevator.midStop].position) < 0.1f && doors.isOpen)
            {
                return true;
            }

            if (elevator.currentTarget == 0 && doors.isOpen && IsDialogueDone())
            {
                return true;
            }
        }
        
        if (scene == "day3_e" && elevator != null && Vector3.Distance(elevator.transform.position, elevator.stops[elevator.currentTarget].position) < 0.1f && elevator.currentTarget == elevator.midStop)
        {
            return true;
        }

        return false;
    }

    public static bool ElevatorDoorsCanBeOpened()
    {
        string scene = SceneManager.GetActiveScene().name;
        Debug.Log("Current scene: '" + scene + "'");  // Посмотрим точное имя
        Debug.Log("IsDialogueDone(): " + IsDialogueDone());
        PlayerInLiftCheck playerLiftCheck = FindObjectOfType<PlayerInLiftCheck>();
        NPCInLiftCheck npcLiftCheck = FindObjectOfType<NPCInLiftCheck>();
        ElevatorMoving elevator = FindObjectOfType<ElevatorMoving>();
        // Пример: в сцене "d1_e" движение разрешено только если someCondition == true
        if (scene == "day1_e")
        {
            Debug.Log("00000000000000");
            Debug.Log("IsPlayerInsideElevator" + playerLiftCheck.IsPlayerInsideElevator());

            if (playerLiftCheck.IsPlayerInsideElevator() && IsDialogueDone())
            {
                Debug.Log("1");
                return true;
            }

            if (!playerLiftCheck.IsPlayerInsideElevator() && (IsDialogueDone() == false))
            {
                Debug.Log("2");
                return true;
            }

            Debug.Log("3");
            return false;
        }   
        if (scene == "day2_m")
        {
            Debug.Log("22222222222222");
            
            Debug.Log("IsPlayerInsideElevator" + playerLiftCheck.IsPlayerInsideElevator());

            if ( IsDialogueDone() == false && !playerLiftCheck.IsPlayerInsideElevator())
            {
                return true;
            }
            
            if ( IsDialogueDone() == false && playerLiftCheck.IsPlayerInsideElevator() && !npcLiftCheck.IsNPCInsideElevator())
            {
                return true;
            }
            
            if (playerLiftCheck.IsPlayerInsideElevator() && IsDialogueDone())
            {
                return true;
            }
            
            Debug.Log("3");
            return false;
            
            
        } 
        if (scene == "day3_m")
        {
            Debug.Log("22222222222222");
            
            Debug.Log("IsPlayerInsideElevator" + playerLiftCheck.IsPlayerInsideElevator());

            if ( IsDialogueDone() == false && !playerLiftCheck.IsPlayerInsideElevator())
            {
                return true;
            }
            
            if ( IsDialogueDone() == false && playerLiftCheck.IsPlayerInsideElevator() && !npcLiftCheck.IsNPCInsideElevator())
            {
                return true;
            }
            
            if (playerLiftCheck.IsPlayerInsideElevator() && IsDialogueDone())
            {
                return true;
            }
            
            Debug.Log("3");
            return false;
        } 
        
        return true;
    }

    public static bool CanLevelBeChanged()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "day4_m")
        {
            return IsDialogueDone();
        }
        return true;
    }


}