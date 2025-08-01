using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private BoxCollider tableCollider;

    public PlayerSide Playerside => playerSide;
    private PlayerSide playerSide;

    private Vector2 inputDirection = Vector2.zero;
    private Bounds movementBounds;

    public void SetMovementBounds(Bounds bounds)
    {
        movementBounds = bounds;
    }

    public void SetSide(PlayerSide side)
    {
        playerSide = side;
    }

    public void SetHitDetector()
    {
        HitDetector detector = FindHitDetector().AddComponent<HitDetector>();
        detector.Initialize(playerSide);

        Renderer[] glowTarget = GetComponentsInChildren<Renderer>();
        detector.SetGlowRenderers(glowTarget);

    }

    public void SetDirection(Vector2 input)
    {
        inputDirection = Vector2.ClampMagnitude(input, 1f);
    }

    private GameObject FindHitDetector()
    {
        foreach (Transform transform in transform.GetComponentInChildren<Transform>())
        {
            if (transform.gameObject.CompareTag("HitDetector")) return transform.gameObject;
        }
        return null;
    }

    private void Update()
    {
            Vector3 move = new Vector3(inputDirection.x, 0, inputDirection.y);
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
            ClampPosition();
    }

    private void ClampPosition()
    {
        Vector3 clamped = transform.position;

        float halfWidth = transform.localScale.x / 2f;
        float halfDepth = transform.localScale.z / 2f;

        clamped.x = Mathf.Clamp(
            clamped.x,
            movementBounds.min.x + halfWidth,
            movementBounds.max.x - halfWidth
        );

        clamped.z = Mathf.Clamp(clamped.z,
            movementBounds.min.z + halfDepth,
            movementBounds.max.z - halfDepth);

        transform.position = clamped;
    }

}
