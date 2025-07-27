using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    private readonly PaddleController _paddle;

    public MoveCommand(PaddleController paddle)
    {
        _paddle = paddle;
    }

    public void Execute(Vector2 inputValue)
    {
        _paddle.SetDirection(inputValue);
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}
