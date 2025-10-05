using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class RetroButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Button Components")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image buttonImage;
    
    [Header("Selection Background")]
    public Image selectionBackground; // Прямоугольник 700x70 цвета #43FCBD
    
    [Header("Hover Effects")]
    public Color hoverTextColor = Color.black; // Черный цвет при наведении
    public bool enableBlinking = true;
    public float blinkSpeed = 2f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    
    private bool isHovered = false;
    private bool isPressed = false;
    private Color originalTextColor;
    private Vector3 originalScale;
    private Coroutine blinkCoroutine;
    
    void Start()
    {
        InitializeButton();
    }
    
    void InitializeButton()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
            
        // Сохраняем оригинальный цвет текста
        if (buttonText != null)
        {
            originalTextColor = buttonText.color;
        }
            
        originalScale = transform.localScale;
        
        // Настройка фона выделения
        SetupSelectionBackground();
    }
    
    void SetupSelectionBackground()
    {
        if (selectionBackground != null)
        {
            // Настройка прямоугольника выделения
            selectionBackground.color = new Color(0.26f, 0.99f, 0.74f, 1f); // #43FCBD
            selectionBackground.rectTransform.sizeDelta = new Vector2(700, 70);
            selectionBackground.gameObject.SetActive(false);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        PlaySound(hoverSound);
        
        // Показываем фон выделения
        if (selectionBackground != null)
        {
            selectionBackground.gameObject.SetActive(true);
            selectionBackground.rectTransform.position = transform.position;
        }
        
        // Начинаем моргание
        if (enableBlinking && buttonText != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        
        // Скрываем фон выделения
        if (selectionBackground != null)
        {
            selectionBackground.gameObject.SetActive(false);
        }
        
        // Останавливаем моргание и возвращаем оригинальный цвет
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        if (buttonText != null)
        {
            buttonText.color = originalTextColor;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        isPressed = true;
        PlaySound(clickSound);
        StartCoroutine(ClickEffect());
    }
    
    IEnumerator ClickEffect()
    {
        // Эффект нажатия
        transform.localScale = originalScale * 0.95f;
        
        yield return new WaitForSeconds(0.1f);
        
        transform.localScale = originalScale;
        isPressed = false;
    }
    
    IEnumerator BlinkEffect()
    {
        while (isHovered && buttonText != null)
        {
            // Моргание между черным и оригинальным цветом
            buttonText.color = hoverTextColor;
            yield return new WaitForSeconds(0.1f);
            
            buttonText.color = originalTextColor;
            yield return new WaitForSeconds(0.1f);
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
