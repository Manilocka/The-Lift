using UnityEngine;
using System.Collections.Generic; 

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLine> lines;
}