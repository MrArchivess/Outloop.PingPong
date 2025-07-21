using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleControler : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;

    private float inputAxis = 0f;

    public void SetDirection(float axis)
    {
        inputAxis = Mathf.Clamp(axis, -1f, 1f);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * inputAxis * moveSpeed * Time.deltaTime, Space.World);
    }
}
