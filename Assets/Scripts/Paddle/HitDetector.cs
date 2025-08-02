using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    public static event Action OnServeStarted;
    public static event Action OnServeCompleted;
    public static event Action<PlayerSide> OnBallHit;

    private float chargeTime = 0f;
    private float chargeDuration = 0f;
    private float maxDuration = 1f;
    private float maxCharge = 0.5f;
    private bool isCharging;

    private bool hitButton = false;

    private string inputX = "";
    private string inputHit = "";

    private GameObject ball;
    private BallController ballCtrl;
    private Rigidbody ballRb;

    [SerializeField] private IBallHitStrategy hitStrategy;

    [SerializeField] private Renderer[] glowRenderer;


    [SerializeField] private Color glowColor = new Color(1f, 0.5f, 0);
    [SerializeField] private float maxEmission = 10f;

    private PlayerSide playerSide;

    public void Initialize(PlayerSide _playerSide)
    {
        playerSide = _playerSide;

        if (playerSide == PlayerSide.Left)
        {
            inputHit = "Hit";
            inputX = "Horizontal";
        }
        else if (playerSide == PlayerSide.Right)
        {
            inputHit = "Hit_P2";
            inputX = "Horizontal_P2";
        }
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
        hitButton = Input.GetButtonDown(inputHit);
        switch (GameManager.Instance.GameState)
        {
            case (ServingState):
                {
                    if (hitButton && GameManager.Instance.CurrentServer == playerSide)
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
                    if (hitButton)
                    {
                        if (!isCharging)
                            isCharging = true;
                        else
                            isCharging = false;

                        chargeTime = 0f;
                        chargeDuration = 0f;
                    }

                    if (isCharging)
                    {
                        chargeTime += Time.deltaTime;
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

                        chargeDuration += Time.deltaTime;
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
                    break;
                }
        }


    }

    private void MakeHit()
    {
        isCharging = false;
        float normalizedCharge = Mathf.Clamp01(chargeTime / maxCharge);
        float inputDirection = Input.GetAxis(inputX);

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
