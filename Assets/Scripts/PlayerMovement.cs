using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 325f; // Увеличено на 30% (250 * 1.3 = 325)
    public float gravity = -60f;

    public Transform groundCheck; // to check if the player is on the ground
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    // Новые переменные для диалога
    public bool canMove = true; // Переменная для блокировки движения
    private Camera mainCamera; // Для Raycast от камеры
    
    [Header("Footstep Audio")]
    public AudioSource footstepAudioSource; // AudioSource для звуков шагов
    public float footstepInterval = 1f; // Интервал между шагами (в секундах)
    private float footstepTimer = 0f;

    void Start()
    {
        // Инициализация камеры
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please assign a camera with the tag 'MainCamera'.");
        }
        
        // Проверяем и настраиваем AudioSource
        if (footstepAudioSource == null)
        {
            Debug.LogError("Footstep AudioSource not assigned! Please assign an AudioSource in the inspector.");
        }
        else
        {
            Debug.Log("Footstep AudioSource found: " + footstepAudioSource.name);
            if (footstepAudioSource.clip == null)
            {
                Debug.LogError("No audio clip assigned to Footstep AudioSource!");
            }
            else
            {
                Debug.Log("Audio clip assigned: " + footstepAudioSource.clip.name);
                
                // Настраиваем AudioSource для правильной работы
                footstepAudioSource.loop = false; // Отключаем зацикливание (управляем вручную)
                footstepAudioSource.playOnAwake = false; // Не воспроизводить автоматически
                
                // ВАЖНО: Убеждаемся, что AudioSource включен и готов к работе
                footstepAudioSource.enabled = true;
                
                Debug.Log("AudioSource настроен: Loop=" + footstepAudioSource.loop + ", Volume=" + footstepAudioSource.volume + ", Interval=" + footstepInterval + "s");
            }
        }
    }
    
    void OnEnable()
    {
        // При активации объекта убеждаемся, что AudioSource включен
        if (footstepAudioSource != null)
        {
            footstepAudioSource.enabled = true;
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (canMove)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);
            
            // Логика звуков шагов с настраиваемым интервалом
            bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
            
            if (isMoving)
            {   
                if (footstepTimer == 0f) footstepAudioSource.Play();
                footstepTimer += Time.deltaTime;
                
                if (footstepAudioSource != null && footstepTimer >= footstepInterval)
                {
                    // Debug.Log("Playing footstep sound - interval: " + footstepInterval + "s");
                    footstepAudioSource.Play();
                    footstepTimer = 0f; // Сбрасываем таймер
                }
            }
            else
            {
                footstepTimer = 0f; // Сбрасываем таймер при остановке
                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    // Debug.Log("Stopping footstep sound");
                    footstepAudioSource.Stop();
                }
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
    
