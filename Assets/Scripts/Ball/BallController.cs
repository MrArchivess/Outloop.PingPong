using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float force;
    [SerializeField] private float maxVelocity = 10f;

    private IBounceStrategy currentBounceStrategy;

    private Rigidbody rb;
    private bool isServed = false;
    private Vector3 originalPosition;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;
    }

    private void Update()
    {    
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Serve(Vector3.up, force);
        }

        if (Input.GetButtonDown("Reset"))
        {
            Reset();
        }
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
    }

    public void Serve(Vector3 direction, float speed)
    {
        if (isServed) return;

        rb.useGravity = true;
        rb.AddForce(direction * speed);
        isServed = true;
    }

    public void Reset()
    {
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        isServed = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( collision.gameObject.tag == "Table")
        {
            currentBounceStrategy = new DefaultBounceStrategy();
            Vector3 bounceDirection = currentBounceStrategy.GetBounceDirection(collision, rb.velocity);
            Debug.Log("Bounced off table!");
        }
    }
}
