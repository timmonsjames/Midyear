using UnityEngine;

public class POVmanager : MonoBehaviour
{
    public GameObject player;
    public GameObject drone;

    public Camera playerCamera;
    public Camera droneCamera;

    private bool isDroneActive = false;

    void Start()
    {
        SwitchToPlayer();
    }

    public void TogglePOV()
    {
        if (isDroneActive)
            SwitchToPlayer();
        else
            SwitchToDrone();
    }

    void SwitchToPlayer()
    {
        isDroneActive = false;

        player.SetActive(true);
        drone.SetActive(false);

        playerCamera.enabled = true;
        droneCamera.enabled = false;
    }

    void SwitchToDrone()
    {
        isDroneActive = true;

        player.SetActive(false);
        drone.SetActive(true);

        playerCamera.enabled = false;
        droneCamera.enabled = true;
    }
}
