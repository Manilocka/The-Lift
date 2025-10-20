using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Character Controller")]
    public CharacterController controller;

    [Header("Movement Settings")]
    public float speed = 3.25f;
    public float gravity = -60f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Target Points")]
    public Transform targetPoint;  // первая цель
    public Transform targetPoint2; // вторая цель (опционально)

    [Header("Animation")]
    public Animator animator; // ссылка на аниматор

    private Vector3 velocity;
    private bool isGrounded;
    public bool canMove = false;
    private bool isWalking = false;
    private bool wasWalking = false; // предыдущее состояние

    // Текущая цель, к которой NPC идёт
    private Transform currentTarget;

    void Start()
    {
        // Начинаем движение к первой цели
        currentTarget = targetPoint;
        
        // Автоматически находим аниматор если он не установлен
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
        
        // Инициализируем аниматор
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    void Update()
    {
        // Проверка, на земле ли NPC
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        canMove = AdditionalCheck.CanNPCMove();

        // Сохраняем предыдущее состояние для сравнения
        wasWalking = isWalking;

        // Определяем, движется ли NPC
        if (canMove && currentTarget != null)
        {
            MoveToTarget();
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        // Обновляем аниматор только если состояние изменилось
        if (wasWalking != isWalking)
        {
            UpdateAnimator();
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void MoveToTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;

        controller.Move(direction * speed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance < 30f)
        {
            Debug.Log("Достигнута целевая точка, расстояние: " + distance);
            
            // Если дошли до первой цели и есть вторая — переключаемся
            if (currentTarget == targetPoint && targetPoint2 != null)
            {
                Debug.Log("Переключаемся на вторую цель");
                currentTarget = targetPoint2;
            }
            else
            {
                // Иначе останавливаемся
                Debug.Log("Останавливаем NPC");
                canMove = false;
                isWalking = false; // Важно: сразу устанавливаем флаг ходьбы в false
                UpdateAnimator(); // Немедленно обновляем аниматор
            }
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            Debug.Log($"Animator updated: isWalking = {isWalking}");
        }
        else
        {
            Debug.LogWarning("Animator is not assigned!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (targetPoint != null)
        {
            Gizmos.DrawLine(transform.position, targetPoint.position);
            Gizmos.DrawSphere(targetPoint.position, 0.2f);
        }

        if (targetPoint2 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetPoint2.position, 0.2f);
        }
    }
}