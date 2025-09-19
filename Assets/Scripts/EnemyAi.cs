using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public GameObject bulletPrefab;
    public Transform AiShootingPoint; //this will allow you to access the position of the AiShootingPoint

    public Transform player;

    public LayerMask whatIsGround, whatisPlayer;
    
    public float health = 100;

    //Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange; //check if the player is in range to attack

    public PlayerMovement checkPlayerScript; //will use this to access PlayerMovement script to check the nearest checkpoint

    public GameObject penguinPrefab;

    private void Start()
    {
      GameObject g = GameObject.Find("Player");
      checkPlayerScript = g.GetComponent<PlayerMovement>();  
       //public PlayerMovement checkPlayerScript;

    }

    private void Awake() { //awake initializes any variables before the game begins
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update() 
    {
        //check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatisPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatisPlayer);


        //deals with what the enemy will do throughout the game
        if(!playerInSightRange && !playerInAttackRange) //if the player isnt in the sight and the attack range, it'll walk around
        Patrolling();

        if(playerInSightRange && !playerInAttackRange)
        ChasePlayer();

        if(playerInSightRange && playerInAttackRange)
        AttackPlayer();


        //Debug.Log("the value is: "+checkPlayerScript.exactCheckpoint[0]); //this returns what is in the array
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //working on trying to make enemy take damage when bullet hits it
    /*
    void OnCollisionEnter(Collision collision)
    {   
        
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "Bullet(Clone)")
        {
            //If the GameObject's name matches the one you suggest, output this message in the console
            Debug.Log("you have been hit by a bullet");
        }

        
        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.tag == "MyGameObjectTag")
        {
            //If the GameObject has the same tag as specified, output this message in the console
            Debug.Log("Do something else here");
        }
    }
    */
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Patrolling()
    {
        if(!walkPointSet)
        SearchWalkPoint();

        if(walkPointSet)
        agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached
        if(distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //calculate random point in the range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if(Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //make sure the enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);
       // transform.Rotate(0, 180, 0);

        if(!alreadyAttacked)
        {
            //ALL OF ATTACK CODE HERE   
            
            //the problem rn is that the bullet is coming in contact with the Ai immediately
            //change the position so it doesnt come from the inside of the player
            Rigidbody rb = Instantiate(bulletPrefab, AiShootingPoint.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            
            //turned isTrigger of because of bug with it automatically being on, and not being able to detect collisions
            Collider cb = rb.GetComponent<Collider>();
            cb.isTrigger = true;

            rb.AddForce(transform.forward *32f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    //call this function when bullet comes in contact with it so it takes damage
    public void TakeDamage(int damage)
    {
        health -= damage;

        if(health <=0)
        {
        //health = 100;
        DestroyEnemy();
        //Invoke(nameof(DestroyEnemy), .5f); //will kill the Ai if health == 0    
        }
        
    }

    private void DestroyEnemy()
    {
        //instead of destroying, set health back to 100 and reposition to the closest respawn position by player to "respawn"
        //i did it like this for quantity control
        //Destroy(gameObject);
        health = 100;
        
        //check the most recent checkpoint
        if(checkPlayerScript.exactCheckpoint[0] == 0)
        {
            //if the players checkpoint was most recently 0 then reposition at first respawn point to not have to create a new prefab
            transform.position = GameObject.Find("Respawn0").transform.position;

            Vector3 respawn1point = GameObject.Find("Respawn0").transform.position;
            Vector3 respawn00point = GameObject.Find("Respawn00").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn1point, rotation);
            GameObject penguinduplicate2 = Instantiate(penguinPrefab, respawn00point, rotation);
        }
        
        if(checkPlayerScript.exactCheckpoint[0] == 1)
        {
            transform.position = GameObject.Find("Respawn1").transform.position;

            Vector3 respawn00point = GameObject.Find("Respawn00").transform.position;
            Vector3 respawn12point = GameObject.Find("Respawn12").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn00point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn12point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 2)
        {
            transform.position = GameObject.Find("Respawn2").transform.position;

            Vector3 respawn1point = GameObject.Find("Respawn0").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn1point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 3)
        {
            transform.position = GameObject.Find("Respawn2").transform.position;

            Vector3 respawn3point = GameObject.Find("Respawn3").transform.position;
            Vector3 respawn4point = GameObject.Find("Respawn4").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn3point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn4point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 4)
        {
            transform.position = GameObject.Find("Respawn1").transform.position;

            Vector3 respawn2point = GameObject.Find("Respawn2").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn2point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 5)
        {
            transform.position = GameObject.Find("Respawn1").transform.position;

            Vector3 respawn13point = GameObject.Find("Respawn13").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn13point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 6)
        {
            transform.position = GameObject.Find("Respawn8").transform.position;

            Vector3 respawn9point = GameObject.Find("Respawn9").transform.position;
            Vector3 respawn10point = GameObject.Find("Respawn10").transform.position;
            Vector3 respawn11point = GameObject.Find("Respawn11").transform.position;
            Vector3 respawn13point = GameObject.Find("Respawn13").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn9point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn10point, rotation);
            GameObject penguinduplicate2 = Instantiate(penguinPrefab, respawn11point, rotation);
            GameObject penguinduplicate3 = Instantiate(penguinPrefab, respawn13point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 7)
        {
            transform.position = GameObject.Find("Respawn5").transform.position;

            Vector3 respawn6point = GameObject.Find("Respawn6").transform.position;
            Vector3 respawn9point = GameObject.Find("Respawn9").transform.position;
            Vector3 respawn10point = GameObject.Find("Respawn10").transform.position;
            Vector3 respawn11point = GameObject.Find("Respawn11").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate3 = Instantiate(penguinPrefab, respawn6point, rotation);
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn9point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn10point, rotation);
            GameObject penguinduplicate2 = Instantiate(penguinPrefab, respawn11point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 9)
        {
            transform.position = GameObject.Find("Respawn4").transform.position;

            Vector3 respawn3point = GameObject.Find("Respawn3").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn3point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 10)
        {
            transform.position = GameObject.Find("Respawn5").transform.position;

            Vector3 respawn6point = GameObject.Find("Respawn6").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn6point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 11)
        {
            transform.position = GameObject.Find("Respawn5").transform.position;

            Vector3 respawn6point = GameObject.Find("Respawn6").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn6point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 12)
        {
            transform.position = GameObject.Find("Respawn4").transform.position;

            Vector3 respawn3point = GameObject.Find("Respawn3").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn3point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 13)
        {
            transform.position = GameObject.Find("Respawn5").transform.position;

            Vector3 respawn6point = GameObject.Find("Respawn6").transform.position;
            Vector3 respawn14point = GameObject.Find("Respawn14").transform.position;
            Vector3 respawn15point = GameObject.Find("Respawn15").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn6point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn14point, rotation);
            GameObject penguinduplicate2 = Instantiate(penguinPrefab, respawn15point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 14)
        {
            transform.position = GameObject.Find("Respawn5").transform.position;

            Vector3 respawn6point = GameObject.Find("Respawn6").transform.position;
            Vector3 respawn7point = GameObject.Find("Respawn7").transform.position;
            Quaternion rotation = new Quaternion(1, 1, 1, 1);

            //makes a duplicate penguin to make it harder and to deal with more cases
            GameObject penguinduplicate = Instantiate(penguinPrefab, respawn6point, rotation);
            GameObject penguinduplicate1 = Instantiate(penguinPrefab, respawn7point, rotation);
        }

        if(checkPlayerScript.exactCheckpoint[0] == 15)
        {
            transform.position = GameObject.Find("Respawn2").transform.position;
        }


        
    }

    //will handle whenever a trigger enters the collider, so basically the bullet
    //you will do everything in here by finding what the name is and doing stuff based on that

    //each bullet will do 19 damage

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Name of the object that hit Ai player: " + other.gameObject.name);

        //if the bullet is what comes in contact with the Ai
        //if//(other.gameObject.name == "Bullet(Clone)")
        if(other.gameObject.name == "SardineSkinWithDemoScript(Clone)")
        {
        //Debug.Log("it detected the bullet hit");  
        TakeDamage(39); //makes the Ai take 39 damage
        Destroy(other.gameObject); //destroys the bullet once it hits
        }
        
    }
}
