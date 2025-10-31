using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("Intro (scene start)")]
    public Animator introAnimator;            // Панель/аниматор, который играет в начале сцены (Fade In)
    public string introTrigger = "FadeIn";    // Триггер для анимации начала
    public float introDuration = 1.0f;        // Длительность анимации начала

    [Header("Outro (before switching)")]
    public Animator outroAnimator;            // Панель/аниматор, который играет перед сменой сцены (Fade Out)
    public string outroTrigger = "FadeOut";   // Триггер для анимации окончания
    public float outroDuration = 1.0f;        // Длительность анимации окончания

    static SceneFader _instance;
    public static SceneFader Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayIntro()
    {
        if (introAnimator != null && !string.IsNullOrEmpty(introTrigger))
        {
            introAnimator.ResetTrigger(outroTrigger);
            introAnimator.SetTrigger(introTrigger);
        }
    }

    public void PlayOutro()
    {
        if (outroAnimator != null && !string.IsNullOrEmpty(outroTrigger))
        {
            outroAnimator.ResetTrigger(introTrigger);
            outroAnimator.SetTrigger(outroTrigger);
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Запускаем затемнение (Outro)
        PlayOutro();
        if (outroDuration > 0f)
        {
            yield return new WaitForSeconds(outroDuration);
        }

        // Грузим асинхронно
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }

        // Плавно показываем сцену (Intro)
        PlayIntro();
        if (introDuration > 0f)
        {
            yield return new WaitForSeconds(introDuration);
        }
    }
}


