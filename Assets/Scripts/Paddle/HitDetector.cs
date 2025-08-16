using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    private PlayerControls playerControls;

    public static event Action OnServeStarted;
    public static event Action OnServeCompleted;
    public static event Action<PlayerSide> OnBallHit;
    public static event Action<AudioClip> OnBallHitSFX;

    private float chargeTime = 0f;
    private float chargeDuration = 0f;
    private float maxDuration = 1f;
    private float maxCharge = 0.5f;
    private bool isCharging;
    private Vector2 lastInputDirection;

    private GameObject ball;
    private BallController ballCtrl;
    private Rigidbody ballRb;

    [SerializeField] private IBallHitStrategy hitStrategy;

    [SerializeField] private Renderer[] glowRenderer;


    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0);
    [SerializeField] private float maxEmission = 10f;

    [SerializeField] private PlayerSide playerSide;

    [SerializeField] private AudioClip hitClip;

    public void Initialize(PlayerSide _playerSide)
    {
        playerSide = _playerSide;
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
        if (hitStrategy == null) hitStrategy = new SimpleHitStrategy();
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballRb = ball.GetComponent<Rigidbody>();
        ballCtrl = ball.GetComponent<BallController>();
    }

    private void Update()
    {
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            chargeDuration += Time.deltaTime;

            float normalizedCharge = Mathf.Clamp01(chargeTime / maxCharge);
            float normalizedDuration = Mathf.Clamp01(chargeDuration / maxCharge);

            Color finalGlow = glowColor * Mathf.Lerp(maxEmission, 0f, normalizedDuration);

            foreach (Renderer renderer in glowRenderer)
            {
                Material[] materials = renderer.materials;
                foreach (Material mat in materials)
                {
                    mat.SetColor("_EmissionColor", finalGlow);
                }
            }

            if (chargeTime > maxCharge)
                chargeTime = maxCharge;

            if (chargeDuration > maxDuration)
            {
                isCharging = false;
                foreach (Renderer renderer in glowRenderer)
                {
                    Material[] materials = renderer.materials;
                    foreach (Material mat in materials)
                    {
                        mat.SetColor("_EmissionColor", Color.black);
                    }
                }
                chargeTime = 0;
                chargeDuration = 0;
            }
        }
    }

    public void HandleHitButton()
    {
        if (GameManager.Instance.MatchState is MatchActiveState)
        {
            switch (GameManager.Instance.GameState)
            {
                case (ServingState):
                    {
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
                    }
                case (PlayingState):
                    {
                        chargeTime = 0f;
                        chargeDuration = 0f;

                        if (!isCharging) isCharging = true;
                        else isCharging = false;
                        break;
                    }
            }
        }
    }

    public void SetDirection(Vector2 direction)
    {
        lastInputDirection = direction;
    }

    private void MakeHit()
    {
        isCharging = false;
        float normalizedCharge = Mathf.Clamp01(chargeTime / maxCharge);

        float inputDirection = lastInputDirection.x;

        hitStrategy.ApplyHit(ballRb, gameObject.GetComponent<Transform>(), normalizedCharge, inputDirection);
        foreach (Renderer renderer in glowRenderer)
        {
            Material[] materials = renderer.materials;
            foreach (Material mat in materials)
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        if (!ballRb.useGravity) ballRb.useGravity = true;

        chargeTime = 0f;
        OnBallHit?.Invoke(playerSide);
        OnBallHitSFX?.Invoke(hitClip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (isCharging)
            {
                MakeHit();
            }
        }
    }
}
