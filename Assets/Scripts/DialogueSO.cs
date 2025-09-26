using UnityEngine;
using System.Collections.Generic; 

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLine> lines;
}

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    public string text;
    public bool isPlayer;
    public bool isThought;
    public List<Choice> choices;
}

[System.Serializable] 
public struct Choice
{
    public string choiceText;
    public int nextLineIndex;
}