using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
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
            button.onClick.AddListener(ExitGame);
            Debug.Log("ExitButton: Кнопка 'Выйти' настроена");
        }
        else
        {
            Debug.LogError("ExitButton: Button компонент не найден");
        }
    }
    
    public void ExitGame()
    {
        Debug.Log("ExitButton: Выход из игры");
        
        // Воспроизводим звук
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        
        // Выходим из игры
        Application.Quit();
        
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
}

