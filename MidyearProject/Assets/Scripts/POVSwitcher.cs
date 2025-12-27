using UnityEngine;

public class POVSwitchController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera playerCamera;
    public Camera droneCamera;

    [Header("Controllers")]
    public PlayerMovement playerController;
    public DroneMovement droneController;

    private bool isDroneView = false;

    void Start()
    {
        SetPlayerView();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isDroneView)
                SetPlayerView();
            else
                SetDroneView();
        }
    }

    void SetPlayerView()
    {
        isDroneView = false;

        playerCamera.enabled = true;
        droneCamera.enabled = false;

        playerController.canControl = true;
        droneController.canControl = false;
    }

    void SetDroneView()
    {
        isDroneView = true;

        playerCamera.enabled = false;
        droneCamera.enabled = true;

        playerController.canControl = false;
        droneController.canControl = true;
    }
}
