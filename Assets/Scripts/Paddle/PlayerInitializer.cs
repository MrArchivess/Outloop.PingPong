using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInitializer : MonoBehaviour
{
    public static event Action SetPlayerInputs;

    private PaddleController paddleController;
    private HitDetector hitDetector;
    private PlayerInput playerInput;
    private InputHandler inputHandler;

    [SerializeField] private Transform boundsLeft;
    [SerializeField] private Transform boundsRight;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        paddleController = GetComponent<PaddleController>();
        hitDetector = GetComponentInChildren<HitDetector>();
        inputHandler  = GetComponent<InputHandler>();
    }

    public void InitializePlayer(PlayerSide side, Bounds bounds)
    {
        paddleController.SetSide(side);
        paddleController.SetHitDetector();
        GameManager.Instance.RegisterPaddle(side, paddleController);
        paddleController.SetMovementBounds(bounds);
        Quaternion originalRotation = transform.rotation;
        inputHandler.SetInputs();

        if (side == PlayerSide.Right)
        {
            //transform.rotation *= Quaternion.Euler(0, 180, 0);
            RotateRightPlayer();
        }

        Quaternion correctRotation = originalRotation *= Quaternion.Euler(0, 180, 0);
        if (side == PlayerSide.Right && transform.rotation != correctRotation)
            RotateRightPlayer();
    }

    private void RotateRightPlayer()
    {
        Quaternion rotation = Quaternion.Euler(0, 180, 0);
        transform.rotation *= rotation;
    }

    public void SetBounds(Transform left, Transform right)
    {
        boundsLeft = left;
        boundsRight = right;
    }
}
