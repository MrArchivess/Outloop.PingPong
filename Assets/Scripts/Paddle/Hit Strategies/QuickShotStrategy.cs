using UnityEngine;

public sealed class QuickShotStrategy : BaseSafeHitStrategy
{
    public QuickShotStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning tune, System.Random rng = null)
        : base(table, tune, rng) { }

    protected override SafeHitStrategy.ShotTuning AdjustTuning(
        SafeHitStrategy.ShotTuning t, float charge, float inputDir)
    {
        var tt = t;

        float c = Mathf.Clamp01(charge);

        // Quicker than a normal drive, but clearly below PowerDrive.
        // Slightly shorter flight window than base so it reads as a faster shot.
        tt.minT = Mathf.Max(0.30f, t.minT - 0.02f);
        tt.maxT = Mathf.Min(1.05f, t.maxT - 0.04f);

        // Add some speed, but not as much as PowerDrive.
        // Small charge scaling so the shot responds to player input without
        // turning into a second power shot.
        tt.minSpeed = Mathf.Max(t.minSpeed + 0.75f, Mathf.Lerp(t.minSpeed + 0.75f, t.minSpeed + 1.75f, c));
        tt.maxSpeed = Mathf.Min(24f, Mathf.Lerp(t.maxSpeed + 1.5f, t.maxSpeed + 3.5f, c));

        // Keep it a little more open than PowerDrive laterally so it remains easier
        // to use as a pressure/rally shot rather than a pure winner attempt.
        tt.lateralClamp = Mathf.Clamp(t.lateralClamp + 0.01f, 0f, 0.20f);

        // Ensure it still aims safely beyond the net.
        tt.zClearDist = Mathf.Max(t.zClearDist, 0.12f);

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
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.42f * bias));

        bool oppIsPositiveZ = b.center.z > Table.netZ;

        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + t.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - t.zClearDist);

        // Land more forward than a normal drive would,
        // but not as deep/aggressive as PowerDrive.
        float z = Mathf.Lerp(safeMinZ, safeMaxZ, 0.52f);

        return new Vector3(x, Table.tableY, z);
    }

    protected override Vector3 PostProcessVelocity(
        Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        // Stronger spin than a normal drive, but less exaggerated than PowerDrive.
        ballRb.angularVelocity = paddle.right * 90f * Mathf.Deg2Rad;
        return v0;
    }

    protected override void ApplyShotVisuals(Rigidbody ballRb)
    {
        SetTrailColor(ballRb, Color.yellow);
    }
}