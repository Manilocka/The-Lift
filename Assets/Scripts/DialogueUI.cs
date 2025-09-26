using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;
public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject choice1Button;
    public GameObject choice2Button;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;

    public bool isTyping { get; private set; } = false;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowLine(DialogueLine line, Action<int> onChoice)
    {
        speakerNameText.text = line.isPlayer ? "" : line.speakerName;
        string text = line.isThought ? "<i>" + line.text + "</i>" : line.text;
        StartCoroutine(Typewriter(text, line, onChoice));
    }

    IEnumerator Typewriter(string fullText, DialogueLine line, Action<int> onChoice)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;

        choice1Button.SetActive(false);
        choice2Button.SetActive(false);
        if (line.choices.Count > 0)
        {
            choice1Button.SetActive(true);
            choice1Text.text = line.choices[0].choiceText;
            Button button1 = choice1Button.GetComponent<Button>();
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(() => onChoice(0));

            if (line.choices.Count > 1)
            {
                choice2Button.SetActive(true);
                choice2Text.text = line.choices[1].choiceText;
                Button button2 = choice2Button.GetComponent<Button>();
                button2.onClick.RemoveAllListeners();
                button2.onClick.AddListener(() => onChoice(1));
            }
        }
    }
}