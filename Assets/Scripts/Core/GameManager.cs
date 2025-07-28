using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject paddlePrefab;
    [SerializeField] private BoxCollider tableCollider;

    private void Start()
    {
        SpawnPaddles();
    }

    private void SpawnPaddles()
    {
        Bounds bounds = tableCollider.bounds;
        float tableSurfaceY = bounds.max.y + 0.25f;

        Vector3 leftPos = new Vector3(bounds.center.x,tableSurfaceY, bounds.min.z - 0.5f);
        Vector3 rightPos = new Vector3(bounds.center.x, tableSurfaceY, bounds.max.z + 0.5f);

        GameObject leftPaddle = Instantiate(paddlePrefab, leftPos, paddlePrefab.transform.rotation);
        GameObject rightPaddle = Instantiate(paddlePrefab, rightPos, paddlePrefab.transform.rotation * Quaternion.Euler(0, 180, 0));

        PaddleController leftCtrl = leftPaddle.GetComponent<PaddleController>();
        PaddleController rightCtrl = rightPaddle.GetComponent<PaddleController>();

        leftCtrl.SetSide(PlayerSide.Left);
        rightCtrl.SetSide(PlayerSide.Right);

        leftCtrl.SetTable(tableCollider);
        rightCtrl.SetTable(tableCollider);

        leftCtrl.SetHitDetector();
        rightCtrl.SetHitDetector();

        InputHandler leftInputHandler = gameObject.AddComponent<InputHandler>();
        leftInputHandler.Initialize(leftCtrl, 1);
        InputHandler rightInputHandler = gameObject.AddComponent<InputHandler>();
        rightInputHandler.Initialize(rightCtrl, 2);
        
    }
}
