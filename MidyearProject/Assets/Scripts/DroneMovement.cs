using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float verticalSpeed = 4f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float y = 0f;
        if (Input.GetKey(KeyCode.Space)) y = 1f;
        if (Input.GetKey(KeyCode.LeftControl)) y = -1f;

        Vector3 velocity = (move * moveSpeed) + Vector3.up * y * verticalSpeed;
        rb.velocity = velocity;
    }
}
