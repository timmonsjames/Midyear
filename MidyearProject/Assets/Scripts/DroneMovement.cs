using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    public bool canControl = false;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float maxHeight = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!canControl) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (Input.GetKey(KeyCode.Space) && transform.position.y < maxHeight)
        {
            rb.velocity = new Vector3(rb.velocity.x, moveSpeed, rb.velocity.z);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.velocity = new Vector3(rb.velocity.x, -moveSpeed, rb.velocity.z);
        }
    }

}

