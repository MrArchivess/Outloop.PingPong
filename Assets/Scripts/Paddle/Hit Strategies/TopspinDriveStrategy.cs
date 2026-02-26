using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TopspinDriveStrategy : BaseSafeHitStrategy
{
    public TopspinDriveStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning tune, System.Random rng = null) : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = base.AdjustTuning(t, charge, inputDir);
        tt.minT = Mathf.Max(tt.minT, 0.32f);
        tt.maxT = Mathf.Min(tt.maxT, 0.95f);
        tt.minSpeed = Mathf.Max(tt.minSpeed, 7.0f);
        return tt;
    }

    protected override Vector3 PostProcessVelocity(Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        ballRb.angularVelocity =
            (paddle.right * Mathf.Lerp(0f, 120f, charge)
            + paddle.up * (inputDir * 40f))
            * Mathf.Deg2Rad;
        return v0;
    }
}
