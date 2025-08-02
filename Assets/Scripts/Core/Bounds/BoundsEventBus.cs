using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsEventBus
{
    public static event Action<PlayerSide, bool> OnBallOutOfBounds;
    public static event Action OnRoundOver;

    public static void RaiseBallOutofBounds(PlayerSide lastHitter, bool isLegal)
    {
        OnBallOutOfBounds?.Invoke(lastHitter, isLegal);
    }

    public static void RaiseRoundOver()
    {
        OnRoundOver?.Invoke(); 
    }
}
