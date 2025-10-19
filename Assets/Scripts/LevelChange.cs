using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    [SerializeField] string SceneName;
    [SerializeField] AudioSource doorAudioSource;

    void OnMouseDown()
    {
        if (!AdditionalCheck.CanLevelBeChanged())
        {
            return;
        }
        DialogueManager.ResetDialogueDone();
        Debug.Log("DialogueDone " + DialogueManager.DialogueDone);
        
        
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
        
        SceneManager.LoadScene(SceneName);
    }
}