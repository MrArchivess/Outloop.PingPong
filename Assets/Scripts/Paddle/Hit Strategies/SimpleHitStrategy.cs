using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHitStrategy : MonoBehaviour, IBallHitStrategy
{
    [SerializeField] private float hitForce = 0.02f;
    [SerializeField] private float upwardLift = 0.16f;

    public void ApplyHit(Rigidbody ballRb, Transform paddleTransform)
    {
        Vector3 direction = (Vector3.forward + Vector3.up * upwardLift).normalized;

        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(direction * hitForce, ForceMode.Impulse);
    }
}