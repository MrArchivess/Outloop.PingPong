using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    private bool inRange = false;
    private Rigidbody ballRb;
    [SerializeField] private IBallHitStrategy hitStrategy;

    private void Awake()
    {
        if (hitStrategy == null) hitStrategy = new SimpleHitStrategy();
        ballRb = GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (inRange && Input.GetButtonDown("Hit"))
        {
            hitStrategy.ApplyHit(ballRb, gameObject.GetComponent<Transform>());
            if (!ballRb.useGravity) ballRb.useGravity = true;
            inRange = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.CompareTag("Ball"))
        inRange = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
            inRange = false;
    }

}
