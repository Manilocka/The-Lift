using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    [SerializeField] string SceneName;
    [SerializeField] AudioSource doorAudioSource;
    [Header("Local Fade-In on click (to black)")]
    [SerializeField] Animator fadeInAnimator;           // Панель/аниматор на этой сцене
    [SerializeField] float fadeInDuration = 1.0f;       // Длительность затемнения

    const string FadeInState = "FadeIn"; // жёстко: прозрачное -> чёрное

    void OnMouseDown()
    {
        if (!AdditionalCheck.CanLevelBeChanged())
        {
            return;
        }
        DialogueManager.ResetDialogueDone();
        // Debug.Log("DialogueDone " + DialogueManager.DialogueDone);
        
        
        if (doorAudioSource != null)
        {
            doorAudioSource.Play();
        }
        
        
        StartCoroutine(LoadSceneAfterSound());
    }

    IEnumerator LoadSceneAfterSound()
    {
        
        if (doorAudioSource != null && doorAudioSource.clip != null)
        {
            yield return new WaitForSeconds(doorAudioSource.clip.length);
        }
        else
        {
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // Локальный fade-in (к чёрному) перед сменой сцены — без триггеров
        if (fadeInAnimator != null)
        {
            // Убедимся, что панель активна и поверх
            if (!fadeInAnimator.gameObject.activeSelf)
            {
                fadeInAnimator.gameObject.SetActive(true);
            }
            fadeInAnimator.Play(FadeInState, 0, 0f);
            if (fadeInDuration > 0f)
            {
                yield return new WaitForSeconds(fadeInDuration);
            }
        }

        SceneManager.LoadScene(SceneName);
    }
}