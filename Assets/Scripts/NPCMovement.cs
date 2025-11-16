using UnityEngine;
using UnityEngine.SceneManagement;

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
    public Transform targetPoint3; // вторая цель (опционально)
    public Transform targetPoint4;
    public Transform targetPoint5;

    [Header("Animation")]
    public Animator animator; // ссылка на аниматор

    private Vector3 velocity;
    private bool isGrounded;
    public bool canMove = false;
    private bool isWalking = false;
    private bool wasWalking = false; // предыдущее состояние

    // Текущая цель, к которой NPC идёт
    private Transform currentTarget;
    
    // Для преследования игрока
    private Transform playerTransform;
    private bool shouldChasePlayer = false;

    void Start()
    {
        // Проверяем, нужно ли преследовать игрока (в сцене day7_fin)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "day7_fin")
        {
            shouldChasePlayer = true;
            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                // Создаем временный объект для хранения позиции игрока
                GameObject tempTarget = new GameObject("PlayerTarget");
                tempTarget.transform.position = playerTransform.position;
                currentTarget = tempTarget.transform;
            }
            else
            {
                Debug.LogError("NPCMovement: Игрок с тегом 'Player' не найден в сцене day7_fin!");
                shouldChasePlayer = false;
                currentTarget = targetPoint;
            }
        }
        else
        {
            // Начинаем движение к первой цели
            currentTarget = targetPoint;
        }
        
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

        // Если нужно преследовать игрока, обновляем цель
        if (shouldChasePlayer && playerTransform != null && currentTarget != null)
        {
            currentTarget.position = playerTransform.position;
        }

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
        
        // Если преследуем игрока, не останавливаемся при достижении близкого расстояния
        if (shouldChasePlayer)
        {
            // Продолжаем преследование, не останавливаемся
            return;
        }
        
        if (distance < 30f)
        {
            Debug.Log("Достигнута целевая точка, расстояние: " + distance);
            
            // Если дошли до первой цели и есть вторая — переключаемся
            if (currentTarget == targetPoint && targetPoint2 != null)
            {
                Debug.Log("Переключаемся на вторую цель");
                currentTarget = targetPoint2;
            }
            else if (currentTarget == targetPoint2 && targetPoint3 != null)
            {
                Debug.Log("Переключаемся на третью цель");
                currentTarget = targetPoint3;
            }
            else if (currentTarget == targetPoint3 && targetPoint4 != null)
            {
                Debug.Log("Переключаемся на четвертую цель");
                currentTarget = targetPoint4;
            }
            else if (currentTarget == targetPoint4 && targetPoint5 != null)
            {
                Debug.Log("Переключаемся на четвертую цель");
                currentTarget = targetPoint5;
            }
            else
            {
                // // Иначе останавливаемся
                // Debug.Log("Останавливаем NPC");
                // canMove = false;
                // isWalking = false; // Важно: сразу устанавливаем флаг ходьбы в false
                // UpdateAnimator(); // Немедленно обновляем аниматор
                Debug.Log("NPC достиг конечной точки - исчезает");
                HideNPC();
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

    void OnDestroy()
    {
        // Удаляем временный объект цели при уничтожении компонента
        if (shouldChasePlayer && currentTarget != null && currentTarget.name == "PlayerTarget")
        {
            Destroy(currentTarget.gameObject);
        }
    }

    void HideNPC()
    {
        canMove = false;
        isWalking = false;
        UpdateAnimator();

        // Способ 1: Отключить игровой объект (рекомендуется)
        gameObject.SetActive(false);
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
        
        if (targetPoint3 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetPoint3.position, 0.2f);
        }
        if (targetPoint4 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetPoint4.position, 0.2f);
        }
        if (targetPoint5 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(targetPoint4.position, 0.2f);
        }
        
        // Показываем линию к игроку, если преследуем
        if (shouldChasePlayer && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}