using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action OnServerChanged;
    public static event Action<PlayerSide> PointWon;

    [SerializeField] private GameObject paddlePrefab;

    [SerializeField] private GameObject boundsLeftObject;
    [SerializeField] private GameObject boundsRightObject;

    [SerializeField] private BallController ballController;

    private PaddleController leftCtrl;
    private PaddleController rightCtrl;

    public IMatchState MatchState => matchState;
    private IMatchState matchState;

    public IGameState GameState => gameState;
    private IGameState gameState;
    public PlayerSide CurrentServer => currentServer;
    private PlayerSide currentServer = PlayerSide.Left;


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

        leftCtrl = leftPlayer.GetComponent<PaddleController>();
        rightCtrl = rightPlayer.GetComponent<PaddleController>();

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
        return currentServer == PlayerSide.Left ? leftCtrl.gameObject : rightCtrl.gameObject;
    }



    private void HandleServeCompleted()
    {
        gameState = new PlayingState();
    }

    private IEnumerator HandleRoundOver()
    {
        // Invoke popup for ScoreUI here
        yield return new WaitForSeconds(3f);
        HandleRoundReset();
    }

    private void HandleRoundReset()
    {
        Debug.Log("Round Reset Started!");
        gameState = new GameOverState();
        SetServer();
        gameState = new ServingState();

        StartCoroutine(WaitForServerPaddleThenSetPosition());
    }

    private IEnumerator WaitForServerPaddleThenSetPosition()
    {
        PaddleController server = null;

        for (int i = 0; i < 5; i++)
        {
            server = GetCurrentServerPaddle()?.GetComponent<PaddleController>();
            if (server != null)
                break;

            yield return null;
        }

        if (server == null)
        {
            Debug.LogWarning("Server paddle not found after retries");
            yield break;
        }

        for (int i = 0;i < 5; i++)
        {
            if (ballController != null)
                break;

            yield return null;
        }

        if (ballController == null)
        {
            Debug.LogWarning("BallController not found during reset.");
            yield break;
        }

        ballController.PrepareForServe(server.transform.position);
    }

    private void DeterminePointWin(PlayerSide playerWhoLastHitTheBall, bool moveIsLegal)
    {
        PlayerSide opponent;

        if (playerWhoLastHitTheBall == PlayerSide.Left) opponent = PlayerSide.Right;
        else opponent = PlayerSide.Left;

        PlayerSide pointWinner;

        if (moveIsLegal)
        {
            pointWinner = playerWhoLastHitTheBall;
        }
        else
        {
            pointWinner = opponent;
        }
        
        PointWon?.Invoke(pointWinner);
        StartCoroutine(HandleRoundOver());
    }

    private void OnEnable()
    {
        HitDetector.OnServeCompleted += HandleServeCompleted;
        BallController.OnBallReset += HandleRoundReset;
        BoundsEventBus.OnBallOutOfBounds += DeterminePointWin;
    }

    private void OnDisable()
    {
        HitDetector.OnServeCompleted -= HandleServeCompleted;
        BallController.OnBallReset -= HandleRoundReset;
        BoundsEventBus.OnBallOutOfBounds -= DeterminePointWin;
    }
}
