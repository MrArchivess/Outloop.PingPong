using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInitializer : MonoBehaviour
{
    public static event Action<Vector3> PlayerInitialized;

    private PaddleController paddleController;
    private HitDetector hitDetector;
    private PlayerInput playerInput;
    private InputHandler inputHandler;

    private Vector3 originalPos;

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
        Vector3 originalPos = new Vector3(0, 0, 0);
        if (side == PlayerSide.Left)
        {
            originalPos = new Vector3(1, 0.5f, -3.33f);
        }

        else if (side == PlayerSide.Right)
        {
            originalPos = new Vector3(-1, 0.5f, 3.33f);
        }

        paddleController.SetOriginalPosition(originalPos);
        StartCoroutine(ForceOrientationStable());
    }

    private IEnumerator ForceOrientationStable()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForEndOfFrame();
            paddleController.SetPlayerRotation(Side);
        }
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
