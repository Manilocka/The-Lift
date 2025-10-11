using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleTextBlink : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text Component")]
    public TextMeshProUGUI buttonText;
    
    [Header("Blink Settings")]
    public Color blinkColor = Color.black;
    public float blinkSpeed = 0.1f;
    
    private Color originalColor;
    private bool isHovered = false;
    private Coroutine blinkCoroutine;
    
    void Start()
    {
        // Автоматически находим текст если не назначен
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        if (buttonText != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkText());
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
    
    IEnumerator BlinkText()
    {
        while (isHovered && buttonText != null)
        {
            // Черный цвет
            buttonText.color = blinkColor;
            yield return new WaitForSeconds(blinkSpeed);
            
            // Оригинальный цвет
            buttonText.color = originalColor;
            yield return new WaitForSeconds(blinkSpeed);
        }
        
        // Восстанавливаем оригинальный цвет
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
}
