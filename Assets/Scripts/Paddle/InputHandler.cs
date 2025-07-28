using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private ICommand moveCommand;
    [SerializeField] public int playerIndex;

    public void Initialize(PaddleController paddle, int index)
    {

        moveCommand = new MoveCommand(paddle);
        playerIndex = index;
    }

    private void Update()
    {
        if (moveCommand == null) return;

        Vector2 input = GetPlayerInput(playerIndex);
        moveCommand.Execute(input);
    }

    private Vector2 GetPlayerInput(int index)
    {
        if (playerIndex == 1)
        {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        else if (playerIndex == 2)
        {
            return new Vector2(Input.GetAxis("Horizontal_P2"), Input.GetAxis("Vertical_P2"));
        }

        return Vector2.zero;
    }
    
}
