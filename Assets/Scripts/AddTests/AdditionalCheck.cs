using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        DialogueManager dialogueManager = GameObject.FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            // Возвращаем значение свойства dialogueDone
            return dialogueManager.DialogueDone;
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
        Debug.Log("Current scene: '" + scene + "'");  // Посмотрим точное имя
        Debug.Log("IsDialogueDone(): " + IsDialogueDone());

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
}
