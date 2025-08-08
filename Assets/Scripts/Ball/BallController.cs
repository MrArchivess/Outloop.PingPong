using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    public static event Action OnBallReset;

    [SerializeField] private float serveForce;
    [SerializeField] private float maxVelocity = 10f;

    private IBounceStrategy currentBounceStrategy;
    private Rigidbody rb;

    public PlayerSide PlayerWhoLastHit => playerWhoLastHit;
    private PlayerSide playerWhoLastHit;

    public bool RoundOverTriggered => roundOverTriggered;
    private bool roundOverTriggered = false;

    public bool IsMoveLegal => isMoveLegal;
    private bool isMoveLegal = false;

    public bool IsServed => isServed;
    private bool isServed = false;


    [SerializeField] private PaddleController[] paddles = new PaddleController[2];


    private void Start()
    {
        paddles = GameObject.FindObjectsOfType<PaddleController>();
        rb = GetComponent<Rigidbody>();
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
        playerWhoLastHit = GameManager.Instance.CurrentServer;
        isMoveLegal = false;
        isServed = false;
        roundOverTriggered = false;

        if (col != null) yield return null;
        if (col != null) col.enabled = true;
    }

    private void Serve()
    {
        if (isServed) return;

        rb.useGravity = true;
        rb.AddForce(Vector3.up * serveForce);
        isServed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Table")
        {
            currentBounceStrategy = new DefaultBounceStrategy();
            Vector3 bounceDirection = currentBounceStrategy.GetBounceDirection(collision, rb.velocity);
        }
    }

    private void SetRoundOverTriggered()
    {
        roundOverTriggered = true;
    }

    private void MakeMoveLegal()
    {
        isMoveLegal = true;
    }

    private void SetLastBallHitter(PlayerSide playerWhoLastHitBall)
    {
        playerWhoLastHit = playerWhoLastHitBall;
        isMoveLegal = false;
        
    }

    private void OnEnable()
    {
        HitDetector.OnServeStarted += Serve;
        HitDetector.OnBallHit += SetLastBallHitter;
        BoundsEventBus.OnRoundOver += SetRoundOverTriggered;
        TableSideBoundsDetector.legalMoveMade += MakeMoveLegal;
        GameManager.OnPlayerConnected += SetPaddles;

    
    }

    private void OnDisable()
    {
        HitDetector.OnServeStarted -= Serve;
        HitDetector.OnBallHit -= SetLastBallHitter;
        BoundsEventBus.OnRoundOver -= SetRoundOverTriggered;
        TableSideBoundsDetector.legalMoveMade -= MakeMoveLegal;
        GameManager.OnPlayerConnected -= SetPaddles;
    }
}
