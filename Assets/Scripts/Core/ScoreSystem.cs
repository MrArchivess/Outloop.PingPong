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
        if (side == PlayerSide.Left) scoreLeft++;
        else if (side == PlayerSide.Right) scoreRight++;
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
}
