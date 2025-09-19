using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;
    public KeyCode jumpKey = KeyCode.Space;

    public float playerHeight; //checks the height of the player and if you are on the ground
    //this is to apply drag, because drag in the air feels weird, so you want to apply when it is on the ground
    public LayerMask whatIsGround; //make sure to apply the LayerMask in the editor to the plane and to the script values if you make a change
    bool grounded;

    public float health = 100;

    public HealthBar modifyHealthBar; //creates object of type HealthBar, to be able to modify and access things from the script
                                      //I attached the script GameObject with the script onto this to be able to access it


    public Rigidbody rbenddoor; //connects to the end door rigidbody without calling it from script

    public int[] exactCheckpoint = new int[1]; //this will tell you the exact checkpoint you just passed to access it in the enemy script
    /*
    Move speed is 7
    Ground Drag is 5
    Player height is 2

    Jump Force is 12
    Jump CoolDown is 0.25 (change if you need to make it so you need a longer cooldown later)
    Air Multiplier is 0.4
    i set all this from the editor
    */


    /*


    Right now the player falls through plane, figure out why, might be cuz i changed the size of the plane and sum mightve switched off

    */

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    public TextMeshProUGUI countdownDisplay;
    float timeLeft = 91.0f; //original is 91

    public Camera firstPersonCamera;
    public Camera overheadCamera;
    public Canvas CanvasObject;

    public CanvasGroup myUIGroup;
    bool fadeIn = false;
    bool fadeOut = false;


    //public GameObject bulletPrefab; //this will handle the bullet prefab and sets reference to it
    //public RigidBody projectile;
    //public float speed = 10f;


    void Start()
    {   
        HideCanvas();
        Invoke(nameof(ShowCanvas), 0.51f);
        Invoke(nameof(HideFade), 0.5f);
        exactCheckpoint[0] = 0; //this will mean that you are still in the main room
        rb = GetComponent<Rigidbody>(); //accesses the RigidBody like OOP to make it usable
        rb.freezeRotation = true; //so player doesn't tip over due to physics
    }

    // Update is called once per frame
    void Update()
    {
        //check if it is touching the ground to apply drag
        //Raycast works as (Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        //Raycast returns true if the ray intersects with a Collider, otherwise false.
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        myInput();
        speedControl();
        
        //will handle and apply the drag
        if(grounded) //if the player is on the ground, it will apply drag
        rb.drag = groundDrag;
        else //else, it adds no drag so it doesn't feel weird applying drag while jumping
        rb.drag = 0;

        timeLeft -= Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeLeft);
        string timeText = string.Format("{1:D1}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        //countdownDisplay.SetText("" + timeLeft);
        countdownDisplay.SetText(timeText);

        //does the fading in to black
        if(fadeIn)
        {
            if(myUIGroup.alpha < 1)
            {
                myUIGroup.alpha += Time.deltaTime * 0.7f; //increases the visibility of the black screen
                if(myUIGroup.alpha>=1)
                {
                    fadeIn = false;
                }
            }
        }

        if(fadeOut)
        {
            if(myUIGroup.alpha >=0)
            {
                myUIGroup.alpha -= Time.deltaTime; //decreases the visibility of the black screen
                if(myUIGroup.alpha==0)
                {
                    fadeOut = false;
                }
            }
        }

        if(timeLeft<=0)
        {
            ranOutOfTime();
        }
    }

    private void FixedUpdate() 
    {
        movePlayer();
    }

    //takes keyboard input
    public void myInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Debug.Log("it pressed it");
            readyToJump = false; //so you can't double jump when the jump function runs
            jump();

            Invoke(nameof(resetJump), jumpCooldown); //makes it so that you are able to jump again after a certain amount of time

        }

        /*
        if(Input.GetMouseButtonDown(0))
        {
            RigidBody instantiatedprojectile = Instantiate(projectile, transform.position, transform.rotation as RigidBody);
            instantiatedprojectile.velocity = transform.TransformDirection(new Vector3(0,0,speed));
            //GameObject bulletObject = Instantiate(bulletPrefab);
            //bulletObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
            //bulletObject.transform.position = MoveCamera.cameraPosition.transform.position + MoveCamera.cameraPosition.transform.forward;
        }
        */
    }

    //moves the player
    public void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded)//if you are on the ground
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if(!grounded)
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    //function to make sure the movement doesn't turn too fast
    private void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        //.magnitude returns float to express vector length, loses direction
        if(flatVel.magnitude > moveSpeed) //if the speed is quicker than the move speed
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed; //makes the Vector3 the direction times the max speed
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z); //applies it to the physics rb
        }
    }

    //what makes the player jump
    private void jump()
    {
        //reset the Y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    //to make sure you cant double jump
    private void resetJump()
    {
        readyToJump = true;
    }

    //call this function when bullet comes in contact with it so it takes damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        modifyHealthBar.healther = health;

        if(health <=0)
        Invoke(nameof(DestroyItself), .5f); //will kill the Ai if health == 0
    }

    private void DestroyItself()
    {
        //plays explosion animations once the player dies, put this wherever you need the animation to come up at
        Vector3 changePlayerPointEnd = GameObject.Find("EndPlayerPos").transform.position;
        transform.position = changePlayerPointEnd;
        ParticleSystem explosionparticle = GameObject.Find("Exposion-[Explosion9]").GetComponent<ParticleSystem>();
        explosionparticle.Play();
        ParticleSystem ringparticle = GameObject.Find("Ring").GetComponent<ParticleSystem>();
        ringparticle.Play();

        CanvasObject.enabled = false;
        firstPersonCamera.enabled = false;
        overheadCamera.enabled = true;

        Invoke(nameof(ShowFade), 2.5f);
        
        /*
        Camera myCamera = Camera.main;
        Vector3 WhereCamRepos = GameObject.Find("WhereToReposCam").transform.position;
        myCamera.transform.position = WhereCamRepos;
        */
        
        //Destroy(gameObject);
    }

    //this now detects when the bullet comes in contact with the player
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Name of the object that hit main player: " + other.gameObject.name);

        //if the bullet is what comes in contact with the MainPlayer
        //if(other.gameObject.name == "Bullet(Clone)")
        if(other.gameObject.name == "SardineSkinWithDemoScript(Clone)")
        {
        //Debug.Log("it detected the bullet hit");  
        TakeDamage(5); //makes the MainPlayer take damage
        Destroy(other.gameObject); //destroys the bullet once it hits
        }

        

        //if the player comes in contact with each of the checkpoints, then it sets the nearest spawn point to whichever is nearest to checkpoint
        //you will do this through enemy script, once an enemy is "killed" since you will have to fight them and escape
        //the enemy will have its health set back to 100 and will reposition to the nearest respawn point to the checkpoint
        if(other.gameObject.name == "Checkpoint1")
        {
        exactCheckpoint[0] = 1; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint2")
        {
        exactCheckpoint[0] = 2; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint3")
        {
        exactCheckpoint[0] = 3; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint4")
        {
        exactCheckpoint[0] = 4; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint5")
        {
        exactCheckpoint[0] = 5; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint6")
        {
        exactCheckpoint[0] = 6; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint7")
        {
        exactCheckpoint[0] = 7; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint8")
        {
        exactCheckpoint[0] = 8; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint9")
        {
        exactCheckpoint[0] = 9; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint10")
        {
        exactCheckpoint[0] = 10; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint11")
        {
        exactCheckpoint[0] = 11; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint12")
        {
        exactCheckpoint[0] = 12; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint13")
        {
        exactCheckpoint[0] = 13; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint14")
        {
        exactCheckpoint[0] = 14; //you will access the array at position 0 later
        }

        if(other.gameObject.name == "Checkpoint15")
        {
        exactCheckpoint[0] = 15; //you will access the array at position 0 later
        }



        //if the player comes in contact with the end door
        /*
        if(other.gameObject.name == "ExitDoor")
        {
        //makes it so that the door can be impacted by forces
        rbenddoor.isKinematic = false;    
        rbenddoor.AddForce(transform.forward * 10, ForceMode.Impulse);//adds force forwards
        }
        */
    }

    void OnCollisionEnter(Collision collision)
    {   
        
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "ExitDoor")
        {
        rbenddoor.isKinematic = false;    
        rbenddoor.AddForce(transform.forward * 40, ForceMode.Impulse);//adds force forwards

        Invoke(nameof(escapedBuilding), 2.5f);
        }

        /*
        if(collision.gameObject.name == "Heal1")
        {
        health += damage;
        modifyHealthBar.healther += Time.deltaTime * 3f;
        }
        */
    }

    void OnCollisionStay(Collision collision) 
    {
        if(collision.gameObject.name == "Heal1")
        {
        health += 0.1f;
        modifyHealthBar.healther = health;
        }
    }

    //plays blowing up animations when you are out of time
    public void ranOutOfTime()
    {
        Vector3 changePlayerPointEnd = GameObject.Find("EndPlayerPos").transform.position;
        transform.position = changePlayerPointEnd;
        ParticleSystem explosionparticle = GameObject.Find("Exposion-[Explosion9]").GetComponent<ParticleSystem>();
        explosionparticle.Play();
        ParticleSystem ringparticle = GameObject.Find("Ring").GetComponent<ParticleSystem>();
        ringparticle.Play();

        CanvasObject.enabled = false;
        firstPersonCamera.enabled = false;
        overheadCamera.enabled = true;

        Invoke(nameof(ShowFade), 2.5f);
    }

    public void escapedBuilding()
    {
        Vector3 changePlayerPointEnd = GameObject.Find("EndPlayerPos").transform.position;
        transform.position = changePlayerPointEnd;
        ParticleSystem explosionparticle = GameObject.Find("Exposion-[Explosion9]").GetComponent<ParticleSystem>();
        explosionparticle.Play();
        ParticleSystem ringparticle = GameObject.Find("Ring").GetComponent<ParticleSystem>();
        ringparticle.Play();

        CanvasObject.enabled = false;
        firstPersonCamera.enabled = false;
        overheadCamera.enabled = true;

        Invoke(nameof(ShowFade), 2.5f);
    }

    public void ShowFade()
    {
        fadeIn = true;
    }

    public void HideFade()
    {
        fadeOut = true;
    }

    public void ShowCanvas()
    {
        CanvasObject.enabled = true;
    }

    public void HideCanvas()
    {
        CanvasObject.enabled = false;
    }
}
