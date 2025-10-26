using UnityEngine;

public class PlayerBlocker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок упёрся в барьер");
            // Например, можно остановить движение игрока:
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = Vector3.zero;
        }
    }
}