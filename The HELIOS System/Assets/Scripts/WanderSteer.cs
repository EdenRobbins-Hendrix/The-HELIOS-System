using UnityEngine;

public class WanderSteer : MonoBehaviour
{


    public float speed;
    public float angle;
    public float rotationSpeed;
    public float rotationOffset; //This is so that I can determine what part of the animal should be facing the target

    private Rigidbody2D body;

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        InvokeRepeating("pickSpot", 0.5f, 5.0f);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveToSpot(target);
    }
    Vector3 target;
    void pickSpot()
    {
        int radius = 5;
        Vector2 circle = Random.insideUnitCircle * radius;
        target = new Vector2(circle.x, circle.y);
        Debug.Log("Target spot: " + target);
    }
    void moveToSpot(Vector3 target)
    {
        if (Vector3.Distance(target, transform.position) < 1.0)
        {
            //Do nothing
            Debug.Log("Within acceptable distance!");
            Vector2 stop = new Vector2(0, 0);
            body.linearVelocity = stop; //this stops the animal cold
        }
        else
        {
            //get random point

            //turn to point
            Vector2 desired = target - transform.position;
            Debug.Log("Desired: " + desired);


            float angle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg - rotationOffset;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                q, Time.deltaTime * rotationSpeed);
            //move to point
            body.AddForce(desired.normalized *
                  speed - body.linearVelocity);
        }
    }
}