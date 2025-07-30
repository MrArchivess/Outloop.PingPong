using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBounceStrategy : IBounceStrategy
{
    public Vector3 GetBounceDirection(Collision collision, Vector3 incomingVelocity)
    {
        return Vector3.Reflect(incomingVelocity, collision.contacts[0].normal);
    }
}
