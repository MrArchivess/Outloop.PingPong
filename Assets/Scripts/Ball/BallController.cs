using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float force;
    [SerializeField] private float maxVelocity = 10f;
    private bool isServed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {    
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Serve(Vector3.up, force);
        }

        if (Input.GetKeyDown(KeyCode.R))
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
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        isServed = false;
    }
}
