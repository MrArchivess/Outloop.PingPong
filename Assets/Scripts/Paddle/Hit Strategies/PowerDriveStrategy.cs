using UnityEngine;

public sealed class PowerDriveStrategy : BaseSafeHitStrategy
{
    public PowerDriveStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning tune, System.Random rng = null)
        : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(
        SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = t;

        // Still faster/flatter than normal drive, but not so flat that it constantly clips net.
        tt.minT = Mathf.Max(0.26f, t.minT - 0.04f);
        tt.maxT = Mathf.Min(0.95f, t.maxT);

        tt.minSpeed = Mathf.Max(t.minSpeed, 8f);
        tt.maxSpeed = Mathf.Min(30f, t.maxSpeed + 6f);

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

        bool oppIsPositiveZ = b.center.z > Table.netZ;

        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + t.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - t.zClearDist);

        // Aggressive, but not so deep that it constantly sails long.
        float z = Mathf.Lerp(safeMinZ, safeMaxZ, 0.62f);

        return new Vector3(x, Table.tableY, z);
    }

    protected override Vector3 PostProcessVelocity(Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        ballRb.angularVelocity = paddle.right * 120f * Mathf.Deg2Rad;
        return v0;
    }

    protected override void ApplyShotVisuals(Rigidbody ballRb)
    {
        SetTrailColor(ballRb, Color.red);
    }
}