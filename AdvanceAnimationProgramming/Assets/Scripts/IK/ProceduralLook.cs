/*
	Advanced Animation Programming
	By Jake Ruth

    ProceduralLook.cs - Handles rotating a bone (transform) to look at a point in world space
*/

using System;
using UnityEngine;

namespace AdvAnimation
{
    public class ProceduralLook
    {
        public readonly Transform lookBone;
        private readonly Quaternion _startRotation;

        public ProceduralLook(Transform lookBone)
        {
            if (lookBone == null)
                throw new Exception("Look bone is null");

            // get a reference to the bone that will be rotating
            this.lookBone = lookBone;

            // get it's starting rotation
            _startRotation = lookBone.rotation;
        }

        public void LookAt(Vector3 point)
        {
            //Debug.DrawLine(lookBone.position, point);

            // Get the look Direction
            Vector3 lookDisplacement = point - lookBone.position;
            Vector3 lookDir = lookDisplacement.normalized;

            // set the bone's rotation
            lookBone.rotation = Quaternion.LookRotation(lookDir, Vector3.up) * _startRotation;
        }
    }
}