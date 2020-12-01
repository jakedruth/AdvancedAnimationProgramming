/*
	Advanced Animation Programming
	By Jake Ruth

    ProceduralGrab.cs - Handles IK solving a tree of bones (transforms) to grab at at a point in world space
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGrab
{
    public readonly Transform endBone;
    public int numAffectedParents;

    public int iterations = 10;
    public float minDelta = 0.001f;

    private float[] _boneLengths;
    private float _totalBoneLength;
    private Transform[] _bones;
    private Vector3[] _positions;
    private Vector3[] _startDirections;
    private Quaternion[] _startRotationBones;
    private Quaternion _startRotationTarget;
    private Transform _localRoot;

    public ProceduralGrab(Transform endBone, Transform locator, int numAffectedParents)
    {
        this.endBone = endBone;
        this.numAffectedParents = numAffectedParents;
        
        Init(locator.position, locator.rotation);
    }

    private void Init(Vector3 targetPosition, Quaternion targetRotation)
    {
        _bones = new Transform[numAffectedParents + 1];
        _positions = new Vector3[numAffectedParents + 1];
        _boneLengths = new float[numAffectedParents];
        _startDirections = new Vector3[numAffectedParents + 1];
        _startRotationBones = new Quaternion[numAffectedParents + 1];

        // find the root bone relative to the number of parents from the end bone
        _localRoot = endBone;
        for (int i = 0; i <= numAffectedParents; i++)
        {
            _localRoot = _localRoot.parent;
            if (_localRoot == null)
                throw new Exception("The number of parents is longer the amount of available parents");
        }

        // Get the initial rotation to the target
        _startRotationTarget = GetRotationInRootSpace(targetRotation);

        Transform current = endBone;
        _totalBoneLength = 0;
        for (int i = _bones.Length - 1; i >= 0; i--)
        {
            _bones[i] = current;
            _startRotationBones[i] = GetRotationInRootSpace(current.rotation);

            if (i == _bones.Length - 1) // the end bone
            {
                _startDirections[i] = GetPositionInRootSpace(targetPosition) - GetPositionInRootSpace(current.position);
            }
            else // A middle bone
            {
                _startDirections[i] = GetPositionInRootSpace(_bones[i + 1].position) - GetPositionInRootSpace(current.position);
                _boneLengths[i] = _startDirections[i].magnitude;
                _totalBoneLength += _boneLengths[i];
            }

            current = current.parent;
        }
    }

    public void ResolveIK(Vector3 targetPosition, Quaternion targetRotation, Vector3 constraint)
    {
        if (_boneLengths.Length != numAffectedParents)
            Init(targetPosition, targetRotation);

        // Get the positions
        for (int i = 0; i < _bones.Length; i++)
        {
            _positions[i] = GetPositionInRootSpace(_bones[i].position);
        }

        // convert targets from world space to local root space
        targetPosition = GetPositionInRootSpace(targetPosition);
        targetRotation = GetRotationInRootSpace(targetRotation);
        constraint = GetPositionInRootSpace(constraint);

        // check to see if target is in range
        bool isTooFar = (targetPosition - GetPositionInRootSpace(_bones[0].position)).sqrMagnitude >= _totalBoneLength * _totalBoneLength;

        if (isTooFar)
        {
            Vector3 direction = (targetPosition - _positions[0]).normalized;

            for (int i = 1; i < _positions.Length; i++)
            {
                _positions[i] = _positions[i - 1] + direction * _boneLengths[i - 1];
            }
        }
        else // calculate IK
        {
            for (int i = 0; i < _positions.Length - 1; i++)
            {
                _positions[i + 1] = _positions[i] + _startDirections[i];
            }

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                //back
                for (int i = _positions.Length - 1; i > 0; i--)
                {
                    if (i == _positions.Length - 1)
                        _positions[i] = targetPosition; //set it to target
                    else
                        _positions[i] = _positions[i + 1] + (_positions[i] - _positions[i + 1]).normalized * _boneLengths[i]; //set in line on distance
                }

                //forward
                for (int i = 1; i < _positions.Length; i++)
                    _positions[i] = _positions[i - 1] + (_positions[i] - _positions[i - 1]).normalized * _boneLengths[i - 1];

                //close enough?
                if ((_positions[_positions.Length - 1] - targetPosition).sqrMagnitude < minDelta * minDelta)
                    break;
            }
        }

        // Apply the constraint
        for (int i = 1; i < _positions.Length - 1; i++)
        {
            Vector3 prev = _positions[i - 1];
            Vector3 pos = _positions[i];
            Vector3 next = _positions[i + 1];

            Vector3 planeNormal = _positions[i + 1] - _positions[i - 1];
            Vector3 projectedConstraint = Vector3.ProjectOnPlane(constraint, planeNormal);
            Vector3 projectedBone = Vector3.ProjectOnPlane(_positions[i], planeNormal);
            float angle = Vector3.SignedAngle(
                projectedBone - _positions[i - 1], 
                projectedConstraint - _positions[i - 1],
                planeNormal);

            _positions[i] = Quaternion.AngleAxis(angle, planeNormal) * (_positions[i] - _positions[i - 1]) +
                           _positions[i - 1];
        }


        // set positions and rotations
        for (int i = 0; i < _positions.Length; i++)
        {
            if (i == _positions.Length - 1)
            {
                SetBoneRotationInRootSpace(_bones[i], Quaternion.Inverse(targetRotation) * _startRotationTarget * Quaternion.Inverse(_startRotationBones[i]));
            }
            else
            {
                SetBoneRotationInRootSpace(_bones[i], Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotationBones[i]));
            }

            SetBonePositionInRootSpace(_bones[i], _positions[i]);
        }
    }

    private Vector3 GetPositionInRootSpace(Vector3 position)
    {
        return Quaternion.Inverse(_localRoot.rotation) * (position - _localRoot.position);
    }

    private void SetBonePositionInRootSpace(Transform bone, Vector3 position)
    {
        bone.position = _localRoot.rotation * position + _localRoot.position;
    }

    private Quaternion GetRotationInRootSpace(Quaternion rotation)
    {
        return Quaternion.Inverse(rotation) * _localRoot.rotation;
    }

    private void SetBoneRotationInRootSpace(Transform bone, Quaternion rotation)
    {
        bone.rotation = _localRoot.rotation * rotation;
    }
}
