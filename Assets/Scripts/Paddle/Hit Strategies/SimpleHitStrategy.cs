using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHitStrategy : MonoBehaviour, IBallHitStrategy
{
    [SerializeField] private float hitForce = 10f;

    public void ApplyHit(Rigidbody ballRb, Transform paddleTransform)
    {
        Vector3 direction = paddleTransform.forward;
        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(direction * hitForce, ForceMode.Impulse);
    }
}