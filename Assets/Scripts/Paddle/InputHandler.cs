using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private ICommand moveCommand;

    public void Initialize(PaddleController paddle)
    {

        moveCommand = new MoveCommand(paddle);
    }

    private void Update()
    {
        if (moveCommand == null) return;

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        moveCommand.Execute(input);

    }
    
}
