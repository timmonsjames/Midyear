using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public Camera mainCam;
    public Camera playerCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam.enabled = true;
        playerCam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mainCam.enabled = !mainCam.enabled;
            playerCam.enabled = !playerCam.enabled;
        }
    }
}
