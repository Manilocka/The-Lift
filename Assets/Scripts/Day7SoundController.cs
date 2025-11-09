using UnityEngine;
using System.Collections;

public class Day7SoundController : MonoBehaviour
{
    [Header("Audio Sources (без AudioClip)")]
    [Tooltip("Звук полиции - играет до запуска диалога")]
    public AudioSource policeSoundSource;
    
    [Tooltip("Звук взлома двери - играет после фразы 'Вскрывайте!'")]
    public AudioSource doorBreakSoundSource;
    
    [Tooltip("Звук стука в дверь - играет после фразы 'Уже поздно'")]
    public AudioSource doorKnockSoundSource;
    
    [Header("Настройки")]
    [Tooltip("Имя диалога для отслеживания")]
    public string targetDialogueName = "day7_n_1";
    
    private bool dialogueStarted = false;
    private bool doorBreakPlayed = false;
    private bool doorKnockPlayed = false;
    private bool doorKnockLooping = false;
    private bool notOpenChoiceSelected = false;
    
    void Start()
    {
        // Проверяем, что мы в правильной сцене
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "day7_1")
        {
            Debug.LogWarning($"Day7SoundController: Эта сцена ({currentScene}) не day7_1. Скрипт может работать некорректно.");
        }
        
        // Настраиваем AudioSource для звука полиции (loop)
        if (policeSoundSource != null)
        {
            policeSoundSource.loop = true;
            policeSoundSource.playOnAwake = false;
            // Запускаем звук полиции сразу
            policeSoundSource.Play();
            Debug.Log("Day7SoundController: Звук полиции запущен");
        }
        else
        {
            Debug.LogError("Day7SoundController: policeSoundSource не назначен!");
        }
        
        // Настраиваем AudioSource для звука взлома двери
        if (doorBreakSoundSource != null)
        {
            doorBreakSoundSource.loop = false;
            doorBreakSoundSource.playOnAwake = false;
        }
        else
        {
            Debug.LogError("Day7SoundController: doorBreakSoundSource не назначен!");
        }
        
        // Настраиваем AudioSource для звука стука в дверь
        if (doorKnockSoundSource != null)
        {
            doorKnockSoundSource.loop = false; // Будем управлять loop вручную
            doorKnockSoundSource.playOnAwake = false;
        }
        else
        {
            Debug.LogError("Day7SoundController: doorKnockSoundSource не назначен!");
        }
    }
    
    void Update()
    {
        // Проверяем, запустился ли диалог day7_n_1
        if (!dialogueStarted && DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            var currentDialogue = DialogueManager.Instance.GetCurrentDialogue();
            if (currentDialogue != null && currentDialogue.name == targetDialogueName)
            {
                dialogueStarted = true;
                // Останавливаем звук полиции
                if (policeSoundSource != null && policeSoundSource.isPlaying)
                {
                    policeSoundSource.Stop();
                    Debug.Log("Day7SoundController: Звук полиции остановлен (диалог начался)");
                }
            }
        }
        
        // Отслеживаем завершение диалога или переход на другую сцену
        // Если звук стука в loop и диалог завершился или переходит на другую сцену, останавливаем звук
        if (doorKnockLooping && doorKnockSoundSource != null && doorKnockSoundSource.isPlaying)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive)
            {
                // Диалог завершился - останавливаем звук
                StopDoorKnockLoop();
            }
            else
            {
                // Проверяем, переходит ли диалог на другую сцену (day7_2)
                var currentDialogue = DialogueManager.Instance.GetCurrentDialogue();
                var currentLine = DialogueManager.Instance.GetCurrentLine();
                if (currentLine.HasValue && currentLine.Value.choices.Count > 0)
                {
                    foreach (var choice in currentLine.Value.choices)
                    {
                        if (choice.choiceText.Contains("Открыть дверь") && !string.IsNullOrEmpty(choice.sceneToLoad))
                        {
                            // Игрок выбрал "Открыть дверь" и будет переход на другую сцену
                            // Останавливаем звук
                            StopDoorKnockLoop();
                        }
                    }
                }
            }
        }
        
        // Если диалог активен, проверяем текущую строку
        if (dialogueStarted && DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            var currentDialogue = DialogueManager.Instance.GetCurrentDialogue();
            if (currentDialogue != null && currentDialogue.name == targetDialogueName)
            {
                var currentLine = DialogueManager.Instance.GetCurrentLine();
                if (currentLine.HasValue)
                {
                    int currentIndex = DialogueManager.Instance.GetCurrentIndex();
                    
                    // Проверяем строку "Вскрывайте!" (индекс 1)
                    if (currentIndex == 1 && !doorBreakPlayed)
                    {
                        if (currentLine.Value.text.Contains("Вскрывайте"))
                        {
                            PlayDoorBreakSound();
                            doorBreakPlayed = true;
                        }
                    }
                    
                    // Проверяем строку "Уже поздно." (индекс 3)
                    if (currentIndex == 3 && !doorKnockPlayed)
                    {
                        if (currentLine.Value.text.Contains("Уже поздно"))
                        {
                            PlayDoorKnockSound();
                            doorKnockPlayed = true;
                        }
                    }
                    
                    // Проверяем, есть ли выбор "Не открывать"
                    if (currentLine.Value.choices.Count > 0)
                    {
                        // Проверяем, выбран ли вариант "Не открывать"
                        foreach (var choice in currentLine.Value.choices)
                        {
                            if (choice.choiceText.Contains("Не открывать"))
                            {
                                // Если выбор еще не был сделан, но мы на строке с выбором
                                // Ждем, пока игрок выберет вариант
                                CheckForNotOpenChoice();
                            }
                        }
                    }
                }
            }
        }
        
        // Если звук стука должен быть в loop и он не играет, запускаем его снова
        if (doorKnockLooping && doorKnockSoundSource != null && !doorKnockSoundSource.isPlaying)
        {
            doorKnockSoundSource.Play();
        }
        
        // Отслеживаем выбор "Открыть дверь" после выбора "Не открывать"
        // Это происходит в поддиалоге, когда игрок видит выбор "Открыть дверь"
        // Когда игрок выберет этот вариант, произойдет переход на сцену day7_2
        // Мы остановим звук при завершении диалога (см. проверку выше)
    }
    
    void OnDestroy()
    {
        // Останавливаем все звуки при уничтожении объекта
        if (policeSoundSource != null && policeSoundSource.isPlaying)
        {
            policeSoundSource.Stop();
        }
        if (doorBreakSoundSource != null && doorBreakSoundSource.isPlaying)
        {
            doorBreakSoundSource.Stop();
        }
        if (doorKnockSoundSource != null && doorKnockSoundSource.isPlaying)
        {
            doorKnockSoundSource.Stop();
        }
    }
    
    private void PlayDoorBreakSound()
    {
        if (doorBreakSoundSource != null)
        {
            doorBreakSoundSource.Play();
            Debug.Log("Day7SoundController: Звук взлома двери воспроизведен");
        }
    }
    
    private void PlayDoorKnockSound()
    {
        if (doorKnockSoundSource != null)
        {
            doorKnockSoundSource.Play();
            Debug.Log("Day7SoundController: Звук стука в дверь воспроизведен");
        }
    }
    
    private void CheckForNotOpenChoice()
    {
        // Проверяем, был ли выбран вариант "Не открывать"
        // Это происходит в OnChoiceSelected в DialogueManager
        // Нам нужно отследить это через проверку текущего диалога
        
        if (DialogueManager.Instance != null)
        {
            var currentDialogue = DialogueManager.Instance.GetCurrentDialogue();
            if (currentDialogue != null)
            {
                // Если мы в поддиалоге после выбора "Не открывать"
                // Проверяем, есть ли в текущем диалоге строка "Мне страшно"
                var currentLine = DialogueManager.Instance.GetCurrentLine();
                if (currentLine.HasValue)
                {
                    // Проверяем, если это строка "Мне страшно" - значит был выбран "Не открывать"
                    if (currentLine.Value.text.Contains("Мне страшно"))
                    {
                        if (!notOpenChoiceSelected)
                        {
                            notOpenChoiceSelected = true;
                            StartDoorKnockLoop();
                        }
                    }
                    
                    // Проверяем, есть ли выбор "Открыть дверь" в текущей строке
                    if (notOpenChoiceSelected && currentLine.Value.choices.Count > 0)
                    {
                        foreach (var choice in currentLine.Value.choices)
                        {
                            if (choice.choiceText.Contains("Открыть дверь"))
                            {
                                // Игрок может выбрать "Открыть дверь" - останавливаем loop при выборе
                                // Но пока только проверяем наличие выбора, сам выбор отследим через OnChoiceSelected
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void StartDoorKnockLoop()
    {
        if (doorKnockSoundSource != null)
        {
            doorKnockLooping = true;
            doorKnockSoundSource.loop = true;
            if (!doorKnockSoundSource.isPlaying)
            {
                doorKnockSoundSource.Play();
            }
            Debug.Log("Day7SoundController: Звук стука в дверь запущен в loop");
        }
    }
    
    private void StopDoorKnockLoop()
    {
        if (doorKnockSoundSource != null)
        {
            doorKnockLooping = false;
            doorKnockSoundSource.loop = false;
            doorKnockSoundSource.Stop();
            Debug.Log("Day7SoundController: Звук стука в дверь остановлен (выбран вариант 'Открыть дверь')");
        }
    }
    
    // Метод для отслеживания выбора через события (если понадобится)
    public void OnChoiceSelected(string choiceText)
    {
        if (choiceText.Contains("Не открывать"))
        {
            notOpenChoiceSelected = true;
            StartDoorKnockLoop();
        }
        else if (choiceText.Contains("Открыть дверь") && notOpenChoiceSelected)
        {
            StopDoorKnockLoop();
        }
    }
}

