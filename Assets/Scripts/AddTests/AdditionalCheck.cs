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
        if (scene == "day1_e" && IsDialogueDone() == true)
        {
            return true;
        }

        // В других сценах — разрешаем движение сразу
        if (scene != "day1_e")
        {
            return false;
        }

        return false;
    }

    public static bool ElevatorDoorsCanBeOpened()
    {
        string scene = SceneManager.GetActiveScene().name;
        Debug.Log("Current scene: '" + scene + "'");  // Посмотрим точное имя
        Debug.Log("IsDialogueDone(): " + IsDialogueDone());
        PlayerInLiftCheck playerLiftCheck = FindObjectOfType<PlayerInLiftCheck>();
        // Пример: в сцене "d1_e" движение разрешено только если someCondition == true
        if (scene == "day1_e")
        {
            Debug.Log("00000000000000");
            Debug.Log("IsPlayerInsideElevator" + playerLiftCheck.IsPlayerInsideElevator());

            if (playerLiftCheck.IsPlayerInsideElevator() && (IsDialogueDone() == true))
            {
                Debug.Log("1");
                return true;
            }
            else
            {
                if (!playerLiftCheck.IsPlayerInsideElevator() && (IsDialogueDone() == false))
                {
                    Debug.Log("2");
                    return true;
                }
            }
            Debug.Log("3");
            return false;
        }   
        

        
        return true;
    }


}
