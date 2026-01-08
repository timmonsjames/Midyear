using UnityEngine;

public class POVmanager : MonoBehaviour
{
    public GameObject player;
    public GameObject drone;
    public GameObject human;
    public GameObject fakeDrone;

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
        human.SetActive(false);
        fakeDrone.SetActive(true);

        playerCamera.enabled = true;
        droneCamera.enabled = false;
    }

    void SwitchToDrone()
    {
        isDroneActive = true;

        player.SetActive(false);
        drone.SetActive(true);
        human.SetActive(true);
        fakeDrone.SetActive(false);

        playerCamera.enabled = false;
        droneCamera.enabled = true;
    }
}
