using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static event Action<PlayerSide> scoreUpdated;
    public static event Action<PlayerSide> winnerAnnounced;

    public int ScoreLeft => scoreLeft;
    private int scoreLeft;

    public int ScoreRight => scoreRight;
    private int scoreRight;

    private int winCondition = 5;

    private void AddPoint(PlayerSide side)
    {
        if (side == PlayerSide.Left)
        {
            scoreLeft++;

        }
            
        else if (side == PlayerSide.Right)
        {
            Debug.Log("Point for the Right");
            scoreRight++;
        }
        scoreUpdated?.Invoke(side);
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (scoreLeft >= winCondition && scoreLeft - scoreRight >= 2) winnerAnnounced?.Invoke(PlayerSide.Left);
        else if (scoreRight >= winCondition && (scoreRight - scoreLeft >= 2)) winnerAnnounced?.Invoke(PlayerSide.Right);
    }

    private void OnEnable()
    {
        GameManager.PointWon += AddPoint;
    }

    private void OnDisable()
    {
        GameManager.PointWon -= AddPoint;
    }

}
