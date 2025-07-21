using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    private readonly PaddleControler _paddle;

    public MoveCommand(PaddleControler paddle)
    {
        _paddle = paddle;
    }

    public void Execute(float inputValue)
    {
        _paddle.SetDirection(inputValue);
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}
