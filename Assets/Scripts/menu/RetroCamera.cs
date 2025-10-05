using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RetroCamera : MonoBehaviour
{
    [Header("Fish Eye Effect")]
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
    
    private Camera cam;
    private Material retroMaterial;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        SetupRetroMaterial();
    }
    
    void SetupRetroMaterial()
    {
        // Создание материала для ретро эффектов
        Shader retroShader = Shader.Find("Hidden/RetroEffect");
        if (retroShader == null)
        {
            // Если шейдер не найден, создаем простой материал
            retroMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        else
        {
            retroMaterial = new Material(retroShader);
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
        // Динамическое изменение эффектов для более живого вида
        if (noise > 0)
        {
            retroMaterial?.SetFloat("_NoiseOffset", Time.time * 10f);
        }
        
        if (enableScanlines)
        {
            retroMaterial?.SetFloat("_ScanlineOffset", Time.time * 2f);
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
