using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action OnServerChanged;

    [SerializeField] private GameObject paddlePrefab;

    [SerializeField] private GameObject boundsLeftObject;
    [SerializeField] private GameObject boundsRightObject;

    public PlayerSide CurrentServer => currentServer;
    private PlayerSide currentServer = PlayerSide.Left;

    public IGameState GameState => gameState;
    private IGameState gameState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
            Instance = this;

        SpawnPaddles();
        gameState = new ServingState();
    }

    private void SpawnPaddles()
    {
        Bounds leftBounds = boundsLeftObject.GetComponent<BoxCollider>().bounds;
        Bounds rightBounds = boundsRightObject.GetComponent<BoxCollider>().bounds;

        Vector3 leftSpawn = leftBounds.center;
        Vector3 rightSpawn = rightBounds.center;


        GameObject leftPlayer = Instantiate(paddlePrefab, leftSpawn, paddlePrefab.transform.rotation);
        GameObject rightPlayer = Instantiate(paddlePrefab, rightSpawn, paddlePrefab.transform.rotation * Quaternion.Euler(0, 180, 0));

        PaddleController leftCtrl = leftPlayer.GetComponent<PaddleController>();
        PaddleController rightCtrl = rightPlayer.GetComponent<PaddleController>();

        leftCtrl.SetMovementBounds(leftBounds);
        rightCtrl.SetMovementBounds(rightBounds);

        leftCtrl.SetSide(PlayerSide.Left);
        rightCtrl.SetSide(PlayerSide.Right);

        leftCtrl.SetHitDetector();
        rightCtrl.SetHitDetector();

        InputHandler leftInputHandler = gameObject.AddComponent<InputHandler>();
        leftInputHandler.Initialize(leftCtrl, 1);
        InputHandler rightInputHandler = gameObject.AddComponent<InputHandler>();
        rightInputHandler.Initialize(rightCtrl, 2);
    }

    private void SetServer()
    {
        if (currentServer == PlayerSide.Left)
            currentServer = PlayerSide.Right;
        else
            currentServer = PlayerSide.Left;
        OnServerChanged?.Invoke();
    }

    private GameObject GetCurrentServerPaddle()
    {
        var paddles = FindObjectsOfType<PaddleController>();
        foreach (var paddle in paddles)
        {
            if (paddle.Playerside == currentServer)
                return paddle.gameObject;
        }
        return null;
    }

    private void OnEnable()
    {
        HitDetector.OnServeCompleted += HandleServeCompleted;
        BallController.OnBallReset += HandleRoundReset;
    }

    private void OnDisable()
    {
        HitDetector.OnServeCompleted -= HandleServeCompleted;
        BallController.OnBallReset -= HandleRoundReset;
    }

    private void HandleServeCompleted()
    {
        gameState = new PlayingState();
    }

    private void HandleRoundReset()
    {
        SetServer();
        gameState = new ServingState();

        var ball = FindObjectOfType<BallController>();
        ball.SetServePosition(GetCurrentServerPaddle().transform.position);
    }
}
