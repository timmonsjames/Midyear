using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public CharacterController controller;
    public bool signal = false;
    public bool signalCooldownStart = false;
    public float signalCooldown = 60f;
    public float time = 0f;
    public int displayTime = 0;
    public float posX;
    public float posY;

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;


        controller.Move(move * moveSpeed * Time.deltaTime);

        posX = transform.position.x;
        posY = transform.position.z;
    }

    private void Update()
    {
        if (time < 0 && Input.GetKeyDown(KeyCode.Space))
        {
            signal = true;
            signalCooldownStart = true;
            Debug.Log("Drone signal out");
        }
        if (!signal && signalCooldownStart)
        {
            signalCooldownStart = false;
            time = signalCooldown;
        }
        time -= Time.deltaTime;
        displayTime = Mathf.FloorToInt(time);
    }
}
