using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SafeVideoBinder : MonoBehaviour
{
    [Header("Components")]
    public VideoPlayer videoPlayer;
    public RenderTexture targetTexture; // для RenderTexture режима

    [Header("Clip")]
    public VideoClip clip; // назначаемый ассет клипа (menu_screen)

    [Header("Behavior")]
    public bool loop = true;
    public bool playOnAwake = true;
    public bool useRenderTexture = true; // если false — перевяжет на Camera.main (Far Plane)

    private bool isPreparing;

    void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindAndPrepare();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeEvents();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // На всякий случай перевязываем после смены сцены
        BindAndPrepare();
    }

    void BindAndPrepare()
    {
        if (videoPlayer == null) return;

        UnsubscribeEvents();
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnPrepared;

        // Жестко запрещаем URL — источник 0xc00d36b4 при путях/смене сцен
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.url = string.Empty;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = loop;
        videoPlayer.playOnAwake = false; // мы сами запустим после Prepare
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;

        if (useRenderTexture)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = targetTexture;
            videoPlayer.targetCamera = null;
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.targetTexture = null;
        }

        // Подготовка может иногда зависать, поэтому перезапустим, если уже готовится
        if (isPreparing)
        {
            videoPlayer.Stop();
        }
        isPreparing = true;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer source)
    {
        isPreparing = false;
        if (playOnAwake)
        {
            source.Play();
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        // Молча пробуем восстановиться, чтобы не засорять лог
        StartCoroutine(RestartAfterFrame());
    }

    System.Collections.IEnumerator RestartAfterFrame()
    {
        yield return null;
        if (videoPlayer == null) yield break;

        // Полный сброс компонента — часто снимает ошибку WindowsVideoMedia
        var go = videoPlayer.gameObject;
        var renderTex = targetTexture;
        var assignedClip = clip;
        var useRT = useRenderTexture;
        var shouldLoop = loop;

        Destroy(videoPlayer);
        videoPlayer = go.AddComponent<VideoPlayer>();
        targetTexture = renderTex;
        clip = assignedClip;
        loop = shouldLoop;
        useRenderTexture = useRT;

        BindAndPrepare();
    }

    void UnsubscribeEvents()
    {
        if (videoPlayer == null) return;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.prepareCompleted -= OnPrepared;
    }
}


