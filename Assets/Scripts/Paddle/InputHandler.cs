using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerControls controls;

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

    private void OnEnable()
    {
        PaddleController paddleController = gameObject.GetComponentInChildren<PaddleController>();
        HitDetector hitDetector = gameObject.GetComponentInChildren<HitDetector>();

        controls.Player.Enable();
        controls.Player.Hit.performed += ctx => hitDetector.HandleHitButton();
        controls.Player.Move.performed += ctx => paddleController.SetDirection(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => paddleController.SetDirection(Vector2.zero);

    }

    private void OnDisable()
    {
        controls.Player.Disable();
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
