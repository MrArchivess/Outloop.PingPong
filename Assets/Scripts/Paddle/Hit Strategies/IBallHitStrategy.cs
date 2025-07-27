using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBallHitStrategy
{
    void ApplyHit(Rigidbody ballRb, Transform paddleTransform);
}
