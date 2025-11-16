using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DialogueSO introDialogue; // Ссылка на диалог в инспекторе

    void Awake()
    {
        // Проверяем, показывался ли диалог ранее
        if (!PlayerPrefs.HasKey("IntroDialogueShown"))
        {
            // Запускаем диалог, если он ещё не показан
            if (DialogueManager.Instance != null && introDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(introDialogue);

                // Блокируем движение (камера будет управляться DialogueCameraController, если диалог с NPC)
                PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
                if (playerMovement != null) playerMovement.canMove = false;
                
                // Камера блокируется только если диалог с NPC (через DialogueManager)
                // Для внутренних монологов и мыслей камера остается активной
            }
            // Отмечаем, что диалог показан
            PlayerPrefs.SetInt("IntroDialogueShown", 1);
            PlayerPrefs.Save();
        }
    }
}