/*
	Advanced Animation Programming
	By Jake Ruth

    CameraController.cs - A simple camera controller that follows a target and 
    attempts to align its orientation to match the target's
*/

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
        // Get the displacement from the target
        Vector3 displacement = target.position - transform.position;
        float sqrDist = displacement.sqrMagnitude;

        // Check to see if the camera is too faraway from the target
        if (Mathf.Abs(_maxSqrDistance - sqrDist) > minOffset)
        {
            // calculate a new position and rotation
            Vector3 targetPos = target.TransformPoint(_targetOffset);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, cameraMoveSpeed * Time.deltaTime);
        }

        // Make sure the camera is looking at the target
        _cam.transform.LookAt(target, transform.up);
    }
}
