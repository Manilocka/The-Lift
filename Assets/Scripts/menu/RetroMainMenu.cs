using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class RetroMainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Button continueButton;
    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("Text Elements")]
    public TextMeshProUGUI continueText;
    public TextMeshProUGUI newGameText;
    public TextMeshProUGUI settingsText;
    public TextMeshProUGUI exitText;
    public TextMeshProUGUI versionText;
    
    // Убрали прямоугольник - только моргающий текст
    
    [Header("Retro Effects")]
    public Image scanlinesOverlay;
    public Image glitchOverlay;
    public VideoPlayer backgroundVideo;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip selectSound;
    public AudioClip confirmSound;
    
    [Header("Retro Settings")]
    public float glitchIntensity = 0.1f;
    public float scanlineSpeed = 1f;
    public Color retroGreen = new Color(0.2f, 1f, 0.2f, 1f);
    public Color retroHighlight = new Color(0.4f, 1f, 0.4f, 1f);
    
    [Header("Scene Management")]
    public string gameSceneName = "day1_e"; // Имя сцены игры
    public string settingsSceneName = "Settings"; // Имя сцены настроек
    
    private int currentSelection = 0;
    private Button[] menuButtons;
    private TextMeshProUGUI[] menuTexts;
    private bool isNavigating = false;
    private bool hasSaveGame = false; // Проверка наличия сохранения
    
    void Start()
    {
        CheckForSaveGame();
        InitializeMenu();
        SetupRetroEffects();
        StartCoroutine(GlitchEffect());
    }
    
    void CheckForSaveGame()
    {
        // Проверяем наличие сохранения игры
        hasSaveGame = PlayerPrefs.HasKey("GameProgress") && PlayerPrefs.GetInt("GameProgress", 0) > 0;
        
        // Если нет сохранения, делаем кнопку "Продолжить" неактивной
        if (continueButton != null)
        {
            continueButton.interactable = hasSaveGame;
        }
    }
    
    void InitializeMenu()
    {
        // Инициализация массивов кнопок и текстов
        menuButtons = new Button[] { continueButton, newGameButton, settingsButton, exitButton };
        menuTexts = new TextMeshProUGUI[] { continueText, newGameText, settingsText, exitText };
        
        // Настройка кнопок
        for (int i = 0; i < menuButtons.Length; i++)
        {
            int index = i; // Захват для замыкания
            menuButtons[i].onClick.AddListener(() => SelectOption(index));
        }
        
        // Убрали прямоугольник - только моргающий текст
        
        // Установка начального выделения
        UpdateSelection(0);
        
        // Настройка версии
        if (versionText != null)
        {
            versionText.text = "v 1.0";
            versionText.color = retroGreen;
        }
    }
    
    // Убрали прямоугольник - только моргающий текст
    
    void SetupRetroEffects()
    {
        // Настройка видео фона
        if (backgroundVideo != null)
        {
            backgroundVideo.isLooping = true;
            backgroundVideo.playOnAwake = true;
            backgroundVideo.Play(); // Принудительно запускаем видео
        }
        
        // Настройка эффекта сканирования
        if (scanlinesOverlay != null)
        {
            scanlinesOverlay.color = new Color(0, 1, 0, 0.1f);
        }
    }
    
    void Update()
    {
        HandleInput();
        UpdateRetroEffects();
    }
    
    void HandleInput()
    {
        if (isNavigating) return;
        
        // Навигация стрелками
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            NavigateUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            NavigateDown();
        }
        
        // Выбор опции
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption(currentSelection);
        }
    }
    
    void NavigateUp()
    {
        PlaySound(selectSound);
        currentSelection = (currentSelection - 1 + menuButtons.Length) % menuButtons.Length;
        UpdateSelection(currentSelection);
    }
    
    void NavigateDown()
    {
        PlaySound(selectSound);
        currentSelection = (currentSelection + 1) % menuButtons.Length;
        UpdateSelection(currentSelection);
    }
    
    void UpdateSelection(int index)
    {
        // Простое выделение - только моргающий текст
        currentSelection = index;
    }
    
    public void SelectOption(int optionIndex)
    {
        if (isNavigating) return;
        
        StartCoroutine(SelectOptionCoroutine(optionIndex));
    }
    
    IEnumerator SelectOptionCoroutine(int optionIndex)
    {
        isNavigating = true;
        PlaySound(confirmSound);
        
        // Эффект мерцания при выборе (как в консоли) - НЕ МЕНЯЕМ цвет текста
        for (int i = 0; i < 5; i++)
        {
            // НЕ МЕНЯЕМ цвет текста - оставляем как настроено в Unity
            yield return new WaitForSeconds(0.05f);
            yield return new WaitForSeconds(0.05f);
        }
        
        // Выполнение действия
        switch (optionIndex)
        {
            case 0: // Продолжить
                if (hasSaveGame)
                    ContinueGame();
                break;
            case 1: // Новая игра
                StartNewGame();
                break;
            case 2: // Настройки
                OpenSettings();
                break;
            case 3: // Выйти
                ExitGame();
                break;
        }
        
        isNavigating = false;
    }
    
    void ContinueGame()
    {
        // Загрузка последнего сохранения
        Debug.Log("Продолжить игру");
        SceneManager.LoadScene(gameSceneName);
    }
    
    void StartNewGame()
    {
        Debug.Log("Новая игра");
        // Очищаем сохранение при новой игре
        PlayerPrefs.DeleteKey("GameProgress");
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }
    
    void OpenSettings()
    {
        Debug.Log("Открыть настройки");
        // Загружаем сцену настроек (если есть)
        if (!string.IsNullOrEmpty(settingsSceneName))
        {
            SceneManager.LoadScene(settingsSceneName);
        }
    }
    
    void ExitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
        
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
    
    void UpdateRetroEffects()
    {
        // Анимация сканирования
        if (scanlinesOverlay != null)
        {
            float offset = Time.time * scanlineSpeed;
            scanlinesOverlay.material.SetFloat("_Offset", offset);
        }
    }
    
    IEnumerator GlitchEffect()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            
            if (glitchOverlay != null)
            {
                StartCoroutine(GlitchFlash());
            }
        }
    }
    
    IEnumerator GlitchFlash()
    {
        if (glitchOverlay != null)
        {
            glitchOverlay.color = new Color(1, 0, 0, glitchIntensity);
            yield return new WaitForSeconds(0.1f);
            glitchOverlay.color = new Color(0, 1, 0, glitchIntensity);
            yield return new WaitForSeconds(0.1f);
            glitchOverlay.color = Color.clear;
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
