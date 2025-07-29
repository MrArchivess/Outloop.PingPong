using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject paddlePrefab;

    [SerializeField] private GameObject boundsLeftObject;
    [SerializeField] private GameObject boundsRightObject;

    private void Start()
    {
        SpawnPaddles();
    }

    private void SpawnPaddles()
    {
        Bounds leftBounds = boundsLeftObject.GetComponent<BoxCollider>().bounds;
        Bounds rightBounds = boundsRightObject.GetComponent<BoxCollider>().bounds;

        Vector3 leftSpawn = leftBounds.center;
        Vector3 rightSpawn = rightBounds.center;


        GameObject leftPlayer = Instantiate(paddlePrefab, leftSpawn, paddlePrefab.transform.rotation);
        GameObject rightPlayer = Instantiate(paddlePrefab, rightSpawn, paddlePrefab.transform.rotation * Quaternion.Euler(0, 180, 0));

        PaddleController leftCtrl = leftPlayer.GetComponent<PaddleController>();
        PaddleController rightCtrl = rightPlayer.GetComponent<PaddleController>();

        leftCtrl.SetMovementBounds(leftBounds);
        rightCtrl.SetMovementBounds(rightBounds);

        leftCtrl.SetSide(PlayerSide.Left);
        rightCtrl.SetSide(PlayerSide.Right);

        leftCtrl.SetHitDetector();
        rightCtrl.SetHitDetector();

        InputHandler leftInputHandler = gameObject.AddComponent<InputHandler>();
        leftInputHandler.Initialize(leftCtrl, 1);
        InputHandler rightInputHandler = gameObject.AddComponent<InputHandler>();
        rightInputHandler.Initialize(rightCtrl, 2);

    }
}
