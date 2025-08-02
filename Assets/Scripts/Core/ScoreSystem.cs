using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private int scoreLeft;
    private int scoreRight;

    private int winCondition = 5;

    private void AddPoint(PlayerSide side)
    {
        if (side == PlayerSide.Left)
        {
            Debug.Log("Point for the Left");
            scoreLeft++; 
        }
            
        else if (side == PlayerSide.Right)
        {
            Debug.Log("Point for the Right");
            scoreRight++;
        }
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (scoreLeft >= winCondition && scoreLeft - scoreRight >= 2)
        {
                Debug.Log("Left is the winner");
        }
        else if (scoreRight >= winCondition && (scoreRight - scoreLeft >= 2))
        {
                Debug.Log("Right is the winner");
        }
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
