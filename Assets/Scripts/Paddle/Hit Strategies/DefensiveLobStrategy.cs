using Unity.Mathematics;
using UnityEngine;

public sealed class DefensiveLobStrategy : BaseSafeHitStrategy
{
    public DefensiveLobStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning tune, System.Random rng = null)
        : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(
        SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = t;
        tt.minT = Mathf.Max(t.minT, 1.15f);
        tt.maxT = Mathf.Max(t.maxT, 2.05f);
        tt.minSpeed = Mathf.Max(5f, t.minSpeed);
        tt.maxSpeed = Mathf.Min(22f, t.maxSpeed + 4f);
        return tt;
    }

    protected override Vector3 ChooseLandingPoint(Vector3 p0, Vector3 fwd, SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var b = Table.oppHalfBounds;

        Vector3 lateral = Vector3.Cross(Vector3.up, fwd).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float bias = Mathf.Clamp(inputDir, -1, 1f) * sideSign;

        float minX = b.min.x + t.lateralClamp * b.size.x;
        float maxX = b.max.x - t.lateralClamp * b.size.x;
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.45f * bias));

        bool oppIsPositiveZ = b.center.z > Table.netZ;

        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + t.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - t.zClearDist);

        float z = Mathf.Lerp(safeMinZ, safeMaxZ, 0.72f);

        return new Vector3(x, Table.tableY, z);
    }
    protected override void ApplyShotVisuals(Rigidbody ballRb)
    {
        SetTrailColor(ballRb, new Color(0.6f, 0.2f, 1f));
    }

    protected override Vector3 PostProcessVelocity(Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        ballRb.angularVelocity = Vector3.zero;
        return v0;
    }
}
