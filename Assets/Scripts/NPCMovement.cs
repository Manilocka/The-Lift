using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Character Controller")]
    public CharacterController controller;

    [Header("Movement Settings")]
    public float speed = 3.25f; // скорость передвижения
    public float gravity = -60f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Target Point")]
    public Transform targetPoint; // точка назначения, задаётся в инспекторе

    private Vector3 velocity;
    private bool isGrounded;
    public bool canMove = true;

    void Update()
    {
        // Проверяем, стоит ли NPC на земле
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Если разрешено движение и есть цель
        if (canMove && targetPoint != null)
        {
            MoveToTarget();
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void MoveToTarget()
    {
        // Получаем направление к цели
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        direction.y = 0; // не двигаемся по вертикали

        // Двигаем NPC в сторону цели
        controller.Move(direction * speed * Time.deltaTime);

        // Плавно поворачиваемся в сторону цели
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Останавливаемся, если почти дошли
        float distance = Vector3.Distance(transform.position, targetPoint.position);
        if (distance < 1f)
        {
            canMove = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (targetPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint.position);
            Gizmos.DrawSphere(targetPoint.position, 0.2f);
        }
    }
}
