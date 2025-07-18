using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBounceStrategy
{
    public Vector3 GetBounceDirection(Collision collision, Vector3 incomingVelocity);
}
