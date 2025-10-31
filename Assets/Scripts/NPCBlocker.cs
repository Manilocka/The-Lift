using UnityEngine;

public class NPCBlocker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Debug.Log("Игрок упёрся в барьер");
            // Например, можно остановить движение игрока:
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = Vector3.zero;
        }
    }
}