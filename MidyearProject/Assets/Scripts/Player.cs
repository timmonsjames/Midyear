using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Rigidbody rb;

    void Start()
    {
        transform.position = new Vector3(0, 10, 0);
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ);
        rb.AddForce(movement * moveSpeed);

        if (Input.GetKeyDown(KeyCode.H))
            rb.AddForce(1000 * Vector3.up);
    }
}
