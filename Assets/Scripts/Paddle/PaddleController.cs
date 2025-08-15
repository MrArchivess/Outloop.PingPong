using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private BoxCollider tableCollider;

    public PlayerSide Playerside => playerSide;
    [SerializeField] private PlayerSide playerSide;

    private Quaternion baseLocalRot;

    private Vector2 inputDirection = Vector2.zero;
    private Bounds movementBounds;

    private void Awake()
    {
        baseLocalRot = transform.localRotation;
    }

    public void SetMovementBounds(Bounds bounds)
    {
        movementBounds = bounds;
    }

    public void SetSide(PlayerSide side)
    {
        playerSide = side;
    }

    public void SetPlayerRotation(PlayerSide side)
    {
        transform.localRotation = baseLocalRot * (side == PlayerSide.Right
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.identity);
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

    private void LogFacing(string tag)
    {
        float yaw = transform.eulerAngles.y;
        Vector3 fwd = transform.forward;
        //Debug.Log($"[{name}] {tag}  yaw={yaw:0.0}  forward={fwd}");
    }

    private void Update()
    { 
            Vector3 move = new Vector3(inputDirection.x, 0, inputDirection.y);
        if (GameManager.Instance.MatchState is MatchActiveState) transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
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
