using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAutoExit : MonoBehaviour
{
    [Header("Сцена, в которую перейдём после танца")]
    [SerializeField] private string nextSceneName = "Game menu";

    [Header("Задержка перед переходом (в секундах)")]
    [SerializeField] private float delay = 15f;

    private void Start()
    {
        // Запускаем переход через delay секунд
        Invoke(nameof(LoadNextScene), delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // Опционально: если игрок нажмёт любую клавишу — уходим сразу
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            CancelInvoke(nameof(LoadNextScene));
            LoadNextScene();
        }
    }
}
