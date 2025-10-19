using UnityEngine;

public class OriginalVolume : MonoBehaviour
{
    [Header("Original Volume Settings")]
    public float originalVolume = 1f;
    
    void Awake()
    {
        // Сохраняем оригинальную громкость при создании
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && originalVolume == 1f)
        {
            originalVolume = audioSource.volume;
        }
    }
    
    void Start()
    {
        // Применяем глобальную громкость при старте
        ApplyGlobalVolume();
    }
    
    public void ApplyGlobalVolume()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // Получаем глобальную громкость из PlayerPrefs
            float globalVolume = PlayerPrefs.HasKey("Volume") ? PlayerPrefs.GetFloat("Volume") : 100f;
            
            // Применяем глобальную громкость
            float volumeMultiplier = globalVolume / 100f;
            audioSource.volume = originalVolume * volumeMultiplier;
        }
    }
}

