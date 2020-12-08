using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _cam;

    public Transform target;
    public float cameraMoveSpeed;
    public float rotateSpeed;
    public float minOffset;
    
    private Vector3 _targetOffset;
    private float _maxSqrDistance;

    void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        _targetOffset = transform.position - target.position;
        _maxSqrDistance = _targetOffset.sqrMagnitude;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 displacement = target.position - transform.position;
        float sqrDist = displacement.sqrMagnitude;
        Vector3 targetPos = target.TransformPoint(_targetOffset);
        if (Mathf.Abs(_maxSqrDistance - sqrDist) > minOffset)
        {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, cameraMoveSpeed * Time.deltaTime);
        }

        _cam.transform.LookAt(target, transform.up);
    }
}
