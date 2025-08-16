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
    public static event Action<GameObject> OnServerDetermined;
    public static event Action<PlayerSide> PointWon;
    public static event Action OnMatchReset;
    public static event Action OnPlayerConnected;

    public static event Action OnMatchReady;
    public static event Action OnMatchGo;
    public static event Action OnMatchStarted;
    public static event Action<PlayerSide> OnMatchDone;

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
    [SerializeField] private PlayerSide currentServer = PlayerSide.Left;

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
        yield return null; // let PlayerInput finish spawning/parenting
        var init = input.GetComponent<PlayerInitializer>();
        var bounds = (input.playerIndex == 0)
            ? boundsLeftObject.GetComponent<BoxCollider>().bounds
            : boundsRightObject.GetComponent<BoxCollider>().bounds;

        init.InitializeCore(input.playerIndex == 0 ? PlayerSide.Left : PlayerSide.Right, bounds);
        init.SetBounds(boundsLeftObject.transform, boundsRightObject.transform);

        OnPlayerConnected?.Invoke();
        CheckMatchStart();
    }

    private void CheckMatchStart()
    {
        if (PlayerInputManager.instance == null) return;
        if (PlayerInputManager.instance.playerCount < 2) return;

        StartCoroutine(StartMatch());
    }

    private IEnumerator StartMatch()
    {
        OnMatchReady?.Invoke();
        yield return new WaitForSeconds(1f);
        OnMatchGo?.Invoke();
        yield return new WaitForSeconds(.5f);
        OnMatchStarted?.Invoke();
        matchState = new MatchActiveState();

    }

    private void EndMatch(PlayerSide player)
    {
        Debug.Log("Ending match");
        matchState = new MatchOverState();
        OnMatchDone?.Invoke(player);
    }

    private void ResetMatch()
    {
        Debug.Log("Resetting Match");
        StartCoroutine(ResetMatchSequence());
    }

    private IEnumerator ResetMatchSequence()
    {
        matchState = new MatchReadyState();
        gameState = new ServingState();
        currentServer = PlayerSide.Left;
        currentServed = 0;
        OnMatchReset?.Invoke();
        StartCoroutine(WaitForServerPaddleThenSetPosition());
        yield return new WaitForSeconds(1f);
        CheckMatchStart();
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

        for (int i = 0; i < 5; i++)
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
        OnServerDetermined?.Invoke(server.gameObject);
        //tell worldfollow that it needs to follow server
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
        MatchFlowUIController.MatchFinished += ResetMatch;
    }

    private void OnDisable()
    {
        HitDetector.OnServeCompleted -= HandleServeCompleted;
        BallController.OnBallReset -= HandleRoundReset;
        BoundsEventBus.OnBallOutOfBounds -= DeterminePointWin;
        ScoreSystem.WinnerAnnounced -= EndMatch;
        MatchFlowUIController.MatchFinished -= ResetMatch;
    }
}