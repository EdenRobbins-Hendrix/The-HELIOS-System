using UnityEngine;

public class MovementSteer : MonoBehaviour
{


    public float speed;
    // public float angle;
    public float rotationSpeed;
    public float rotationOffset; //This is so that I can determine what part of the animal should be facing the target
    public bool isWandering;
    public bool isHunting;
    public GameObject target;
    public Vector3 wanderSpot;

    private Rigidbody2D body;

    // Use this for initialization
    void Start()
    {
        isWandering = true;
        isHunting = false;
        body = GetComponent<Rigidbody2D>();
        InvokeRepeating("pickSpot", 0.5f, 5.0f);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
    }
    void pickSpot()
    {

        int radius = 5;
        Vector2 circle = Random.insideUnitCircle * radius;
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