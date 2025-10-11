using System;
using System.Collections.Generic;

[Serializable]
public struct DialogueLine {
    public string speakerName;
    public string text;
    public bool isPlayer;
    public bool isThought;
    public List<Choice> choices;
}
[Serializable]
public struct Choice {
    public string choiceText;
    public List<DialogueLine> dialogueLines; // Список реплик для этого выбора
}