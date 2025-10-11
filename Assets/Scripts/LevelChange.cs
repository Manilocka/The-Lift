using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    [SerializeField] string SceneName;


    void OnMouseDown()
    {
        DialogueManager.ResetDialogueDone();
        Debug.Log("DialogueDone " + DialogueManager.DialogueDone);
        SceneManager.LoadScene(SceneName);
        
    }


}