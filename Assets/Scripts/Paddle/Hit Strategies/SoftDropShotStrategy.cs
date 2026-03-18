using UnityEngine;

public sealed class SoftDropShotStrategy : BaseSafeHitStrategy
{
    public SoftDropShotStrategy(
        SafeHitStrategy.TableSpec table,
        SafeHitStrategy.ShotTuning tune,
        System.Random rng = null)
        : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(
        SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = t;

        // Slower and safer than normal drive, but still reliable.
        tt.minSpeed = Mathf.Max(5f, t.minSpeed);
        tt.maxSpeed = Mathf.Min(20f, t.maxSpeed);

        // Give enough time to clear the net cleanly.
        tt.minT = Mathf.Max(t.minT, 0.80f);
        tt.maxT = Mathf.Max(t.maxT, 1.70f);

        return tt;
    }

    protected override Vector3 ChooseLandingPoint(
        Vector3 p0, Vector3 fwd, SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var b = Table.oppHalfBounds;

        Vector3 lateral = Vector3.Cross(Vector3.up, fwd).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float bias = Mathf.Clamp(inputDir, -1f, 1f) * sideSign;

        // Keep lateral choice conservative for reliability.
        float minX = b.min.x + t.lateralClamp * b.size.x;
        float maxX = b.max.x - t.lateralClamp * b.size.x;
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.30f * bias));

        bool oppIsPositiveZ = b.center.z > Table.netZ;

        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + t.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - t.zClearDist);

        // Front-mid opponent side: short enough to feel like a drop,
        // but far enough to be consistently solvable.
        float z = oppIsPositiveZ
            ? Mathf.Lerp(safeMinZ, safeMaxZ, 0.35f)
            : Mathf.Lerp(safeMaxZ, safeMinZ, 0.35f);

        return new Vector3(x, Table.tableY, z);
    }

    protected override bool TrySolveSafe(
        Vector3 p0, Vector3 fwd, Vector3 target,
        SafeHitStrategy.ShotTuning tune, float inputDir, out Vector3 v0)
    {
        // Just solve directly for the chosen safe short target.
        if (Planner.TrySolveBallistic(p0, target, tune, fwd, out v0))
            return true;

        // If needed, relax slightly deeper while staying short-ish.
        var b = Table.oppHalfBounds;

        Vector3 lateral = Vector3.Cross(Vector3.up, fwd).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float bias = Mathf.Clamp(inputDir, -1f, 1f) * sideSign;

        float minX = b.min.x + tune.lateralClamp * b.size.x;
        float maxX = b.max.x - tune.lateralClamp * b.size.x;
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.30f * bias));

        bool oppIsPositiveZ = b.center.z > Table.netZ;

        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + tune.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - tune.zClearDist);

        float fallbackZ = oppIsPositiveZ
            ? Mathf.Lerp(safeMinZ, safeMaxZ, 0.45f)
            : Mathf.Lerp(safeMaxZ, safeMinZ, 0.45f);

        Vector3 fallbackTarget = new Vector3(x, Table.tableY, fallbackZ);

        var fallbackTune = tune;
        fallbackTune.minT = Mathf.Max(tune.minT, 0.85f);
        fallbackTune.maxT = Mathf.Max(tune.maxT, 1.85f);
        fallbackTune.maxSpeed = Mathf.Max(tune.maxSpeed, 22f);

        if (Planner.TrySolveBallistic(p0, fallbackTarget, fallbackTune, fwd, out v0))
            return true;

        return false;
    }

    protected override Vector3 PostProcessVelocity(
        Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        // Preserve the solved path exactly.
        ballRb.angularVelocity = Vector3.zero;
        return v0;
    }

    protected override void ApplyShotVisuals(Rigidbody ballRb)
    {
        SetTrailColor(ballRb, Color.yellow);
    }
}