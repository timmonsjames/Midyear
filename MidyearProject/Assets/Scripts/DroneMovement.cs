using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public CharacterController controller;

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
