using UnityEngine;

public class DroneStation : MonoBehaviour
{
    public Transform player;
    public float activationRange = 3f;
    public POVmanager povManager;

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= activationRange && Input.GetKeyDown(KeyCode.E))
        {
            povManager.TogglePOV();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
