using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //locks the mouse pointer to the center of the screen
        Cursor.visible = false; //makes the cursor invisible
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX; //gets the position of the mouse on the X axis
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensX; //gets the position of the mouse on the Y axis

        //handles the rotation
        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //clamps it so the up and down cant be more than 90 degrees
        
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); //rotates the actual body of the player
    }
}
