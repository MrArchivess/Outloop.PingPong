using System;
using UnityEngine;

public class PingPongHitFX : MonoBehaviour
{
    [SerializeField] Transform fxRoot;         // Root of the particle prefab (defaults to this transform)
    [SerializeField] float scale = 2f;         // 2x size
    [SerializeField] bool orientOpposite = true; // true = burst away from travel direction

    ParticleSystem[] systems;
    Vector3 lastDir = Vector3.forward;         // fallback if velocity is tiny

    void Awake()
    {
        if (!fxRoot) fxRoot = transform;
        systems = GetComponentsInChildren<ParticleSystem>(true);

        // Make transform scaling affect the whole effect
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        fxRoot.localScale = Vector3.one * scale;
    }

    private void OnEnable()
    {
        BallController.OnBallHit += PlayHitFX;
    }

    private void OnDisable()
    {
        BallController.OnBallHit -= PlayHitFX;
    }

    private void PlayHitFX(BallController ball)
    {
        Vector3 dir  = lastDir;
        if (ball.TryGetComponent<Rigidbody>(out var rb3) && rb3.velocity.sqrMagnitude > 1e-6f)
            dir = rb3.velocity.normalized;
        else if (ball.TryGetComponent<Rigidbody2D>(out var rb2) && rb2.velocity.sqrMagnitude > 1e-6f)
            dir = new Vector3(rb2.velocity.x, 0f, rb2.velocity.y).normalized;

        if (orientOpposite) dir = -dir;
        if (dir.sqrMagnitude < 1e-6f) dir = lastDir;
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
        lastDir = dir;

        foreach (var ps in systems)
            ps.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

        fxRoot.SetPositionAndRotation(ball.transform.position, Quaternion.LookRotation(dir, Vector3.up));
        foreach(var ps in systems)
            ps.Play(withChildren: true);
        
    }
}
