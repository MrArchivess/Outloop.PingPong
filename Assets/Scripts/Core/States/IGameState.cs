using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameState
{
    public void StateEnter();
    public void StateExit();
    public void HandleState();
}
