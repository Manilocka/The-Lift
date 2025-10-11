using UnityEngine;

public interface IInteractable
{
    string GetDescription();
    void Interact();
}

public class Door : MonoBehaviour, IInteractable
{

    public Animator m_Animator;
    public bool isOpen;
    
    void Start()
    {
        // Автоматически получаем Animator если не назначен
        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
            {
                Debug.LogError("Door: Animator component not found on " + gameObject.name);
                return;
            }
        }
        
        if (isOpen)
            m_Animator.SetBool("isOpen", true);
    }
    public string GetDescription()
    {
        if (isOpen) 
            return "Нажмите E чтобы закрыть дверь";
        else 
            return "Нажмите E чтобы открыть дверь";
    }
    public void Interact()
    {
        // Проверяем что Animator назначен
        if (m_Animator == null)
        {
            Debug.LogError("Door: Animator not assigned on " + gameObject.name);
            return;
        }
        
        isOpen = !isOpen;
        m_Animator.SetBool("isOpen", isOpen);
        
    }

}
