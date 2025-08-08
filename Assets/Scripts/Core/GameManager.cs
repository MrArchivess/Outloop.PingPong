using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<PlayerSide> OnServerChanged;
    public static event Action<PlayerSide> PointWon;
    public static event Action OnMatchReset;
    public static event Action OnPlayerConnected;

    [SerializeField] private GameObject paddlePrefab;

    [SerializeField] private GameObject boundsLeftObject;
    [SerializeField] private GameObject boundsRightObject;

    [SerializeField] private BallController ballController;

    [SerializeField] private PlayerControls playerControls;

    private PaddleController leftCtrl;
    private PaddleController rightCtrl;

    public IMatchState MatchState => matchState;
    private IMatchState matchState;

    public IGameState GameState => gameState;
    private IGameState gameState;
    public PlayerSide CurrentServer => currentServer;
    [SerializeField]private PlayerSide currentServer = PlayerSide.Left;

    private int maxServe = 2;
    private int currentServed = 0;


    public void RegisterPaddle(PlayerSide side, PaddleController controller)
    {
        if (side == PlayerSide.Left)
            leftCtrl = controller;
        else if (side == PlayerSide.Right)
            rightCtrl = controller;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
            Instance = this;

        gameState = new ServingState();
        matchState = new MatchReadyState();
     
        StartCoroutine(StartMatch());
    }

    private void Start()
    {
        StartCoroutine(RegisterPlayerJoinHandler());
    }

    private IEnumerator RegisterPlayerJoinHandler()
    {
        while (PlayerInputManager.instance == null)
            yield return null;

        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput input)
    {
        StartCoroutine(InitPlayerNextFrame(input));
        Debug.Log($" Player joined: index={input.playerIndex}, device(s): {string.Join(", ", input.devices)}");
    }

    private IEnumerator InitPlayerNextFrame(PlayerInput input)
    {
        yield return null;
        var initializer = input.GetComponent<PlayerInitializer>();

        if (input.playerIndex == 0)
            initializer.InitializePlayer(PlayerSide.Left, boundsLeftObject.GetComponent<BoxCollider>().bounds);
        else
            initializer.InitializePlayer(PlayerSide.Right, boundsRightObject.GetComponent<BoxCollider>().bounds);

        initializer.SetBounds(boundsLeftObject.transform, boundsRightObject.transform);
        OnPlayerConnected.Invoke();
    }

    private IEnumerator StartMatch()
    {
        yield return new WaitForSeconds(3f);
        matchState = new MatchActiveState();
    }

    private void EndMatch(PlayerSide player)
    {
        StartCoroutine(EndMatch());
    }

    private IEnumerator EndMatch()
    {
        matchState = new MatchOverState();
        yield return new WaitForSeconds(3f);
        StartCoroutine(ResetMatch());
    }

    private IEnumerator ResetMatch()
    {
        matchState = new MatchReadyState();
        gameState = new ServingState();
        currentServer = PlayerSide.Left;
        currentServed = 0;
        OnMatchReset.Invoke();
        yield return new WaitForSeconds(1f);
        StartCoroutine(StartMatch());
    }

    private void SetServer()
    {
        if (currentServer == PlayerSide.Left)
            currentServer = PlayerSide.Right;
        else
            currentServer = PlayerSide.Left;
        currentServed = 0;
        OnServerChanged?.Invoke(currentServer);
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
        currentServed++;
        if (currentServed == maxServe) SetServer();
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
        ScoreSystem.WinnerAnnounced += EndMatch;
    }

    private void OnDisable()
    {
        HitDetector.OnServeCompleted -= HandleServeCompleted;
        BallController.OnBallReset -= HandleRoundReset;
        BoundsEventBus.OnBallOutOfBounds -= DeterminePointWin;
        ScoreSystem.WinnerAnnounced -= EndMatch;
    }
}
