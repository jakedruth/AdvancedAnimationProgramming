/*
	Advanced Animation Programming
	By Jake Ruth

    ProceduralGrab.cs - Handles IK solving a tree of bones (transforms) to grab at at a point in world space

    Credit for logic: 
        https://www.youtube.com/watch?v=RTc6i-7N3ms
        https://medium.com/unity3danimation/create-your-own-ik-in-unity3d-989debd86770
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class ProceduralGrab
    {
        public readonly Transform endBone;
        public int numAffectedParents;

        public int iterations = 10;
        public float minDelta = 0.001f;
        public delegate void OnOverExtended();
        public OnOverExtended onOverExtended;

        private float[] _boneLengths;
        private float _totalBoneLength;
        private Transform[] _bones;
        private Vector3[] _positions;
        private Vector3[] _startDirections;
        private Quaternion[] _startRotationBones;
        private Quaternion _startRotationTarget;
        private Transform _relativeRoot;

        public ProceduralGrab(Transform endBone, Transform locator, int numAffectedParents)
        {
            this.endBone = endBone;
            this.numAffectedParents = numAffectedParents;

            Init(locator.position, locator.rotation);
        }

        private void Init(Vector3 targetPosition, Quaternion targetRotation)
        {
            // Calculate the array size for all starting information
            _bones = new Transform[numAffectedParents + 1];
            _positions = new Vector3[numAffectedParents + 1];
            _boneLengths = new float[numAffectedParents];
            _startDirections = new Vector3[numAffectedParents + 1];
            _startRotationBones = new Quaternion[numAffectedParents + 1];

            // find the root bone relative to the number of parents from the end bone
            _relativeRoot = endBone;
            for (int i = 0; i <= numAffectedParents; i++)
            {
                _relativeRoot = _relativeRoot.parent;
                if (_relativeRoot == null)
                    throw new Exception("The number of parents is longer the amount of available parents");
            }

            // Get the initial rotation to the target
            _startRotationTarget = GetRotationInRootSpace(targetRotation);

            // Initialize each array based of the bones
            Transform current = endBone;
            _totalBoneLength = 0;

            // Reverse for loop because the relative root bone is index 0 while the end bone (e.g. a hand) is the last index
            for (int i = _bones.Length - 1; i >= 0; i--)
            {
                _bones[i] = current;
                _startRotationBones[i] = GetRotationInRootSpace(current.rotation);

                if (i == _bones.Length - 1) // the end bone
                {
                    _startDirections[i] =
                        GetPositionInRootSpace(targetPosition) - GetPositionInRootSpace(current.position);
                }
                else // A middle bone or the relative root bone
                {
                    _startDirections[i] = GetPositionInRootSpace(_bones[i + 1].position) -
                                          GetPositionInRootSpace(current.position);
                    _boneLengths[i] = _startDirections[i].magnitude;
                    _totalBoneLength += _boneLengths[i];
                }

                current = current.parent;
            }
        }

        public void ResolveIK(Vector3 targetPosition, Quaternion targetRotation, Vector3 constraint)
        {
            // Recalculate if the number of affected parents is changed on the fly
            if (_boneLengths.Length != numAffectedParents)
                Init(targetPosition, targetRotation);

            // Get the positions
            for (int i = 0; i < _bones.Length; i++)
            {
                _positions[i] = GetPositionInRootSpace(_bones[i].position);
            }

            // convert targets from world space to local-root space
            targetPosition = GetPositionInRootSpace(targetPosition);
            targetRotation = GetRotationInRootSpace(targetRotation);
            constraint = GetPositionInRootSpace(constraint);

            // check to see if target is in range
            bool isTooFar = (targetPosition - GetPositionInRootSpace(_bones[0].position)).sqrMagnitude >=
                            _totalBoneLength * _totalBoneLength;

            if (isTooFar)
            {
                onOverExtended?.Invoke();

                // just align all bones to face the target
                Vector3 direction = (targetPosition - _positions[0]).normalized;

                for (int i = 1; i < _positions.Length; i++)
                {
                    _positions[i] = _positions[i - 1] + direction * _boneLengths[i - 1];
                }
            }
            else // calculate IK
            {
                // Set each bone's position to it's starting position based of the starting directions
                for (int i = 0; i < _positions.Length - 1; i++)
                {
                    _positions[i + 1] = _positions[i] + _startDirections[i];
                }


                //for (int iteration = 0; iteration < iterations; iteration++)
                int count = 0;
                while (count < iterations)
                {
                    // First, Set the end bone to the target position,
                    // Then, move all bones based on their direction to the next bone and bone length
                    for (int i = _positions.Length - 1; i > 0; i--)
                    {
                        if (i == _positions.Length - 1)
                            _positions[i] = targetPosition; //set it to target
                        else
                            _positions[i] = _positions[i + 1] +
                                            (_positions[i] - _positions[i + 1]).normalized *
                                            _boneLengths[i]; //set in line on distance
                    }

                    // Starting from the relative root, move all bones back in position, keeping their direction from the previous step
                    for (int i = 1; i < _positions.Length; i++)
                        _positions[i] = _positions[i - 1] +
                                        (_positions[i] - _positions[i - 1]).normalized * _boneLengths[i - 1];

                    // Check to see if the end bone has reached the target
                    if ((_positions[_positions.Length - 1] - targetPosition).sqrMagnitude < minDelta * minDelta)
                        break;

                    count++;
                }
            }

            // Apply the constraint to each middle bone
            // Note: The end bone and the relative Root bone are not affected
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                Vector3 prev = _positions[i - 1];
                Vector3 pos = _positions[i];
                Vector3 next = _positions[i + 1];

                // Project the bone and the constraint onto a plane that is perpendicular to the direction from the previous and next bone
                // Then, rotate the projected bone around the axis based of the angle between the projected points
                Plane plane = new Plane(next - prev, prev);
                Vector3 projectedConstraint = plane.ClosestPointOnPlane(constraint);
                Vector3 projectedBone = plane.ClosestPointOnPlane(pos);
                float angle = Vector3.SignedAngle(
                    projectedBone - prev,
                    projectedConstraint - prev,
                    plane.normal);

                _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (pos - prev) + prev;
            }


            // set positions and rotations of each bone
            for (int i = 0; i < _positions.Length; i++)
            {
                if (i == _positions.Length - 1)
                {
                    SetBoneRotationInRootSpace(_bones[i],
                        Quaternion.Inverse(targetRotation) * _startRotationTarget *
                        Quaternion.Inverse(_startRotationBones[i]));
                }
                else
                {
                    SetBoneRotationInRootSpace(_bones[i],
                        Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) *
                        Quaternion.Inverse(_startRotationBones[i]));
                }

                SetBonePositionInRootSpace(_bones[i], _positions[i]);
            }
        }

        private Vector3 GetPositionInRootSpace(Vector3 position)
        {
            return Quaternion.Inverse(_relativeRoot.rotation) * (position - _relativeRoot.position);
        }

        private void SetBonePositionInRootSpace(Transform bone, Vector3 position)
        {
            bone.position = _relativeRoot.rotation * position + _relativeRoot.position;
        }

        private Quaternion GetRotationInRootSpace(Quaternion rotation)
        {
            return Quaternion.Inverse(rotation) * _relativeRoot.rotation;
        }

        private void SetBoneRotationInRootSpace(Transform bone, Quaternion rotation)
        {
            bone.rotation = _relativeRoot.rotation * rotation;
        }
    }
}
