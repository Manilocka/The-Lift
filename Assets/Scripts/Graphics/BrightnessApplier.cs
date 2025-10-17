using UnityEngine;
using UnityEngine.UI;

// Накладывает прозрачный чёрный слой поверх UI для имитации яркости,
// полезно когда фон камеры скрыт пост-эффектами/видео/канвасом.
public class BrightnessApplier : MonoBehaviour
{
    [Header("Overlay")]
    public Image overlay; // Полноэкранный Image на верхнем Canvas

    void Awake()
    {
        if (overlay == null)
            overlay = GetComponent<Image>();
        // Настройка UI слоя: растянуть на весь экран, игнорировать клики
        var rt = overlay != null ? overlay.rectTransform : GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        if (overlay != null)
        {
            overlay.raycastTarget = false;
        }
        Apply(SettingsManager.Instance != null ? SettingsManager.Instance.brightness : PlayerPrefs.GetFloat("Brightness", 50f));
    }

    public void Apply(float brightness)
    {
        if (overlay == null) return;
        // brightness [0..100]; 0 — чёрный, 100 — прозрачный
        float t = Mathf.Clamp01(brightness / 100f);
        // Нелинейная кривая для более заметного эффекта при низкой яркости
        float darkness = Mathf.Pow(1f - t, 0.7f); // 0 => 1, 100 => 0
        overlay.color = new Color(0f, 0f, 0f, Mathf.Clamp01(darkness * 0.85f));
    }

    void Update()
    {
        if (SettingsManager.Instance != null)
        {
            Apply(SettingsManager.Instance.brightness);
        }
    }
}


