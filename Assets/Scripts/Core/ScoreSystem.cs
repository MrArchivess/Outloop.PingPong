using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    public static event Action<PlayerSide> ScoreUpdated;
    public static event Action<PlayerSide> WinnerAnnounced;

    public int ScoreLeft => scoreLeft;
    private int scoreLeft;

    public int ScoreRight => scoreRight;
    private int scoreRight;

    public int WinCondition => winCondition;
    private int winCondition = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
            Instance = this;
    }

    private void AddPoint(PlayerSide side)
    {
        ScoreUpdated?.Invoke(side);
        if (side == PlayerSide.Left)
        {
            scoreLeft++;

        }
            
        else if (side == PlayerSide.Right)
        {
            Debug.Log("Point for the Right");
            scoreRight++;
        }
    }

    private void CheckWinCondition()
    {
        Debug.Log("Checking Win Condition");
        if (scoreLeft >= winCondition && scoreLeft - scoreRight >= 2) WinnerAnnounced?.Invoke(PlayerSide.Left);
        else if (scoreRight >= winCondition && (scoreRight - scoreLeft >= 2)) WinnerAnnounced?.Invoke(PlayerSide.Right);
    }

    private void ResetScore()
    {
        scoreLeft = 0;
        scoreRight = 0;
    }

    private void OnEnable()
    {
        GameManager.PointWon += AddPoint;
        GameManager.OnMatchReset += ResetScore;
        MatchFlowUIController.ScoreFlashFinished += CheckWinCondition;
    }

    private void OnDisable()
    {
        GameManager.PointWon -= AddPoint;
        GameManager.OnMatchReset -= ResetScore;
        MatchFlowUIController.ScoreFlashFinished -= CheckWinCondition;
    }

}
