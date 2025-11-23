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

    [Header("Audio")]
    [Tooltip("Звук, который проигрывается при появлении нового текста")]
    public AudioSource textAppearSoundSource;
    public AudioClip textAppearSound;

    public bool isTyping { get; private set; } = false;
    
    // Цвета для подсветки выбора
    private Color normalChoiceColor = Color.white;
    private Color selectedChoiceColor = Color.yellow;
    
    // Переменные для мигания
    private bool isBlinking = false;
    private float blinkSpeed = 4f; // Увеличено с 2f до 4f для более быстрого мигания
    private float blinkTimer = 0f;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        isBlinking = false;
        gameObject.SetActive(false);
    }
    
    public void UpdateChoiceHighlight(int selectedChoice, int totalChoices)
    {
        // Сбрасываем все кнопки к обычному цвету
        choice1Text.color = normalChoiceColor;
        if (choice2Text != null)
            choice2Text.color = normalChoiceColor;
        
        // Включаем мигание для выбранной опции
        isBlinking = true;
        blinkTimer = 0f;
    }
    
    void Update()
    {
        if (isBlinking)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            float alpha = (Mathf.Sin(blinkTimer) + 1f) / 2f; // От 0 до 1
            
            // Получаем текущий выбранный выбор из DialogueManager
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                var currentLine = DialogueManager.Instance.GetCurrentLine();
                if (currentLine.HasValue && currentLine.Value.choices.Count > 0)
                {
                    int selectedChoice = DialogueManager.Instance.GetSelectedChoice();
                    
                    // Применяем мигание к выбранной опции
                    if (selectedChoice == 0)
                    {
                        Color blinkColor = selectedChoiceColor;
                        blinkColor.a = alpha;
                        choice1Text.color = blinkColor;
                    }
                    if (selectedChoice == 1 && currentLine.Value.choices.Count > 1)
                    {
                        Color blinkColor = selectedChoiceColor;
                        blinkColor.a = alpha;
                        choice2Text.color = blinkColor;
                    }
                }
            }
        }
    }

    public void ShowLine(DialogueLine line, Action<int> onChoice)
    {
        speakerNameText.text = line.isPlayer ? "" : line.speakerName;
        string text = line.text;
        
        // Воспроизводим звук при появлении нового текста
        PlayTextAppearSound();
        
        StartCoroutine(Typewriter(text, line, onChoice));
    }
    
    private void PlayTextAppearSound()
    {
        if (textAppearSound != null)
        {
            if (textAppearSoundSource != null)
            {
                textAppearSoundSource.PlayOneShot(textAppearSound);
            }
            else
            {
                // Если AudioSource не назначен, создаем временный
                AudioSource.PlayClipAtPoint(textAppearSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }
        }
    }

    IEnumerator Typewriter(string fullText, DialogueLine line, Action<int> onChoice)
    {
        // Сначала сразу убираем кнопки выбора
        choice1Button.SetActive(false);
        choice2Button.SetActive(false);
        
        isTyping = true;
        dialogueText.text = "";
        
        // Применяем курсив и серый цвет для мыслей
        if (line.isThought)
        {
            dialogueText.fontStyle = FontStyles.Italic;
            ColorUtility.TryParseHtmlString("#D3D3D3", out Color lightGray);
            dialogueText.color = lightGray;
        }
        else
        {
            dialogueText.fontStyle = FontStyles.Normal;
            dialogueText.color = Color.white; // Возвращаем белый цвет для обычного текста
        }
        
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f); // Ускорено с 0.05f до 0.03f
            
            // Проверяем, не нажал ли пользователь левую кнопку мыши, пробел или Enter для пропуска
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // Показываем весь текст сразу
                dialogueText.text = fullText;
                break;
            }
        }
        isTyping = false;
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