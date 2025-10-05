using UnityEngine;
using UnityEngine.UI;

public class UICameraEffects : MonoBehaviour
{
    [Header("Camera Effects")]
    public Camera uiCamera;
    public Canvas canvas;
    
    [Header("Fish Eye Settings")]
    [Range(0f, 2f)]
    public float fishEyeStrength = 0.3f;
    
    [Header("Retro Effects")]
    [Range(0f, 1f)]
    public float chromaticAberration = 0.1f;
    [Range(0f, 1f)]
    public float vignette = 0.3f;
    [Range(0f, 1f)]
    public float noise = 0.05f;
    
    [Header("Scanlines")]
    public bool enableScanlines = true;
    [Range(0f, 1f)]
    public float scanlineIntensity = 0.3f;
    [Range(1f, 10f)]
    public float scanlineCount = 3f;
    
    [Header("Color Grading")]
    public Color tintColor = new Color(0.2f, 1f, 0.2f, 1f);
    [Range(0f, 1f)]
    public float tintStrength = 0.2f;
    
    private Material retroMaterial;
    
    void Start()
    {
        SetupUICamera();
        SetupRetroMaterial();
    }
    
    void SetupUICamera()
    {
        // Настраиваем Canvas для работы с камерой
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = 1f; // Расстояние от камеры
        }
        
        // Настраиваем камеру для UI
        if (uiCamera != null)
        {
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5f; // Размер UI
            uiCamera.nearClipPlane = 0.1f;
            uiCamera.farClipPlane = 10f;
        }
    }
    
    void SetupRetroMaterial()
    {
        // Создаем материал для ретро эффектов
        Shader retroShader = Shader.Find("Hidden/RetroEffect");
        if (retroShader != null)
        {
            retroMaterial = new Material(retroShader);
        }
        else
        {
            // Если шейдер не найден, создаем простой материал
            retroMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (retroMaterial != null)
        {
            // Установка параметров для ретро эффектов
            retroMaterial.SetFloat("_FishEyeStrength", fishEyeStrength);
            retroMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
            retroMaterial.SetFloat("_Vignette", vignette);
            retroMaterial.SetFloat("_Noise", noise);
            retroMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
            retroMaterial.SetFloat("_ScanlineCount", scanlineCount);
            retroMaterial.SetColor("_TintColor", tintColor);
            retroMaterial.SetFloat("_TintStrength", tintStrength);
            retroMaterial.SetInt("_EnableScanlines", enableScanlines ? 1 : 0);
            
            Graphics.Blit(source, destination, retroMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
    
    void Update()
    {
        // Динамическое изменение эффектов
        if (noise > 0 && retroMaterial != null)
        {
            retroMaterial.SetFloat("_NoiseOffset", Time.time * 10f);
        }
        
        if (enableScanlines && retroMaterial != null)
        {
            retroMaterial.SetFloat("_ScanlineOffset", Time.time * 2f);
        }
    }
    
    void OnDestroy()
    {
        if (retroMaterial != null)
        {
            DestroyImmediate(retroMaterial);
        }
    }
}
