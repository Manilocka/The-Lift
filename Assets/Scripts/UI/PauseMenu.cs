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
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
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
        if (!isPaused) return;
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Не принудительно прячем курсор — зависит от твоего контроллера
    }

    public void OpenSettings()
    {
        // Сохраняем текущую сцену для возврата из настроек
        PlayerPrefs.SetString("ReturnScene", SceneManager.GetActiveScene().name);
        // Флаг: при возврате из настроек снова открыть меню паузы
        PlayerPrefs.SetInt("ReturnOpenPause", 1);
        PlayerPrefs.Save();
        // Выходим из паузы, чтобы сцена настроек работала нормально
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(settingsSceneName);
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


