using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PickUpController : MonoBehaviour
{

    //make sure that every gun has this script with specific variables for that gun alongside with colliders and RB
    public FireBullet gunScript; //handles whether you can fire bullets or not
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, gunContainer, fpsCam;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    public bool equipped; //checks if the weapon is equipped
    public static bool slotFull; //checks if player is already carrying weapon

    //will handle the UI's depending on whats equipped
    public Image bullbackDisplayer;
    public TextMeshProUGUI bullCountDisplayer;
    public RawImage fishDisplayer;

    private void Start() 
    {
        //will not turn the bullet count on until a weapon is picked up
        bullbackDisplayer.enabled = false;
        bullCountDisplayer.enabled = false;
        fishDisplayer.enabled = false;

        if(!equipped )
        {
            gunScript.enabled = false; //cant fire bullets
            rb.isKinematic = false; //making something not kinematic means that forces or collisions will affect the rb
            coll.isTrigger = false; //so it doesnt fall through the floor, so the collider is active
        }
        if(equipped)
        {
            gunScript.enabled = true; //lets you fire bullets
            rb.isKinematic = true; //so force doesn't affect the movement of it
            coll.isTrigger = true; //so the gun doesnt collide with things
            slotFull = true;
        }
    }

    private void Update()
    {
        //check if player is in range and "E" is pressed
        Vector3 distanceToPlayer = player.position - transform.position;
        if(!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.E) && !slotFull) PickUp();

        //drop if equipped and "Q" is pressed
        if(equipped && Input.GetKeyDown(KeyCode.Q)) Drop();

        if(!equipped && (slotFull == false))
        {
            bullbackDisplayer.enabled = false;
            bullCountDisplayer.enabled = false;
            fishDisplayer.enabled = false;
        }

        if(equipped && slotFull)
        {
            bullbackDisplayer.enabled = true;
            bullCountDisplayer.enabled = true;
            fishDisplayer.enabled = true;
        }
    }

    private void PickUp()
    {
        //makes sure you can't pick up any weapons
        equipped = true;
        slotFull = true;

        //make weapon a child of the camera and move it to default position
        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one; 

        //Make RigidBody kinematic and BoxCollider a trigger
        rb.isKinematic = true; //making something kinematic means that forces or collisions will not affect the rb anymore
        coll.isTrigger = true;

        //Enable script
        gunScript.enabled = true;
    }

    private void Drop()
    {
        //makes sure you can pick up any weapons
        equipped = false;
        slotFull = false;

        //set parent to null
        transform.SetParent(null);

        //Make RigidBody not kinematic and BoxCollider normal
        rb.isKinematic = false;
        coll.isTrigger = false;

        //gun carries momentum of player
        rb.velocity = player.GetComponent<Rigidbody>().velocity;

        //Add forces. uses fps cam because depending on where you are looking, thats where it will throw it
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);

        //add a random rotation
        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10);

        //disable script
        gunScript.enabled = false;
    }
}
