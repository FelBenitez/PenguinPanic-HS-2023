using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition; //accesses the position of the CameraPos gameObject inside the Player

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position; //makes the position of the camera whatever the position of cameraPosition is
    }
}
