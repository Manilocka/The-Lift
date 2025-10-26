using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueOnClickWithDelay : DialogueOnClick
{
    public float delay = 2f; 

    

    public new void OnMouseDown()
    {
        StartCoroutine(ShowDialogueAfterDelay(delay));
    }
    
    private IEnumerator ShowDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDialogue();
    }
    
}