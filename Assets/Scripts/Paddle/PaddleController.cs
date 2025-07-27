using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private BoxCollider tableCollider;
    [SerializeField] private PlayerSide playerSide;

    private Vector2 inputDirection = Vector2.zero;
    private Bounds bounds;

    private void Start()
    {
        bounds = tableCollider.bounds;
    }

    public void SetSide(PlayerSide side)
    {
        playerSide = side;
    }

    public void SetTable(BoxCollider table)
    {
        tableCollider = table;
        bounds = table.bounds;
    }

    public void SetDirection(Vector2 input)
    {
        inputDirection = Vector2.ClampMagnitude(input, 1f);
    }
    private void Update()
    {
        Vector3 move = new Vector3(inputDirection.x, 0, inputDirection.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        ClampPosition();
    }

    void ClampPosition()
    {
        Bounds bounds = tableCollider.bounds;

        Vector3 clamped = transform.position;


        float halfWidth = transform.localScale.x / 150f;
        float halfDepth = transform.localScale.z / 150f;

        clamped.x = Mathf.Clamp(
            clamped.x,
            bounds.min.x + halfWidth,
            bounds.max.x - halfWidth
        );

        float zMin, zMax;

        float padding = 0.5f;

        if (playerSide == PlayerSide.Left)
        {
            zMin = bounds.min.z - padding;
            zMax = bounds.center.z;
        }
        else
        {
            zMin = bounds.center.z;
            zMax = bounds.max.z + padding;
        }

        clamped.z = Mathf.Clamp(clamped.z, zMin, zMax);

        transform.position = clamped;
    }

}
