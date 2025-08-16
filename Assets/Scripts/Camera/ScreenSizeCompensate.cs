using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ScreenSizeCompensate : MonoBehaviour
{
    [SerializeField] private float referenceSizeAt1m = 0.25f;
    [SerializeField] private Vector2 scaleClamp = new Vector2(0.5f, 2.5f);

    Camera _cam;
    Vector3 _baseScale;

    private void Awake()
    {
        _cam = Camera.main;
        _baseScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (!_cam) return;
        if (_cam.orthographic) return;

        float dist = Vector3.Distance(_cam.transform.position, transform.position);
        float s = Mathf.Clamp(referenceSizeAt1m * Mathf.Max(0.0001f, dist), scaleClamp.x, scaleClamp.y);
        transform.localScale = _baseScale * s;
    }
}
