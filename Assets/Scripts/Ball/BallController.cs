using System;
using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static event Action OnBallReset;

    [SerializeField] private float serveForce;
    [SerializeField] private float maxVelocity = 10f;

    private IBounceStrategy currentBounceStrategy;

    private Rigidbody rb;
    public bool IsServed => isServed;
    private bool isServed = false;

    [SerializeField] private PaddleController[] paddles = new PaddleController[2]; 
    

    private void Start()
    {
        paddles = GameObject.FindObjectsOfType<PaddleController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {    
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Serve();
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

    private void SetPaddles()
    {
        paddles = new PaddleController[2];
        paddles = FindObjectsOfType<PaddleController>();
    }

    private void Serve()
    {
        if (isServed) return;

        rb.useGravity = true;
        rb.AddForce(Vector3.up * serveForce);
        isServed = true;
    }

    private void Reset()
    {
        OnBallReset?.Invoke();
    }

    public void PrepareForServe(Vector3 position)
    {
        StartCoroutine(SetServePosition(position));
    }

    private IEnumerator SetServePosition(Vector3 position)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        yield return new WaitForFixedUpdate();

        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        transform.position = position;
        transform.rotation = Quaternion.identity;
        isServed = false;

        if (col != null) yield return null;
        if (col != null) col.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( collision.gameObject.tag == "Table")
        {
            currentBounceStrategy = new DefaultBounceStrategy();
            Vector3 bounceDirection = currentBounceStrategy.GetBounceDirection(collision, rb.velocity);

        }
    }

    private void OnEnable()
    {
        HitDetector.OnServeStarted += Serve;
    }

    private void OnDisable()
    {
        HitDetector.OnServeStarted -= Serve;
    }
}
