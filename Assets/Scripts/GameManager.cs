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

                // Блокируем движение и взгляд (как в вашем коде)
                PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
                if (playerMovement != null) playerMovement.canMove = false;
                MouseLock mouseLock = FindObjectOfType<MouseLock>();
                if (mouseLock != null)
                {
                    mouseLock.canLook = false;
                    mouseLock.crosshair.gameObject.SetActive(false);
                }
            }
            // Отмечаем, что диалог показан
            PlayerPrefs.SetInt("IntroDialogueShown", 1);
            PlayerPrefs.Save();
        }
    }
}