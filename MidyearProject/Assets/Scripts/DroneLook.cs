using UnityEngine;

public class DroneMouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public bool canLook = false;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Pitch (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply pitch to camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw (left/right) rotates the drone body
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
