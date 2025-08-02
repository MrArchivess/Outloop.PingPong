using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallOutOfBoundsDetector : MonoBehaviour
{
    public static event Action<PlayerSide, bool> ballOutOfBounds;
    
    public static event Action roundOver;

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            BallController ball = col.gameObject.GetComponent<BallController>();
            if (!ball.RoundOverTriggered)
            {
                Debug.Log("Round Over");

                BoundsEventBus.RaiseBallOutofBounds(ball.PlayerWhoLastHit, ball.IsMoveLegal);
                BoundsEventBus.RaiseRoundOver();
                /*                ballOutOfBounds?.Invoke(ball.PlayerWhoLastHit, ball.IsMoveLegal);
                                roundOver?.Invoke();*/
            }
        }
    }
}
