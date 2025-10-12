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

        // В сцене "day1_e" движение разрешено только если диалог завершен И двери лифта открыты
        if (scene == "day1_e")
        {
            // Проверяем, завершен ли диалог
            if (!IsDialogueDone())
            {
                return false;
            }
            
            // Проверяем, открыты ли двери лифта
            ElevatorDoorController doorController = GameObject.FindObjectOfType<ElevatorDoorController>();
            if (doorController != null && doorController.isOpen)
            {
                return true;
            }
            
            return false;
        }

        // В других сценах — разрешаем движение сразу
        return false;
    }

    public static bool ElevatorDoorsCanBeOpened()
    {
        string scene = SceneManager.GetActiveScene().name;
        Debug.Log("Current scene: '" + scene + "'");
        Debug.Log("IsDialogueDone(): " +IsDialogueDone());
        

        if (scene == "day1_e")
        {
            Debug.Log("Разрешаем открытие дверей в day1_e");
            return true;
        }   
        if (scene == "day1_m")
        {
            Debug.Log("Разрешаем открытие дверей в day1_m");
            return true;
        } 
        
        // В других сценах разрешаем открытие дверей
        return true;
    }
    



}
