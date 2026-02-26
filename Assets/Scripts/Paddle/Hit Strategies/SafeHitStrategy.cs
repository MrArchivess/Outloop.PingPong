using UnityEngine;

public sealed class SafeHitStrategy
{
    public struct TableSpec
    {
        public Bounds fullBounds;
        public Bounds oppHalfBounds;
        public float tableY;
        public float netZ;
        public float netHeight;
        public float netMargin;
        public float netHalfThickness;
        public float ballRadius;
    }

    public struct ShotTuning
    {
        public float gravity;
        public float minT, maxT;
        public float minSpeed, maxSpeed;
        public float lateralClamp;
        public float zClearDist;
        public float preferForwardDot;
    }

    // ------------- Instance state -----------------------------------------------------

    public TableSpec Table { get; }
    public ShotTuning BaseTune { get; }
    private readonly System.Random _rng;

    public SafeHitStrategy(TableSpec table, ShotTuning baseTune, System.Random rng = null)
    {
        Table = table;
        BaseTune = baseTune;
        _rng = rng ?? new System.Random();
    }

    // ------------- Public API (building blocks) -----------------------------------------------------
    public Vector3 PickLandingPointBiased(Vector3 p0, ShotTuning t, float lateralBias = 0f)
    {
        var b = Table.oppHalfBounds;

        // X range with clamp margin
        float minX = b.min.x + t.lateralClamp * b.size.x;
        float maxX = b.max.x - t.lateralClamp * b.size.x;

        // Bias [-1..1] -> [0.05..0.95]-ish then clamp to min/max with margins
        float xLerp = Mathf.Clamp01(0.5f + 0.45f * Mathf.Clamp(lateralBias, -1f, 1f));
        float x = Mathf.Lerp(minX, maxX, xLerp);

        // Determine which side of the net the opponent half is on
        bool oppIsPositiveZ = b.center.z > Table.netZ;

        // Safe range: always at least zClearDist past the net, but still inside oppHalfBounds
        float safeMinZ = oppIsPositiveZ
            ? Mathf.Max(b.min.z, Table.netZ + t.zClearDist)
            : b.min.z;

        float safeMaxZ = oppIsPositiveZ
            ? b.max.z
            : Mathf.Min(b.max.z, Table.netZ - t.zClearDist);

        // If the safe range is collapsed, fall back to center (still better than invalid)
        if (safeMaxZ <= safeMinZ + 1e-4f)
        {
            float zFallback = Mathf.Clamp(b.center.z, b.min.z, b.max.z);
            return new Vector3(x, Table.tableY, zFallback);
        }

        // Gentle jitter along Z to avoid repetition
        float zSpan = b.size.z * 0.5f;
        float zJitter = (float)(_rng.NextDouble() * 0.4 - 0.2) * zSpan;
        float z = Mathf.Clamp(Mathf.Lerp(safeMinZ, safeMaxZ, 0.5f) + zJitter, safeMinZ, safeMaxZ);

        return new Vector3(x, Table.tableY, z);
    }

    public bool TrySolveBallistic(Vector3 p0, Vector3 pt, ShotTuning tune, Vector3 paddleForward, out Vector3 bestV0)
    {
        bestV0 = Vector3.zero;
        float bestScore = float.NegativeInfinity;
        bool any = false;

        float g = tune.gravity;
        Vector3 a = new Vector3(0f, -g, 0f);

        //coarse-to-fine scan (adjust steps if you want)
        const int STEPS = 24;
        for (int i = 0; i < STEPS; i++)
        {
            float T = Mathf.Lerp(tune.minT, tune.maxT, i / (float)(STEPS - 1));

            if (T <= 1e-4f) continue;

            Vector3 v0 = (pt - p0 - 0.5f * a * (T * T)) / T;
            float speed = v0.magnitude;
            if (speed < tune.minSpeed || speed > tune.maxSpeed) continue;

            if (!TryGetNetClearance(p0, v0, T, Table, g, out float clearance)) continue;
            if(clearance < 0f) continue;

            float align = Vector3.Dot(v0.normalized, paddleForward.normalized);
            float score = (clearance * 15f) + (align * tune.preferForwardDot) - 0.01f * speed;

            if (score > bestScore)
            {
                bestScore = score;
                bestV0 = v0;
                any = true;
            }
        }

        return any;
    }

    static bool TryGetNetClearance(Vector3 p0, Vector3 v0, float T, TableSpec tbl, float g, out float clearance)
    {
        clearance = float.PositiveInfinity;

        float dz = v0.z;
        if (Mathf.Abs(dz) < 1e-3f) return false;

        float tNet = (tbl.netZ - p0.z) / dz;
        if (tNet <= 0f || tNet >= T) return false;

        float yNet = p0.y + v0.y * tNet - 0.5f * g * tNet * tNet;
        clearance = yNet - (tbl.netHeight + tbl.netMargin);
        return true;
    }

    public bool TryLob(Vector3 p0, ShotTuning tune, Vector3 paddleForward, out Vector3 v0, out Vector3 lobTarget)
    {
        v0 = Vector3.zero; lobTarget = Vector3.zero;
        var deepTune = tune;
        deepTune.minT = Mathf.Max(tune.minT, 0.9f);
        deepTune.maxT = Mathf.Max(tune.maxT, 1.6f);
        deepTune.minSpeed = Mathf.Max(5f, tune.minSpeed * 0.9f);
        deepTune.maxSpeed = Mathf.Min(28f, tune.maxSpeed * 1.1f);

        var b = Table.oppHalfBounds;
        float x = Mathf.Lerp(b.min.x, b.max.x, 0.5f);
        float z = (b.center.z > Table.netZ)
            ? (b.max.z - 0.15f * b.size.z)
            : (b.min.z + 0.15f * b.size.z);

        lobTarget = new Vector3(x, Table.tableY, Mathf.Clamp(z, b.min.z, b.max.z));
        return TrySolveBallistic(p0, lobTarget, deepTune, paddleForward, out v0);
    }

    public bool ComputeSafeVelocity(Vector3 p0, Vector3 paddleForward, ShotTuning tune, float lateralBias, out Vector3 v0, out Vector3 chosenLanding)
    {
        v0 = Vector3.zero; chosenLanding = Vector3.zero;

        Vector3 pt = PickLandingPointBiased(p0, tune, lateralBias);

        if (TrySolveBallistic(p0, pt, tune, paddleForward, out v0))
        {
            chosenLanding = pt;
            return true;
        }

        if (TryLob(p0, tune, paddleForward, out v0, out pt))
        {
            chosenLanding = pt;
            return true;
        }

        // Final keep-alive push to keep the rally playable
        v0 = new Vector3(0f, 4.5f, Mathf.Sign(Table.oppHalfBounds.center.z - p0.z) * 6f);
        chosenLanding = p0 + new Vector3(0f, 0f, Mathf.Sign(v0.z) * 0.5f);
        return false;
    }

    // ------------------- small helpers (public for tests) -----------------------------

    public static Vector3 SolveV0ForTime(Vector3 p0, Vector3 pt, float gravity, float T)
    {
        Vector3 a = new Vector3(0f, -gravity, 0f);
        return (pt - p0 - 0.5f * a * (T * T)) / T;
    }

    public static bool TryTimeAtZ(Vector3 p0, Vector3 v0, float planeZ, float T, out float t)
    {
        t = 0f;
        float dz = v0.z;
        if (Mathf.Abs(dz) < 1e-6f) return false;
        float tt = (planeZ - p0.z) / dz;
        if (tt > 0f && tt < T) { t = tt; return true; }
        return false;
    }
}
