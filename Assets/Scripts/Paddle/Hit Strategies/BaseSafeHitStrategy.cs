using UnityEngine;

public abstract class BaseSafeHitStrategy : IBallHitStrategy
{
    protected readonly SafeHitStrategy.TableSpec Table;
    protected readonly SafeHitStrategy.ShotTuning BaseTune;
    protected readonly System.Random Rng;

    protected readonly SafeHitStrategy Planner;

    protected BaseSafeHitStrategy(SafeHitStrategy.TableSpec table, SafeHitStrategy.ShotTuning baseTune, System.Random rng = null)
    {
        Table = table;
        BaseTune = baseTune;
        Rng = rng ?? new System.Random();

        Planner = new SafeHitStrategy(table, baseTune, Rng);
    }

    // Final to guarantee invariants are always applied
    public void ApplyHit(Rigidbody ballRb, Transform paddleTransform, float charge, float inputDir)
    {

        var tune = AdjustTuning(BaseTune, charge, inputDir);
        var p0 = ballRb.position;
        var fwd = paddleTransform.forward;

        // Let subclass bias/choose a landing point (defaults to safe random)
        var target = ChooseLandingPoint(p0, fwd, tune, charge, inputDir);

        Debug.Log($"[L1 target] input={inputDir:0.00}  targetX={target.x:0.###}  bX=({Table.oppHalfBounds.min.x:0.###},{Table.oppHalfBounds.max.x:0.###})  clamp={tune.lateralClamp:0.00}");
        Debug.DrawLine(target + Vector3.up * 0.02f, target + Vector3.up * 0.22f, Color.magenta, 2f);


        // Solve a safe trajectory; fallback to lob if needed
        Vector3 v0;
        if (!TrySolveSafe(p0, fwd, target, tune, inputDir, out v0))
        {
            // last resort: gentle keep alive
            v0 = new Vector3(0f, 4.5f, Mathf.Sign(Table.oppHalfBounds.center.z - p0.z) * 6f);
        }

        // Give subclasses a chance to add spin/curve/etc.
        v0 = PostProcessVelocity(v0, ballRb, paddleTransform, charge, inputDir);

        // Apply impulse relative to current velocity for continuity
        var deltaV = v0 - ballRb.linearVelocity;
        ballRb.AddForce(ballRb.mass * deltaV, ForceMode.Impulse);

        OnShotApplied(ballRb, v0, target);
    }

    protected virtual SafeHitStrategy.ShotTuning AdjustTuning(SafeHitStrategy.ShotTuning tune, float charge, float inputDir)
    {
        var t = tune;
        t.maxSpeed += Mathf.Lerp(0f, 6f, Mathf.Clamp01(charge));
        t.minT = Mathf.Lerp(tune.minT, tune.minT + 0.10f, Mathf.Clamp01(charge));
        t.maxT = Mathf.Lerp(tune.maxT, tune.maxT + 0.20f, Mathf.Clamp01(charge));
        return t;
    }

    protected virtual Vector3 ChooseLandingPoint(Vector3 p0, Vector3 paddleForward, SafeHitStrategy.ShotTuning tune, float charge, float inputDir)
    {
        // Compute lateralBias the same way you already do
        Vector3 lateral = Vector3.Cross(Vector3.up, paddleForward).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float lateralBias = Mathf.Clamp(inputDir, -1f, 1f) * sideSign;

        // Use the already-fixed implementation
        return Planner.PickLandingPointBiased(p0, tune, lateralBias);
    }

    protected virtual Vector3 PostProcessVelocity(Vector3 v0, Rigidbody ballRb, Transform paddle, float charge, float inputDir)
    {
        ballRb.angularVelocity = paddle.right * Mathf.Lerp(0f, 60f, Mathf.Clamp01(charge)) * Mathf.Deg2Rad;
        return v0;
    }

    protected virtual void OnShotApplied(Rigidbody rb, Vector3 v0, Vector3 landing) { }

    // ---- Core solver wrapper (safety invariant) ----
    private bool TrySolveSafe(
        Vector3 p0, Vector3 fwd, Vector3 target,
        SafeHitStrategy.ShotTuning tune, float inputDir, out Vector3 v0)
    {
        if (SafeSolve(p0, fwd, target, tune, out v0)) return true;

        // --- Biased lob attempt (preserve left/right) ---
        var b = Table.oppHalfBounds;
        Vector3 lateral = Vector3.Cross(Vector3.up, fwd).normalized;
        float sideSign = Mathf.Sign(Vector3.Dot(lateral, Vector3.right));
        float bias = Mathf.Clamp(inputDir, -1f, 1f) * sideSign;

        var deepTune = tune;
        deepTune.minT = Mathf.Max(tune.minT, 0.9f);
        deepTune.maxT = Mathf.Max(tune.maxT, 1.6f);
        deepTune.minSpeed = Mathf.Max(5f, tune.minSpeed * 0.9f);
        deepTune.maxSpeed = Mathf.Min(28f, tune.maxSpeed * 1.1f);

        float minX = b.min.x + tune.lateralClamp * b.size.x;
        float maxX = b.max.x - tune.lateralClamp * b.size.x;
        float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(0.5f + 0.45f * bias));
        float z = (b.center.z > Table.netZ) ? (b.max.z - 0.15f * b.size.z)
                                               : (b.min.z + 0.15f * b.size.z);
        var lobTarget = new Vector3(x, Table.tableY, Mathf.Clamp(z, b.min.z, b.max.z));

        if (Planner.TrySolveBallistic(p0, lobTarget, deepTune, fwd, out v0)) return true;

        // Last-chance (unbiased) lob before keep-alive
        Vector3 _;
        if (Planner.TryLob(p0, tune, fwd, out v0, out _)) return true;

        return false;
    }

    private bool SafeSolve(Vector3 p0, Vector3 fwd, Vector3 pt, SafeHitStrategy.ShotTuning tune, out Vector3 v0) =>
        Planner.TrySolveBallistic(p0, pt, tune, fwd, out v0);
}
