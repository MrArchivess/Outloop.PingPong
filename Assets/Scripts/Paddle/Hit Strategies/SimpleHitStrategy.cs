using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHitStrategy : IBallHitStrategy
{
    [SerializeField] private float hitForce = 0.012f;
    [SerializeField] private float upwardLift = 0.48f;

    public void ApplyHit(Rigidbody ballRb, Transform paddleTransform, float charge, float inputDir)
    {
        float scaleFactor = paddleTransform.localScale.magnitude;
        float scaledForce = hitForce * scaleFactor;
        float strength = scaledForce * Mathf.Lerp(0.9f, 1f, charge);

        Vector3 forward = paddleTransform.forward;

        if (forward.z < 0f)
        {
            inputDir *= -1f;
        }

        Vector3 sideCurve = paddleTransform.right * inputDir * 0.5f;

        Vector3 finalDirection = (forward + sideCurve + Vector3.up * upwardLift).normalized;

        ballRb.velocity = Vector3.zero;
        ballRb.AddForce(finalDirection * strength, ForceMode.Impulse);
    }
}