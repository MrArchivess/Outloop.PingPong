using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    
    [SerializeField] private PaddleController paddleController;
    [SerializeField] private HitDetector hitDetector;
    [SerializeField] private PlayerInput playerInput;

    private ICommand moveCommand;

    private bool inputsSet = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        paddleController = gameObject.GetComponentInChildren<PaddleController>();
        moveCommand = new MoveCommand(paddleController);
    }


    public void SetInputs()
    {
        if (inputsSet) return;
        inputsSet = true;

        hitDetector = gameObject.GetComponentInChildren<HitDetector>();
        
        playerInput.actions["Move"].performed += ctx =>
        {
            Vector2 dir = ctx.ReadValue<Vector2>();
            moveCommand.Execute(dir);
            hitDetector.SetDirection(dir);
        };
        playerInput.actions["Move"].canceled += ctx =>
        {
            moveCommand.Execute(Vector2.zero);
            hitDetector.SetDirection(Vector2.zero);
        };

        playerInput.actions["FaceSouth"].performed += ctx =>
        {
            hitDetector.HandleFaceButton(FaceButton.A);
        };

        playerInput.actions["FaceEast"].performed += ctx =>
        {
            hitDetector.HandleFaceButton(FaceButton.B);
        };

        playerInput.actions["FaceWest"].performed += ctx =>
        {
            hitDetector.HandleFaceButton(FaceButton.X);
        };

        playerInput.actions["FaceNorth"].performed += ctx =>
        {
            hitDetector.HandleFaceButton(FaceButton.Y);
        };
    }
}
