using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLine> lines;
}