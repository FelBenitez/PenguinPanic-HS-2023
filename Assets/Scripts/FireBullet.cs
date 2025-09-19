using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireBullet : MonoBehaviour
{
    public GameObject bulletPrefab; //this will handle the bullet prefab and sets reference to it

    public float shootForce, upwardForce; //handles all bullet force

    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots; //the stats for the gun
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;

    //References
    public Camera fpsCam;
    public Transform attackPoint;

    public Rigidbody rbdoor; //connects to the doors rigidbody without calling it from script


    //Graphics
    //public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;
    //public Canvas bullDisplayer;
    //public PickUpController puController;  
    public ParticleSystem muzzleFlash;

    //bug fixing
    public bool allowInvoke = true;

    private void Awake() {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        myInput();

        //Set ammo display, if it exists
        if(ammunitionDisplay != null)
        ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " +magazineSize / bulletsPerTap); //changes the text by itself even when canvas is hidden

        /*
        if(puController.equipped == true)
        {
            bullDisplayer.setActive(false);
        }
        */
    }

    private void myInput()
    {
        //check if you are allowed to hold down button
        if(allowButtonHold)
        shooting = Input.GetKey(KeyCode.Mouse0);
        else
        shooting = Input.GetKeyDown(KeyCode.Mouse0); //so you have to tap button every time you want to shoot

        //Reloading
        if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        Reload();

        //Reload automatically when trying to shoot without ammo
        if(readyToShoot && shooting & !reloading && bulletsLeft <=0)
        Reload();


        //shooting
        if(readyToShoot && shooting && !reloading && bulletsLeft >0)
        {
            //set bullets shot to 0
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Find the exact hit position using a raycast
        //a Ray is a data struct in Unity that represents a point of origin and a direction for the Ray to travel
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f)); //creates a ray that points to the middle of screen
        RaycastHit hit; //gets info back from raycast

        //check if ray hits something
        Vector3 targetPoint;
        if(Physics.Raycast(ray, out hit))
        targetPoint = hit.point;
        else
        targetPoint = ray.GetPoint(75); //just a point far away from the character

        //Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculate new direction with spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x,y,0);

        //creates the bullet
        GameObject currentBullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);

        //rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        //currentBullet.GetComponent<SphereCollider>(); //adds a collider to the bullet so it doesn't go straight through
        //currentBullet.gameObject.tag="BulletHit";

        Collider cb = currentBullet.GetComponent<Collider>();
        cb.isTrigger = true;

        //Add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized  * shootForce, ForceMode.Impulse);
        muzzleFlash.Play();
        //currentBullet.GetComponent<RigidBody>().AddForce(fpsCam.transform.up  * upwardForce, ForceMode.Impulse); //this is only for things like grenades
        //you dont want to add upwards force to things like bullets

        //instantiate muzzle flash if you have one
        //if(muzzleFlash != null)
        //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);


        bulletsLeft--;
        bulletsShot++;

        //Invoke resetShot function (if not already invoked)
        if(allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        //if more than one bulletsPerTap make sure to repeat shoot function, like shotgun
        if(bulletsShot<bulletsPerTap && bulletsLeft > 0)
        Invoke("Shoot", timeBetweenShots);


    }

    /*
    private void OnCollisionEnter (Collision collision) 
    {
    if (collision.gameObject.tag == "Untagged")
    {
            Destroy(gameObject);
    }
    }      
    */

    //will handle whenever a trigger enters the collider, so basically the bullet
    //you will do everything in here by finding what the name is and doing stuff based on that
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Name of the object: " + other.gameObject.name);

        //if the gun comes in contact with the door, so that you can only leave the room if you have your gun
        if(other.gameObject.name == "Door")
        {
        //makes it so that the door can be impacted by forces
        rbdoor.isKinematic = false;    
        rbdoor.AddForce(transform.forward * 10, ForceMode.Impulse);//adds force forwards
        //other.isTrigger = true;
        }
        
    }
    
    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
