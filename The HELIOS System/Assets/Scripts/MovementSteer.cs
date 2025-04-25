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
        InvokeRepeating("pickSpot", 0.5f, 5.0f);

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
            Debug.Log(GameManager.Instance);
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

        int radius = 5;
        Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
        wanderSpot = new Vector2(circle.x, circle.y);
        // Debug.Log("Target spot: " + target);
    }
    void moveToSpot(Vector3 target)
    {

        if (Vector3.Distance(target, transform.position) < 0.01)
        {
            //Do nothing
            Debug.Log("Within acceptable distance!");
            Vector2 stop = new Vector2(0, 0);
            body.linearVelocity = stop; //this stops the animal cold
        }
        else
        {
            //turn to target
            Vector2 desired = target - transform.position;
            // Debug.Log("Desired: " + desired);
            float angle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg - rotationOffset;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                q, Time.deltaTime * rotationSpeed);

            //move to point
            body.AddForce(desired.normalized *
                  speed - body.linearVelocity);
        }

    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(name + " collided with " + collision.gameObject.name);
        GameObject collisionObject = collision.gameObject;
        if (isHunting)
        {
            GameManager.Instance.attemptFeed(gameObject, collisionObject, 5.0f);
        }
    }


}