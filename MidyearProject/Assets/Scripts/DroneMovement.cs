using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public CharacterController controller;
    public bool signal = false;
    public float posX;
    public float posY;

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;


        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            signal = true;
            Debug.Log("Drone signal out");
        }

        posX = transform.position.x;
        posY = transform.position.z;
    }
}
