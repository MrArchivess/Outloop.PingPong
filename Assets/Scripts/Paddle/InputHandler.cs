using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private ICommand moveCommand;

    private void Start()
    {
        var paddle = FindObjectOfType<PaddleControler>();
        moveCommand = new MoveCommand(paddle);
    }

    private void Update()
    {
        // Keyboard: maps A/D or Arrow Keys, returns -1, 0, 1
        float input = Input.GetAxis("Horizontal");

        // Gamepad joystick also uses "Horizontal" by default
        moveCommand.Execute(input);
    }
    
}
