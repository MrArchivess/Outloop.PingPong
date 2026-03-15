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

    // ---------- HitPhase -------------
    private enum HitPhase { Idle, Windup, Active, Recovery }
    [Header("HitPhase")]
    [SerializeField] private bool useAnimationEvents = false;
    [SerializeField] private float windupTime = 0.10f;
    [SerializeField] private float activeTime = 0.12f;
    [SerializeField] private float recoveryTime = 0.18f;

    private HitPhase phase = HitPhase.Idle;
    private float phaseTimer = 0f;
    private Vector2 lastInputDirection;

    [SerializeField] private Animator animator;
    [SerializeField] private string swingTriggerName = "Swing";

    private int swingTriggerHash;

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

    // ---------- Hit Proximity ------------------
    [Header("Hit Proximity")]
    [SerializeField] private Transform contactAnchor;
    [SerializeField] private Vector3 proximityOffset = new Vector3(0f, 0f, 0.25f);
    [SerializeField] private float proximityRadius = 1f;
    [SerializeField] private LayerMask ballLayer;

    [Header("Grace Return")]
    [SerializeField] private float graceWindowTime = 0.35f;
    [SerializeField] private float lungeRadius = 1.5f;
    [SerializeField] private Vector3 lungeOffset = new Vector3(0f, 0f, 0.5f);

    private bool graceWindowActive = false;
    private float graceTimer = 0f;


    [Header("Shot Input Buffer")]
    [SerializeField] private float comboBufferTime = 0.12f;

    private ShotSlot pendingShotSlot = ShotSlot.None;
    private FaceButton firstButton = FaceButton.None;
    private FaceButton secondButton = FaceButton.None;
    private float comboBufferTimer = 0f;
    private bool comboBufferOpen = false;

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

    public void SetAnimator(Animator anim)
    {
        animator = anim;
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

        ballLayer = LayerMask.GetMask("Ball");
        rng ??= new System.Random();

        if (animator == null)
            animator = GetComponentInChildren<Animator>() ?? GetComponentInParent<Animator>();

        swingTriggerHash = Animator.StringToHash(swingTriggerName);
    }

    private void Update()
    {
        if (phase != HitPhase.Idle)
        {
            phaseTimer += Time.deltaTime;

            switch (phase)
            {
                case HitPhase.Windup:
                    HandleGlow();
                    if (!useAnimationEvents && phaseTimer >= windupTime) { phase = HitPhase.Active; phaseTimer = 0; }
                    break;
                case HitPhase.Active:
                    bool canNormalHit = IsBallInProximity();
                    bool canGraceHit = graceWindowActive && IsBallInLungeProximity();
                    if (canNormalHit) { MakeHit(); }
                    else if (canGraceHit) { MakeHit(); }
                    else if (!useAnimationEvents && phaseTimer >= activeTime) { phase = HitPhase.Recovery; phaseTimer = 0f; }
                    break;
                case HitPhase.Recovery:
                    if (!useAnimationEvents && phaseTimer >= recoveryTime) { phase = HitPhase.Idle; ClearGlow(); phaseTimer = 0f; }
                    break;
            }
        }

        if (comboBufferOpen)
        {
            comboBufferTimer += Time.deltaTime;
            if (comboBufferTimer >= comboBufferTime || phase != HitPhase.Windup)
            {
                comboBufferOpen = false;
            }
        }

        if (graceWindowActive)
        {
            graceTimer += Time.deltaTime;
            if (graceTimer >= graceWindowTime)
            {
                graceWindowActive = false;
                graceTimer = 0f;
            }
        }
    }

    public void Animation_BeginActiveWindow()
    {
        if (phase != HitPhase.Windup) return;

        phase = HitPhase.Active;
        phaseTimer = 0f;
    }

    public void Animation_EndActiveWindow()
    {
        if (phase != HitPhase.Active) return;
        
        phase = HitPhase.Recovery;
        phaseTimer = 0f;
    }

    public void Animation_EndRecovery()
    {
        if (phase != HitPhase.Recovery) return;

        phase = HitPhase.Idle;
        phaseTimer = 0f;
        ClearGlow();
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
                    StartHitWindow();
                    break;

            }
        }
    }

    public void HandleFaceButton(FaceButton button)
    {
        if (GameManager.Instance.MatchState is not MatchActiveState)
            return;

        switch (GameManager.Instance.GameState)
        {
            case ServingState:
                if (GameManager.Instance.CurrentServer != playerSide)
                    return;

                if (!ballCtrl.IsServed)
                    OnServeStarted?.Invoke();
                else
                {
                    MakeHit();
                    OnServeCompleted?.Invoke();
                }
                break;

            case PlayingState:
                HandleShotInput(button);
                break;

        }
    }

    public void NotifyFirstBounceOnOwnSide()
    {
        graceWindowActive = true;
        graceTimer = 0f;
    }

    public void NotifyBounceOnOtherSideOrSecondBounce()
    {
        graceWindowActive = false;
        graceTimer = 0f;
    }

    private ShotSlot ToSingleSlot(FaceButton button)
    {
        return button switch
        {
            FaceButton.A => ShotSlot.A,
            FaceButton.B => ShotSlot.B,
            FaceButton.X => ShotSlot.X,
            FaceButton.Y => ShotSlot.Y,
            _ => ShotSlot.None
        };
    }

    private ShotSlot ToComboSlot(FaceButton a, FaceButton b)
    {
        if (a == b) return ToSingleSlot(a);

        // Normalize order so A+B == B+A
        if ((int)a > (int)b)
            (a, b) = (b, a);

        if (a == FaceButton.A && b == FaceButton.B) return ShotSlot.AB;
        if (a == FaceButton.A && b == FaceButton.X) return ShotSlot.AX;
        if (a == FaceButton.A && b == FaceButton.Y) return ShotSlot.AY;
        if (a == FaceButton.B && b == FaceButton.X) return ShotSlot.BX;
        if (a == FaceButton.B && b == FaceButton.Y) return ShotSlot.BY;
        if (a == FaceButton.X && b == FaceButton.Y) return ShotSlot.XY;

        return ShotSlot.None;
    }

    private void HandleShotInput(FaceButton button)
    {
        if (phase == HitPhase.Idle)
        {
            firstButton = button;
            secondButton = FaceButton.None;
            pendingShotSlot = ToSingleSlot(button);

            comboBufferOpen = true;
            comboBufferTimer = 0f;

            //Debug.Log($"[HitDetector] Selected initial shot slot: {pendingShotSlot}");
            StartHitWindow();
            return;
        }

        if (phase == HitPhase.Windup && comboBufferOpen)
        {
            if (button == firstButton) return;
            if (button == FaceButton.None) return;
            if (secondButton != FaceButton.None) return;

            secondButton = button;
            pendingShotSlot = ToComboSlot(firstButton, secondButton);
            //Debug.Log($"[HitDetector] Updated combo shot slot: {pendingShotSlot}");
        }
    }

    private void StartHitWindow()
    {
        if (phase != HitPhase.Idle) return;
        phase = HitPhase.Windup;
        phaseTimer = 0f;

        if (animator != null)
            animator.SetTrigger(swingTriggerHash);
    }

    public void SetDirection(Vector2 direction) => lastInputDirection = direction;


    private void MakeHit()
    {
        EnsureStrategyReady();

        float timing01 = Mathf.Clamp01(phaseTimer / activeTime);
        float quality = 1f - Mathf.Abs(timing01 - 0.5f) * 2f;
        float inputDirectionX = lastInputDirection.x;

        hitStrategy.ApplyHit(ballRb, transform, quality, inputDirectionX);

        ClearGlow();
        if (!ballRb.useGravity) ballRb.useGravity = true;

        phase = HitPhase.Recovery;
        phaseTimer = 0f;

        OnBallHit?.Invoke(playerSide);
        OnBallHitSFX?.Invoke(hitClip);

        Debug.Log($"[HitDetector] Executing shot slot: {pendingShotSlot}");
        pendingShotSlot = ShotSlot.None;
        firstButton = FaceButton.None;
        secondButton = FaceButton.None;
        comboBufferOpen = false;
        comboBufferTimer = 0f;
    }

    private bool IsBallInProximity()
    {
        if (ballRb == null) return false;

        Vector3 origin = contactAnchor ? contactAnchor.position : transform.position;
        origin += (contactAnchor ? contactAnchor.TransformDirection(proximityOffset) : transform.TransformDirection(proximityOffset));

        float r = proximityRadius;
        Collider[] hits = Physics.OverlapSphere(origin, r, ballLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag("Ball")) return true;

        return false;
    }

    private bool IsBallInLungeProximity()
    {
        if (ballRb == null) return false;

        Vector3 origin = contactAnchor ? contactAnchor.position : transform.position;
        origin += (contactAnchor ? contactAnchor.TransformDirection(lungeOffset)
                                : transform.TransformDirection(lungeOffset));

        Collider[] hits = Physics.OverlapSphere(origin, lungeRadius, ballLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag("Ball")) return true;

        return false;
    }

    private void HandleGlow()
    {
        float t = Mathf.Clamp01(phaseTimer / windupTime);
        Color finalGlow = glowColor * Mathf.Lerp(0f, maxEmission, t);
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
            //Debug.LogWarning("[HitDetector] Could not find Table_Left / Table_Right by tag. Cannot auto-configure bounds.");
            return false;
        }

        var leftCol = leftObj.GetComponent<Collider>();
        var rightCol = rightObj.GetComponent<Collider>();
        if (leftCol == null || rightCol == null)
        {
            //Debug.LogWarning("[HitDetector] Table_Left / Table_Right missing Collider. Cannot auto-configure bounds.");
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
            //Debug.LogWarning("[HitDetector] NetMetricsProvider missing or invalid. Falling back to oppHalf center Z and +0.15m height.");
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

    private void OnLegalBounceMade(PlayerSide bouncedSide)
    {
        if (bouncedSide != playerSide) return;

        graceWindowActive = true;
        graceTimer = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = contactAnchor ? contactAnchor.position : transform.position;
        origin += (contactAnchor ? contactAnchor.TransformDirection(proximityOffset)
                                : transform.TransformDirection(proximityOffset));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, proximityRadius);

        Vector3 lungeOrigin = contactAnchor ? contactAnchor.position : transform.position;
        lungeOrigin += (contactAnchor ? contactAnchor.TransformDirection(lungeOffset)
                                     : transform.TransformDirection(lungeOffset));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lungeOrigin, lungeRadius);
    }
    private void OnEnable()
    {
        TableSideBoundsDetector.legalMoveMadeOnSide += OnLegalBounceMade;
    }

    private void OnDisable()
    {
        TableSideBoundsDetector.legalMoveMadeOnSide -= OnLegalBounceMade;
    }
}

// ---------- Shot Slots ---------------------

public enum FaceButton
{
    None,
    A,
    B,
    X,
    Y
}

public enum ShotSlot
{
    None,
    A,
    B,
    X,
    Y,
    AB,
    AX,
    AY,
    BX,
    BY,
    XY
}

