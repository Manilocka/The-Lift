using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SendElevatorNew : MonoBehaviour
{
    [SerializeField] int floorNumber; // Конечная точка
    [SerializeField] private AudioSource buttonAudioSource; // AudioSource для звука кнопки
    ElevatorMoving platform;
    ElevatorDoorController doorController;
    

    void Start()
    {
        GameObject elevator = GameObject.FindGameObjectWithTag("Elevator");
        platform = elevator.GetComponent<ElevatorMoving>();
        doorController = elevator.GetComponentInChildren<ElevatorDoorController>();
    }

    void Update()
    {
        // Проверяем нажатие левой кнопки мыши
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Проверяем, попал ли луч в этот триггер
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                OnMouseDown();
            }
        }
    }
    
    void OnMouseDown()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "day7_fin")
        {
            // В финальной сцене управление лифтом перехватывается специальной логикой
            Debug.Log("SendElevatorNew: управление отключено в day7_fin");
            return;
        }
        if (scene == "day6_e")
        {
            // Если мы находимся на midStop и диалог ещё идёт — НЕ разрешаем ехать дальше
            if (platform != null && 
                platform.midStop >= 0 && 
                platform.currentTarget == platform.midStop && 
                !AdditionalCheck.IsDialogueDone())
            {
                Debug.Log("SendElevatorNew: Диалог на промежуточной остановке ещё идёт — движение заблокировано!");
                if (buttonAudioSource != null) buttonAudioSource.Play(); // можно звук нажатия оставить
                return; // полностью блокируем запуск лифта
            }
        }

        // Воспроизводим звук кнопки через AudioSource
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
        
        if (platform != null && !platform.canMove)
        {
            // Если лифт уже на этом этаже - просто открываем двери.
            // Но если кнопка находится внутри кабины — не возвращаемся, позволяем обработать нажатие (поездка вверх и т.п.).
            if (platform.startStop == floorNumber)
            {
                if (doorController != null && !doorController.isOpen)
                {
                    doorController.OpenDoors();
                }

                // Если это внешняя кнопка (не находится в иерархии лифта), прекращаем обработку.
                if (!transform.IsChildOf(platform.transform))
                {
                    return;
                }
                // Иначе — это внутренняя кнопка; продолжаем и допускаем смену endStop.
            }
            
            if (scene == "day4_e")
            {
                platform.endStop = 1;
            }
            else if (scene == "day7_e")
            {
                platform.endStop = 2;
            }
            else
            {
                // Проверяем, не находится ли лифт уже на этом этаже
                if (platform.startStop == 3)
                {
                    platform.endStop = 0;
                }
                else
                {
                    // Лифт на другом этаже - устанавливаем конечную точку и запускаем движение
                    platform.endStop = 3;
                }
            }
            // Запускаем корутину, которая управляет закрытием дверей и проверкой игрока
            StartCoroutine(DelayedStartWithDoorCheck(1f, 5f)); // Задержка закрытия дверей, задержка старта
        }
    }

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Ждем, пока игрок не окажется внутри лифта.
        // Это предотвращает ситуацию: игрок нажал кнопку внутри, вышел, и лифт уехал без него.
        // ВАЖНО: Проверяем только для внутренних кнопок (кнопки внутри кабины лифта)
        bool isInternalButton = (platform != null && transform.IsChildOf(platform.transform));
        
        PlayerInLiftCheck liftCheck = null;
        if (isInternalButton)
        {
            liftCheck = FindObjectOfType<PlayerInLiftCheck>(true);
            if (liftCheck != null)
            {
                // Если игрок уже внутри — продолжаем
                if (!liftCheck.IsPlayerInsideElevator())
                {
                    Debug.Log("SendElevatorNew: Ждем пока игрок зайдет в лифт перед стартом движения...");
                    // Ждем максимум 30 секунд — если игрок не зайдет, лифт не поедет
                    float timeout = 30f;
                    float elapsed = 0f;
                    while (!liftCheck.IsPlayerInsideElevator() && elapsed < timeout)
                    {
                        yield return null;
                        elapsed += Time.deltaTime;
                    }
                    
                    // Если таймаут истек, отменяем движение
                    if (elapsed >= timeout && !liftCheck.IsPlayerInsideElevator())
                    {
                        Debug.LogWarning("SendElevatorNew: Таймаут ожидания игрока истек, движение лифта отменено");
                        yield break; // Прекращаем выполнение корутины
                    }
                }
            }
        }

        // КРИТИЧЕСКАЯ ПРОВЕРКА: Перед запуском движения проверяем, что игрок все еще в лифте
        // Это предотвращает ситуацию, когда игрок выбежал из лифта во время ожидания
        if (isInternalButton && liftCheck != null)
        {
            // Проверяем еще раз прямо перед запуском
            if (!liftCheck.IsPlayerInsideElevator())
            {
                Debug.LogWarning("SendElevatorNew: Игрок вышел из лифта перед стартом, движение отменено");
                // Открываем двери, если они закрыты, чтобы игрок мог вернуться
                if (doorController != null && !doorController.isOpen)
                {
                    doorController.OpenDoors();
                }
                yield break; // Прекращаем выполнение корутины
            }
            
            // Дополнительная проверка: ждем пока двери полностью закроются и проверяем, что игрок все еще внутри
            if (doorController != null)
            {
                // Ждем, пока двери закроются
                while (!doorController.AreDoorsFullyClosed())
                {
                    // Во время закрытия дверей проверяем, что игрок не выбежал
                    if (!liftCheck.IsPlayerInsideElevator())
                    {
                        Debug.LogWarning("SendElevatorNew: Игрок выбежал из лифта во время закрытия дверей, движение отменено");
                        // Останавливаем закрытие дверей и открываем их обратно
                        doorController.OpenDoors();
                        yield break; // Прекращаем выполнение корутины
                    }
                    yield return null;
                }
                
                // // Финальная проверка после закрытия дверей
                // if (!liftCheck.IsPlayerInsideElevator())
                // {
                //     Debug.LogWarning("SendElevatorNew: Игрок вышел из лифта после закрытия дверей, движение отменено");
                //     doorController.OpenDoors();
                //     yield break; // Прекращаем выполнение корутины
                // }
            }
        }

        // Все проверки пройдены - запускаем движение лифта
        if (platform != null)
        {
            platform.canMove = true; // Запускаем движение лифта только если игрок внутри
            Debug.Log("SendElevatorNew: Лифт запущен, игрок внутри");
        }
    }
    private IEnumerator DelayedStartWithDoorCheck(float doorCloseDelay, float startDelay)
    {
        // Ждем перед закрытием дверей
        yield return new WaitForSeconds(doorCloseDelay);
        
        // Проверяем, является ли это внутренней кнопкой
        bool isInternalButton = (platform != null && transform.IsChildOf(platform.transform));
        PlayerInLiftCheck liftCheck = isInternalButton ? FindObjectOfType<PlayerInLiftCheck>(true) : null;
        
        // Если это внутренняя кнопка, проверяем, что игрок все еще в лифте перед закрытием дверей
        if (isInternalButton && liftCheck != null)
        {
            if (!liftCheck.IsPlayerInsideElevator())
            {
                Debug.LogWarning("SendElevatorNew: Игрок вышел из лифта перед закрытием дверей, отменяем движение");
                yield break; // Прекращаем выполнение корутины
            }
        }
        
        // Закрываем двери
        if (doorController != null)
        {
            doorController.CloseDoors();
        }
        
        // Если это внутренняя кнопка, следим за закрытием дверей и проверяем, что игрок не выбежал
        if (isInternalButton && liftCheck != null && doorController != null)
        {
            // Во время закрытия дверей проверяем, что игрок не выбежал
            while (!doorController.AreDoorsFullyClosed())
            {
                if (!liftCheck.IsPlayerInsideElevator())
                {
                    Debug.LogWarning("SendElevatorNew: Игрок выбежал из лифта во время закрытия дверей, отменяем движение");
                    // Останавливаем закрытие и открываем двери обратно
                    doorController.OpenDoors();
                    yield break; // Прекращаем выполнение корутины
                }
                yield return null;
            }
            
            // Финальная проверка после закрытия дверей
            if (!liftCheck.IsPlayerInsideElevator())
            {
                Debug.LogWarning("SendElevatorNew: Игрок вышел из лифта после закрытия дверей, отменяем движение");
                doorController.OpenDoors();
                yield break; // Прекращаем выполнение корутины
            }
        }
        
        // Ждем оставшееся время до старта
        float remainingDelay = startDelay - doorCloseDelay;
        if (remainingDelay > 0)
        {
            yield return new WaitForSeconds(remainingDelay);
        }
        
        // Финальная проверка перед запуском движения
        if (isInternalButton && liftCheck != null)
        {
            if (!liftCheck.IsPlayerInsideElevator())
            {
                Debug.LogWarning("SendElevatorNew: Игрок вышел из лифта перед стартом движения, отменяем");
                if (doorController != null && !doorController.isOpen)
                {
                    doorController.OpenDoors();
                }
                yield break; // Прекращаем выполнение корутины
            }
        }
        
        // Все проверки пройдены - запускаем движение лифта
        if (platform != null)
        {
            platform.canMove = true; // Запускаем движение лифта только если игрок внутри
            Debug.Log("SendElevatorNew: Лифт запущен, игрок внутри");
        }
    }
    
    private IEnumerator DelayedDoorClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (doorController != null)
        {
            doorController.CloseDoors(); // Закрываем двери после задержки
        }
    }
}