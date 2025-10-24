using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // сохраняем фейдер при смене сцен
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutIn(sceneName));
    }

    private IEnumerator FadeOutIn(string sceneName)
    {
        // Плавное затемнение
        yield return StartCoroutine(Fade(0f, 1f));

        // Загружаем новую сцену
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Плавное осветление
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = endAlpha;
    }
}