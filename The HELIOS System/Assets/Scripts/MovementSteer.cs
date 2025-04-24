using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class MovementSteer : MonoBehaviour
{


    public float speed;
    // public float angle;
    public float rotationSpeed;
    public float rotationOffset; //This is so that I can determine what part of the animal should be facing the target
    public bool isWandering;
    public bool isHunting;
    public bool isAvoidingPredator;
    public float minDistFromPredator;
    [SerializeField]
    private bool isSpriteFlipped;
    private SpriteRenderer spriteRenderer;
    public GameObject target;
    public Vector3 wanderSpot;

    private Rigidbody2D body;
    private CircleCollider2D detectionCircle;
    private List<GameObject> predatorsInRange;
    public List<String> potentialPredatorNames;

    // Use this for initialization
    void Start()
    {
        predatorsInRange = new List<GameObject> { };
        isWandering = true;
        isHunting = false;
        body = GetComponent<Rigidbody2D>();
        detectionCircle = GetComponent<CircleCollider2D>();
        detectionCircle.radius = minDistFromPredator;
        isSpriteFlipped = false;
        spriteRenderer = GetComponent<SpriteRenderer>();

        /* 
            So there is a fascinating problem: 
            I set the squirrel object to kinematic, so that way we could just ignore all those collisions and they wouldn't just mob up
            However, they all picked the same spot, eventually becoming the "mega squirrel". 
            The Internet says that this is because the pick spot is called at the same interval for every prefab, so that takes away the 
            "randomness" and they sync up. 
        */
        float startDelay = 0.5f + UnityEngine.Random.value;
        float repeatRate = 4.5f + UnityEngine.Random.Range(0f, 1f);
        InvokeRepeating("pickSpot", startDelay, repeatRate);


    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (potentialPredatorNames.Contains(collision.name))

            foreach (String predatorName in potentialPredatorNames)
            {
                if (collision.name.Contains(predatorName))
                {
                    predatorsInRange.Add(collision.gameObject);

                }
            }
        if (isHunting)
        {
            Debug.Log(name + " collided with " + collision.gameObject.name);
            GameObject collisionObject = collision.gameObject;
            if (isHunting)
            {
                GameManager.Instance.attemptFeed(gameObject, collisionObject, 5.0f);
            }

        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (predatorsInRange.Contains(collision.gameObject))
        {
            predatorsInRange.Remove(collision.gameObject);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Before I do anything, I need to search for any predators nearby. 

        // Search for closest potential predator
        GameObject closestPredator = null;
        if (predatorsInRange.Count > 0)
        {
            // Debug.Log("Predators are in range");
            float smallestDistance = float.PositiveInfinity;
            GameObject smallest = predatorsInRange.First();
            foreach (GameObject pred in predatorsInRange)
            {
                float distance = Vector3.Distance(transform.position, pred.transform.position);
                if (distance < smallestDistance)
                {
                    smallest = pred;
                    smallestDistance = distance;
                }
            }
            closestPredator = smallest;
        }
        else
        {
            closestPredator = null;
        }



        // if there is a predator, turn of wandering and hunting and instead avoid predator
        if (closestPredator != null)
        {
            // Debug.Log("Avoiding Predator is set to true");
            isWandering = false;
            isHunting = false;
            isAvoidingPredator = true;
        }
        else
        {
            isAvoidingPredator = false;
            // Debug.Log(GameManager.Instance);
            GameManager.Instance.setOrganismBehavior(gameObject);
        }

        if (isWandering)
        {
            // Debug.Log("Wandering");
            moveToSpot(wanderSpot);
        }
        else if (isHunting)
        {
            if (target == null)
            {
                GameManager.Instance.setOrganismBehavior(gameObject);
                Vector2 stop = new Vector2(0, 0);
                body.linearVelocity = stop; //this stops the animal cold

            }
            else
            {
                // Debug.Log("Hunting");
                moveToSpot(target.transform.position);
            }
        }
        else if (isAvoidingPredator)
        {
            // Debug.Log("Avoiding predator behavior reached");

            // // Debug.Log(name + "target is true?");
            Vector2 desired = transform.position - closestPredator.transform.position;
            // Debug.Log("Desired Location: " + desired);
            // Debug.Log(name + ": " + desired);
            // Debug.Log(name + "normalized: " + desired.normalized);
            // // Debug.Log(name + "Desired: " + desired);
            // // Debug.Log(name + "Magnitude: " + desired.magnitude);
            // // Debug.Log(name + "Position: " + target.transform.position);



            if (desired.magnitude < minDistFromPredator)
            {
                // Debug.Log("Reached Desired.magnitude thing");
                moveToSpot(desired);
            }

        }
    }
    void pickSpot()
    {

        int radius = 10;
        Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
        wanderSpot = new Vector2(circle.x, circle.y);
        // Debug.Log("Target spot: " + target);
    }
    void moveToSpot(Vector3 target)
    {

        if (Vector3.Distance(target, transform.position) < 0.01)
        {
            //Do nothing
            // Debug.Log("Within acceptable distance!");
            Vector2 stop = new Vector2(0, 0);
            body.linearVelocity = stop; //this stops the animal cold
        }
        else
        {


            // get desired location
            Vector2 desired = (target - transform.position);

            // flip sprite accordingly
            spriteRenderer.flipX = desired.x > 0;
            isSpriteFlipped = spriteRenderer.flipX;


            // get angle for rotation correct
            float angle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
            if (isSpriteFlipped) // going right? 
            {
                if (desired.y > 0)
                {    //going up? 

                }
                else
                { //going down? 

                }


            }
            else // going left? 
            {
                // angle = angle + 180;

                if (desired.y > 0)
                { //going up? 
                    angle = angle - 150;

                }
                else
                { //going down? 
                    angle = angle + 150;
                }

            }
            // Debug.Log("Angle: " + angle);

            // Actually do the rotation
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                    q, Time.deltaTime * rotationSpeed);

            // Move towards desired spot. This can be replaced with rigid body thing if need be
            transform.position = Vector2.MoveTowards(transform.position, desired, speed * Time.deltaTime);

            //     body.AddForce(desired.normalized *
            //           speed - body.linearVelocity);



            //     float turningOffset = 0.0f;
            //     bool rotationBoolean = false;

            //     // flip the sprite if appropriate
            //     if (desired.x < 0)
            //     {
            //         Debug.Log("less than");
            //         isSpriteFlipped = false;
            //         spriteRenderer.flipX = false;
            //     }
            //     else
            //     {
            //         isSpriteFlipped = true;
            //         spriteRenderer.flipX = true;
            //     }



            //     // Debug.Log("Desired: " + desired);
            //     float angle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg - rotationOffset;
            // Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            // transform.rotation = Quaternion.Slerp(transform.rotation,
            //     q, Time.deltaTime * rotationSpeed);


            //     //move to point
            //     body.AddForce(desired.normalized *
            //           speed - body.linearVelocity);
        }

    }


    // This doesn't really work now that the game objects are kinematic

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     Debug.Log(name + " collided with " + collision.gameObject.name);
    //     GameObject collisionObject = collision.gameObject;
    //     if (isHunting)
    //     {
    //         GameManager.Instance.attemptFeed(gameObject, collisionObject, 5.0f);
    //     }
    // }



}