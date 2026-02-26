using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    // ---------- Events ------------
    public static event Action OnServeStarted;
    public static event Action OnServeCompleted;
    public static event Action<PlayerSide> OnBallHit;
    public static event Action<AudioClip> OnBallHitSFX;

    // ---------- Input/charge ------------
    [Header("Charge")]
    private float chargeTime = 0f;
    private float chargeDuration = 0f;
    [SerializeField] private float maxDuration = 1f;
    [SerializeField] private float maxCharge = 0.5f;
    private bool isCharging;
    private Vector2 lastInputDirection;

    // ---------- Ball refs ------------
    private GameObject ball;
    private BallController ballCtrl;
    private Rigidbody ballRb;



    // ---------- Visuals/SFX ------------
    [Header("Visuals/SFX")]
    [SerializeField] private Renderer[] glowRenderer;
    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0);
    [SerializeField] private float maxEmission = 10f;
    [SerializeField] private AudioClip hitClip;

    // ---------- PlayerSide ------------
    private PlayerSide playerSide;

    // ---------- Table & Net config ------------
    [Header("Table & Net Config")]
    [SerializeField] private Bounds fullTableBounds;
    [SerializeField] private Bounds leftHalfBounds;
    [SerializeField] private Bounds rightHalfBounds;
    [SerializeField] private float tableY;

    [SerializeField] private NetMetricsProvider netProvider;

    // ---------- Safe Shot Tuning ---------------
    [Header("Safe Shot Tuning")]
    [SerializeField] private float minT = 0.35f;
    [SerializeField] private float maxT = 1.20f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 24f;
    [SerializeField] private float lateralClamp = 0.06f;
    [SerializeField] private float zClearDist = 0.12f;
    [SerializeField] private float preferForwardDot = 0.6f;
    [SerializeField] private float ballRadius = 0.02f;
    [SerializeField] private float netClearanceExtra = 0.02f;

    // ---------- Choose which safe shot you want ------------
    public enum ShotKind { Drive, Drop }
    [Header("Shot Type")]
    [SerializeField] private ShotKind shotKind = ShotKind.Drive;

    // ---------- Strategy ----------
    [SerializeReference] private IBallHitStrategy hitStrategy;

    private SafeHitStrategy.TableSpec tableSpec;
    private SafeHitStrategy.ShotTuning tune;
    private System.Random rng;

    public void Initialize(PlayerSide _playerSide)
    {
        playerSide = _playerSide;
        BuildSpecAndStrategy();
    }

    public void SetHitClip(AudioClip hitSFX)
    {
        hitClip = hitSFX;
    }


    public void SetGlowRenderers(Renderer[] glowRenderer)
    {
        this.glowRenderer = glowRenderer;
    }

    private void Awake()
    {
        //if (hitStrategy == null) hitStrategy = new SimpleHitStrategy();
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballRb = ball.GetComponent<Rigidbody>();
        ballCtrl = ball.GetComponent<BallController>();
        netProvider = FindFirstObjectByType<NetMetricsProvider>();

        rng ??= new System.Random();
    }

    private void Update()
    {
        if (!isCharging) return;

        chargeTime += Time.deltaTime;
        chargeDuration += Time.deltaTime;

        float normalizedDuration = Mathf.Clamp01(chargeDuration / maxDuration);

        Color finalGlow = glowColor * Mathf.Lerp(maxEmission, 0f, normalizedDuration);

        if (glowRenderer != null)
        {
            foreach (Renderer renderer in glowRenderer)
            {
                Material[] materials = renderer.materials;
                foreach (Material mat in materials)
                {
                    mat.SetColor("_EmissionColor", finalGlow);
                }
            }
        }

        if (chargeTime > maxCharge) chargeTime = maxCharge;

        if (chargeDuration > maxDuration)
        {
            isCharging = false;
            ClearGlow();
            chargeTime = 0;
            chargeDuration = 0;
        }

    }

    public void HandleHitButton()
    {
        if (GameManager.Instance.MatchState is MatchActiveState)
        {
            switch (GameManager.Instance.GameState)
            {
                case ServingState:
                    if (GameManager.Instance.CurrentServer == playerSide)
                    {
                        if (!ballCtrl.IsServed)
                            OnServeStarted?.Invoke();
                        else
                        {
                            MakeHit();
                            OnServeCompleted?.Invoke();
                        }
                    }
                    break;

                case PlayingState:
                    chargeTime = 0f;
                    chargeDuration = 0f;
                    isCharging = !isCharging;
                    break;

            }
        }
    }

    public void SetDirection(Vector2 direction) => lastInputDirection = direction;
    

    private void MakeHit()
    {
        EnsureStrategyReady();

        isCharging = false;
        float normalizedCharge = Mathf.Clamp01(chargeTime / maxCharge);
        float inputDirectionX = lastInputDirection.x;

        hitStrategy.ApplyHit(ballRb, transform, normalizedCharge, inputDirectionX);

        ClearGlow();
        if (!ballRb.useGravity) ballRb.useGravity = true;

        chargeTime = 0f;
        OnBallHit?.Invoke(playerSide);
        OnBallHitSFX?.Invoke(hitClip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && isCharging)
            MakeHit();
    }

    private void ClearGlow()
    {
        if (glowRenderer == null) return;
        foreach (Renderer renderer in glowRenderer)
        {
            Material[] materials = renderer.materials;
            foreach (Material mat in materials)
                mat.SetColor("_EmissionColor", Color.black);
        }
    }

    private void EnsureStrategyReady()
    {
        if (hitStrategy != null) return;

        if (tableSpec.oppHalfBounds.size == Vector3.zero)
            BuildSpecAndStrategy();
        else
            BuildStrategyOnly();
    }

    private bool TryAutoConfigureTableFromColliders()
    {
        //Find table halves by tag
        GameObject leftObj = GameObject.FindGameObjectWithTag("Table_Left");
        GameObject rightObj = GameObject.FindGameObjectWithTag("Table_Right");
        if (leftObj == null || rightObj == null)
        {
            Debug.LogWarning("[HitDetector] Could not find Table_Left / Table_Right by tag. Cannot auto-configure bounds.");
            return false;
        }

        var leftCol = leftObj.GetComponent<Collider>();
        var rightCol = rightObj.GetComponent<Collider>();
        if (leftCol == null || rightCol == null)
        {
            Debug.LogWarning("[HitDetector] Table_Left / Table_Right missing Collider. Cannot auto-configure bounds.");
            return false;
        }

        leftHalfBounds = leftCol.bounds;
        rightHalfBounds = rightCol.bounds;

        fullTableBounds = leftHalfBounds;
        fullTableBounds.Encapsulate(rightHalfBounds);

        tableY = fullTableBounds.max.y;

        return true;
    }


    private void BuildSpecAndStrategy()
    {
        //Auto-config if inspector bounds were never set
        if (fullTableBounds.size == Vector3.zero ||
            leftHalfBounds.size == Vector3.zero ||
            rightHalfBounds.size == Vector3.zero)
        {
            TryAutoConfigureTableFromColliders();
        }

        // 1) Figure out opponent half
        Bounds oppHalf = (playerSide == PlayerSide.Left) ? rightHalfBounds : leftHalfBounds;

        // 2) Ask the net for live metrics
        float netZ, netTopY;
        Bounds netBounds = new Bounds();
        if (netProvider == null || !netProvider.TryGetMetrics(out netZ, out netTopY, out netBounds))
        {
            Debug.LogWarning("[HitDetector] NetMetricsProvider missing or invalid. Falling back to oppHalf center Z and +0.15m height.");
            netZ = oppHalf.center.z;   // crude fallback
            netTopY = tableY + 0.15f;
        }

        tableSpec = new SafeHitStrategy.TableSpec
        {
            fullBounds = fullTableBounds,
            oppHalfBounds = oppHalf,
            tableY = tableY,
            netZ = netZ,
            netHeight = netTopY,     // already includes provider.extraMargin
            netMargin = ballRadius + netClearanceExtra,           // margin baked into netTopY; keep 0 here or add extra if you like
            netHalfThickness = netBounds.extents.z,
        };

        tune = new SafeHitStrategy.ShotTuning
        {
            gravity = Physics.gravity.magnitude,
            minT = minT,
            maxT = maxT,
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,
            lateralClamp = lateralClamp,
            zClearDist = zClearDist,
            preferForwardDot = preferForwardDot
        };

        BuildStrategyOnly();
    }

    private void BuildStrategyOnly()
    {
        rng ??= new System.Random();

        // Choose which safe shot to use by default.
        switch (shotKind)
        {
            case ShotKind.Drop:
                hitStrategy = new SoftDropShotStrategy(tableSpec, tune, rng);
                break;
            default:
                hitStrategy = new TopspinDriveStrategy(tableSpec, tune, rng);
                break;
        }
    }

}


