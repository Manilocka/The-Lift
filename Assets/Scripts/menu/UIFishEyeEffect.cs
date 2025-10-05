using UnityEngine;
using UnityEngine.UI;

public class UIFishEyeEffect : MonoBehaviour
{
    [Header("Fish Eye Settings")]
    [Range(0f, 2f)]
    public float fishEyeStrength = 0.3f;
    
    [Header("UI Components")]
    public RectTransform targetTransform;
    public Image targetImage;
    
    [Header("Animation")]
    public bool animateEffect = true;
    public float animationSpeed = 1f;
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float timeOffset;
    
    void Start()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();
            
        if (targetImage == null)
            targetImage = GetComponent<Image>();
        
        // Сохраняем оригинальные значения
        if (targetTransform != null)
        {
            originalScale = targetTransform.localScale;
            originalPosition = targetTransform.localPosition;
        }
        
        // Добавляем случайное смещение для анимации
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Update()
    {
        ApplyFishEyeEffect();
    }
    
    void ApplyFishEyeEffect()
    {
        if (targetTransform == null) return;
        
        // Вычисляем позицию относительно центра экрана
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 currentPos = targetTransform.position;
        Vector2 offset = currentPos - screenCenter;
        
        // Вычисляем расстояние от центра
        float distance = offset.magnitude;
        float maxDistance = Mathf.Sqrt(screenCenter.x * screenCenter.x + screenCenter.y * screenCenter.y);
        float normalizedDistance = distance / maxDistance;
        
        // Применяем fish eye эффект
        float distortion = 1f + (fishEyeStrength * normalizedDistance * normalizedDistance);
        
        // Искажаем масштаб
        Vector3 newScale = originalScale * distortion;
        
        // Добавляем анимацию если включена
        if (animateEffect)
        {
            float animation = Mathf.Sin(Time.time * animationSpeed + timeOffset) * 0.1f;
            newScale += Vector3.one * animation;
        }
        
        targetTransform.localScale = newScale;
        
        // Небольшое смещение позиции для большего эффекта
        Vector3 positionOffset = new Vector3(
            offset.x * fishEyeStrength * 0.01f,
            offset.y * fishEyeStrength * 0.01f,
            0f
        );
        
        targetTransform.localPosition = originalPosition + positionOffset;
    }
    
    public void SetFishEyeStrength(float strength)
    {
        fishEyeStrength = Mathf.Clamp(strength, 0f, 2f);
    }
    
    public void ResetEffect()
    {
        if (targetTransform != null)
        {
            targetTransform.localScale = originalScale;
            targetTransform.localPosition = originalPosition;
        }
    }
}
