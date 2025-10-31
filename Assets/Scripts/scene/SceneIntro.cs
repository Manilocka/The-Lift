using System.Collections;
using UnityEngine;

public class SceneIntro : MonoBehaviour
{
    [SerializeField] Animator fadeAnimator;   // Панель/аниматор этой сцены
    [SerializeField] float fadeOutDuration = 1.0f; // длительность клипа FadeOut
    [SerializeField] bool disablePanelAfter = true; // отключить объект после проигрыша

    const string FadeOutState = "FadeOut"; // жёстко: чёрное -> прозрачное

    void Start()
    {
        if (fadeAnimator != null)
        {
            // Немедленно играем нужный стейт по имени, без триггеров
            fadeAnimator.Play(FadeOutState, 0, 0f);
            if (fadeOutDuration > 0f)
            {
                StartCoroutine(DisableLater());
            }
        }
    }

    IEnumerator DisableLater()
    {
        yield return new WaitForSeconds(fadeOutDuration);
        if (disablePanelAfter && fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(false);
        }
    }
}


