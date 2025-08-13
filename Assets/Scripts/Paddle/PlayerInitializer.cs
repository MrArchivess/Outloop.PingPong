using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInitializer : MonoBehaviour
{
    private PaddleController paddleController;
    private HitDetector hitDetector;
    private PlayerInput playerInput;
    private InputHandler inputHandler;

    public PlayerSide Side {  get; private set; }

    [SerializeField] private Transform boundsLeft;
    [SerializeField] private Transform boundsRight;

    private void Awake()
    {
        paddleController = GetComponent<PaddleController>();
        inputHandler  = GetComponent<InputHandler>();
        playerInput = GetComponent<PlayerInput>();
        hitDetector = GetComponentInChildren<HitDetector>();
    }

    public void InitializeCore(PlayerSide side, Bounds bounds)
    {
        Side = side;
        paddleController.SetSide(side);
        paddleController.SetMovementBounds(bounds);
        paddleController.SetHitDetector();
        GameManager.Instance.RegisterPaddle(side, paddleController);
        inputHandler.SetInputs();

        StartCoroutine(ForceOrientationStable());
    }

    private IEnumerator ForceOrientationStable()
    {
        yield return new WaitForEndOfFrame();
        paddleController.SetPlayerRotation(Side);

        yield return new WaitForEndOfFrame();
        paddleController.SetPlayerRotation(Side);
    }

    public void ForceRotationNextFrame(PlayerSide side)
    {
        StartCoroutine(RotateLate(side));
    }

    private IEnumerator RotateLate(PlayerSide side)
    {
        yield return new WaitForEndOfFrame();
        paddleController.SetPlayerRotation(side);
    }

    public void SetBounds(Transform left, Transform right)
    {
        boundsLeft = left;
        boundsRight = right;
    }
}
