using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SoftDropShotStrategy : BaseSafeHitStrategy
{
    public SoftDropShotStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning tune, System.Random rng = null)
        : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(
        SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = t;
        tt.maxSpeed = Mathf.Min(t.maxSpeed, 14f);
        tt.minSpeed = 4.5f;
        tt.minT = 0.45f;
        tt.maxT = 1.25f;
        return tt;
    }

    protected override Vector3 ChooseLandingPoint(
        Vector3 p0, Vector3 fwd, SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var b = Table.oppHalfBounds;
        Vector3 lateral = Vector3.Cross(Vector3.up, fwd).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float bias = Mathf.Clamp(inputDir, -1f, 1f) * sideSign;

        float minX = b.min.x + t.lateralClamp * b.size.x;
        float maxX = b.max.x - t.lateralClamp * b.size.x;
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.45f * bias));

        float z = (b.center.z > Table.netZ) ? (Table.netZ + t.zClearDist + 0.05f * b.size.z)
                                            : (Table.netZ - t.zClearDist - 0.05f * b.size.z);
        return new Vector3(x, Table.tableY, Mathf.Clamp(z, b.min.z, b.max.z));
    }
}

