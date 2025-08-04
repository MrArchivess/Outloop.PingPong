using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMatchState
{
    public void EnterState();

    public void ExitState();

    public void HandleState();
}
