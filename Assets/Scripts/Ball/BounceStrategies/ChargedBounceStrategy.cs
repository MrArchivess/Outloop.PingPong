using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedBounceStrategy : IBounceStrategy
{
    public Vector3 GetBounceDirection(Collision collision, Vector3 incomingVelocity)
    {
        Vector3 normal = collision.contacts[0].normal;
        Vector3 bounceDir = Vector3.Reflect(incomingVelocity, normal);
        bounceDir += new Vector3(0, 0.5f, 0); // add arc
        return bounceDir.normalized * incomingVelocity.magnitude * 1.2f;
    }
}
