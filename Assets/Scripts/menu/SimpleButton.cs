using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleButton : MonoBehaviour
{
    [Header("Button Settings")]
    public string targetSceneName = "";
    public bool isBackButton = false;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    
    private Button button;
    
    void Start()
    {
        SetupButton();
    }
    
    void SetupButton()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
            Debug.Log("SimpleButton: Кнопка настроена - " + gameObject.name);
        }
        else
        {
            Debug.LogError("SimpleButton: Button компонент не найден на " + gameObject.name);
        }
    }
    
    public void OnButtonClick()
    {
        Debug.Log("SimpleButton: Кнопка нажата - " + gameObject.name);
        
        // Воспроизводим звук
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        // Выполняем действие
        if (isBackButton)
        {
            ReturnToMainMenu();
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            LoadScene();
        }
        else
        {
            Debug.LogWarning("SimpleButton: Не указано действие для кнопки " + gameObject.name);
        }
    }
    
    void ReturnToMainMenu()
    {
        Debug.Log("SimpleButton: Возврат в главное меню");
        SceneManager.LoadScene("Game menu");
    }
    
    void LoadScene()
    {
        Debug.Log("SimpleButton: Загрузка сцены " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
    
    // Публичные методы для вызова из других скриптов
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }
    
    public void SetAsBackButton(bool isBack)
    {
        isBackButton = isBack;
    }
}
