using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RetroButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Button Components")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image buttonImage;
    
    // Убрали прямоугольник - только моргающий текст
    
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
        
        // Убрали прямоугольник - только моргающий текст
    }
    
    // Убрали прямоугольник - только моргающий текст
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        PlaySound(hoverSound);
        
        // Начинаем моргание текста
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
