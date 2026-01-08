using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeDrone : MonoBehaviour
{
    public GameObject drone;

    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        transform.position = drone.transform.position;
    }
}
