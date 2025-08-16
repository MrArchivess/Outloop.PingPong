using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    Camera _cam;

    private void Awake() => _cam = Camera.main;

    private void LateUpdate()
    {
        if (!_cam) return;
        transform.forward = _cam.transform.forward;
    }

}