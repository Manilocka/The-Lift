using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevaorMoving : MonoBehaviour
{
    public bool canMove;

    [SerializeField] float speed;
    [SerializeField] int startStop;
    [SerializeField] int endStop;
    [SerializeField] Transform[] stops;


    int currentTarget;
    bool reverse;

    void Start()
    {
        transform.position = stops[startStop].position;
        currentTarget = startStop;
        UpdateDirection();
    }

    void Update()
    {
        if (!canMove) return;
        UpdateDirection();

        // Проверяем, достигли ли текущей остановки
        if (Vector3.Distance(transform.position, stops[currentTarget].position) < 0.01f)
        {
            if (currentTarget == endStop)
            {
                canMove = false;
                startStop = endStop; // Обновляем начальную остановку
                currentTarget = endStop;
                return;
            }

            if (reverse)
            {
                currentTarget--;
            }
            else
            {
                currentTarget++;
            }
        }

        // Движение к текущей цели
        transform.position = Vector3.MoveTowards(transform.position, stops[currentTarget].position, speed * Time.deltaTime);
    }




    private void UpdateDirection()
    {
        if (endStop > currentTarget)
        {
            reverse = false;
        }
        else if (endStop < currentTarget)
        {
            reverse = true;
        }
    }
}
