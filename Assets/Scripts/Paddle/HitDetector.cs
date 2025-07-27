using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    private bool inRange = false;
    [SerializeField] private IBallHitStrategy hitStrategy;

    private void Awake()
    {
        if (hitStrategy == null) hitStrategy = new SimpleHitStrategy();
    }

    private void Update()
    {
        if (inRange && Input.GetButtonDown("Hit"))
        {
            //HitStrategy
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
