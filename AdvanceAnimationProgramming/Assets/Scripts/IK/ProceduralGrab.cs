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

    private float[] boneLengths;
    private float totalBoneLength;
    private Transform[] bones;
    private Vector3[] positions;
    private Vector3[] startDirections;
    private Quaternion[] startRotationBones;
    private Quaternion startRotationTarget;
    private Transform localRoot;

    public ProceduralGrab(Transform endBone, Transform locator, int numAffectedParents)
    {
        this.endBone = endBone;
        this.numAffectedParents = numAffectedParents;
        
        Init(locator.position, locator.rotation);
    }

    private void Init(Vector3 targetPosition, Quaternion targetRotation)
    {
        bones = new Transform[numAffectedParents + 1];
        positions = new Vector3[numAffectedParents + 1];
        boneLengths = new float[numAffectedParents];
        startDirections = new Vector3[numAffectedParents + 1];
        startRotationBones = new Quaternion[numAffectedParents + 1];

        // find the root bone relative to the number of parents from the end bone
        localRoot = endBone;
        for (int i = 0; i <= numAffectedParents; i++)
        {
            localRoot = localRoot.parent;
            if (localRoot == null)
                throw new Exception("The number of parents is longer the amount of available parents");
        }

        // Get the initial rotation to the target
        startRotationTarget = GetRotationInRootSpace(targetRotation);

        Transform current = endBone;
        totalBoneLength = 0;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotationBones[i] = GetRotationInRootSpace(current.rotation);

            if (i == bones.Length - 1) // the end bone
            {
                startDirections[i] = GetPositionInRootSpace(targetPosition) - GetPositionInRootSpace(current.position);
            }
            else // A middle bone
            {
                startDirections[i] = GetPositionInRootSpace(bones[i + 1].position) - GetPositionInRootSpace(current.position);
                boneLengths[i] = startDirections[i].magnitude;
                totalBoneLength += boneLengths[i];
            }

            current = current.parent;
        }
    }

    public void ResolveIK(Vector3 targetPosition, Quaternion targetRotation, Vector3 constraint, float snapBackStrength)
    {
        if (boneLengths.Length != numAffectedParents)
            Init(targetPosition, targetRotation);

        // Get the positions
        for (int i = 0; i < bones.Length; i++)
        {
            positions[i] = GetPositionInRootSpace(bones[i].position);
        }

        // convert targets from world space to local root space
        targetPosition = GetPositionInRootSpace(targetPosition);
        targetRotation = GetRotationInRootSpace(targetRotation);
        constraint = GetPositionInRootSpace(constraint);

        // check to see if target is in range
        bool isTooFar = (targetPosition - GetPositionInRootSpace(bones[0].position)).sqrMagnitude >= totalBoneLength * totalBoneLength;

        if (isTooFar)
        {
            Vector3 direction = (targetPosition - positions[0]).normalized;

            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * boneLengths[i - 1];
            }
        }
        else // calculate IK
        {
            for (int i = 0; i < positions.Length - 1; i++)
                positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + startDirections[i], snapBackStrength);

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                //back
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                        positions[i] = targetPosition; //set it to target
                    else
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * boneLengths[i]; //set in line on distance
                }

                //forward
                for (int i = 1; i < positions.Length; i++)
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * boneLengths[i - 1];

                //close enough?
                if ((positions[positions.Length - 1] - targetPosition).sqrMagnitude < minDelta * minDelta)
                    break;
            }
        }

        // Apply the constraint
        for (int i = 1; i < positions.Length - 1; i++)
        {
            Vector3 prev = positions[i - 1];
            Vector3 pos = positions[i];
            Vector3 next = positions[i + 1];

            Vector3 planeNormal = positions[i + 1] - positions[i - 1];
            Vector3 projectedConstraint = Vector3.ProjectOnPlane(constraint, planeNormal);
            Vector3 projectedBone = Vector3.ProjectOnPlane(positions[i], planeNormal);
            float angle = Vector3.SignedAngle(
                projectedBone - positions[i - 1], 
                projectedConstraint - positions[i - 1],
                planeNormal);

            positions[i] = Quaternion.AngleAxis(angle, planeNormal) * (positions[i] - positions[i - 1]) +
                           positions[i - 1];
        }


        // set positions and rotations
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                SetBoneRotationInRootSpace(bones[i], Quaternion.Inverse(targetRotation) * startRotationTarget * Quaternion.Inverse(startRotationBones[i]));
            }
            else
            {
                SetBoneRotationInRootSpace(bones[i], Quaternion.FromToRotation(startDirections[i], positions[i + 1] - positions[i]) * Quaternion.Inverse(startRotationBones[i]));
            }

            SetBonePositionInRootSpace(bones[i], positions[i]);
        }
    }

    private Vector3 GetPositionInRootSpace(Vector3 position)
    {
        return Quaternion.Inverse(localRoot.rotation) * (position - localRoot.position);
    }

    private void SetBonePositionInRootSpace(Transform bone, Vector3 position)
    {
        bone.position = localRoot.rotation * position + localRoot.position;
    }

    private Quaternion GetRotationInRootSpace(Quaternion rotation)
    {
        return Quaternion.Inverse(rotation) * localRoot.rotation;
    }

    private void SetBoneRotationInRootSpace(Transform bone, Quaternion rotation)
    {
        bone.rotation = localRoot.rotation * rotation;
    }
}
