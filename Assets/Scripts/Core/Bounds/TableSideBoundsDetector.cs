using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSideBoundsDetector : MonoBehaviour
{
    [SerializeField] private PlayerSide tableSide;

    public static event Action legalMoveMade;
    public static event Action<PlayerSide> legalMoveMadeOnSide;

    private void Awake()
    {
        if (gameObject.tag == "Table_Left") tableSide = PlayerSide.Left;
        else tableSide = PlayerSide.Right;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball") && !col.gameObject.GetComponent<BallController>().RoundOverTriggered)
        {
            BallController ball = col.gameObject.GetComponent<BallController>();

            if (ball.PlayerWhoLastHit != tableSide && !ball.IsMoveLegal)
            {
                legalMoveMade?.Invoke();
                legalMoveMadeOnSide?.Invoke(tableSide);

            }
            else if (!ball.RoundOverTriggered)
            {
                BoundsEventBus.RaiseBallOutofBounds(ball.PlayerWhoLastHit, ball.IsMoveLegal);
                BoundsEventBus.RaiseRoundOver();
            }

        }
    }
}
