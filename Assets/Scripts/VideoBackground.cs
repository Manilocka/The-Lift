using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoBackground : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;
    public Material videoMaterial;
    
    [Header("Retro Effects")]
    public bool enableScanlines = true;
    public float scanlineIntensity = 0.3f;
    public float scanlineSpeed = 1f;
    public Color scanlineColor = new Color(0, 1, 0, 0.1f);
    
    [Header("Color Grading")]
    public Color tintColor = new Color(0.2f, 1f, 0.2f, 1f);
    public float tintStrength = 0.2f;
    public float contrast = 1.2f;
    public float brightness = 0.1f;
    
    void Start()
    {
        SetupVideoPlayer();
        SetupRetroMaterial();
    }
    
    void SetupVideoPlayer()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
            
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = true;
            videoPlayer.playOnAwake = true;
            
            // Используем Camera Far Plane для отображения на фоне
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.targetCameraAlpha = 1f;
            
            // Принудительно запускаем видео
            StartCoroutine(StartVideoDelayed());
        }
    }
    
    IEnumerator StartVideoDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
    
    void SetupRetroMaterial()
    {
        if (videoMaterial == null)
        {
            // Создание материала для ретро эффектов
            Shader retroShader = Shader.Find("UI/Default");
            if (retroShader != null)
            {
                videoMaterial = new Material(retroShader);
            }
        }
        
        if (videoMaterial != null && renderTexture != null)
        {
            videoMaterial.mainTexture = renderTexture;
            UpdateRetroProperties();
        }
    }
    
    void Update()
    {
        UpdateRetroProperties();
        UpdateScanlines();
    }
    
    void UpdateRetroProperties()
    {
        if (videoMaterial != null)
        {
            videoMaterial.SetColor("_TintColor", tintColor);
            videoMaterial.SetFloat("_TintStrength", tintStrength);
            videoMaterial.SetFloat("_Contrast", contrast);
            videoMaterial.SetFloat("_Brightness", brightness);
            videoMaterial.SetInt("_EnableScanlines", enableScanlines ? 1 : 0);
        }
    }
    
    void UpdateScanlines()
    {
        if (enableScanlines && videoMaterial != null)
        {
            float offset = Time.time * scanlineSpeed;
            videoMaterial.SetFloat("_ScanlineOffset", offset);
            videoMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
            videoMaterial.SetColor("_ScanlineColor", scanlineColor);
        }
    }
    
    public void SetVideoClip(VideoClip clip)
    {
        if (videoPlayer != null)
        {
            videoPlayer.clip = clip;
        }
    }
    
    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
    
    void OnDestroy()
    {
        if (videoMaterial != null)
        {
            DestroyImmediate(videoMaterial);
        }
    }
}
