using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")] 
    public GameObject pauseCanvas; // один Canvas паузы
    public Button resumeButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Scenes")] 
    public string settingsSceneName = "Settings";
    public string mainMenuSceneName = "Game menu";

    private bool isPaused = false;

    void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null)
        {
            // ВРЕМЕННО: скрываем кнопку Настройки
            settingsButton.gameObject.SetActive(false);
        }
        if (exitButton != null) exitButton.onClick.AddListener(ExitToMainMenu);

        // Если вернулись из настроек по паузному сценарию — сразу открыть паузу
        if (PlayerPrefs.GetInt("ReturnOpenPause", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ReturnOpenPause");
            PlayerPrefs.Save();
            PauseGame();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame(); else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Всегда просто снимаем паузу, не перегружаем сцену и не меняем ReturnScene
        if (!isPaused)
        {
            // На случай если кто-то вызвал Continue после возврата из настроек
            if (pauseCanvas != null) pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            // Прячем курсор обратно
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Прячем курсор обратно
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        // ВРЕМЕННО ОТКЛЮЧЕНО
        Debug.Log("Settings disabled temporarily");
    }

    public void ExitToMainMenu()
    {
        // Фиксируем текущую сцену как последнюю (на случай Продолжить)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCurrentScene();
        }
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // PauseMenu теперь управляет одним Canvas; никаких массивов
}


