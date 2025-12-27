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
    public DroneMouseLook droneMouseLook;

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
        playerCamera.enabled = true;
        droneCamera.enabled = false;

        playerController.canControl = true;
        droneController.canControl = false;

        droneMouseLook.canLook = false;
    }

    void SetDroneView()
    {
        playerCamera.enabled = false;
        droneCamera.enabled = true;

        playerController.canControl = false;
        droneController.canControl = true;

        droneMouseLook.canLook = true;
    }

}
