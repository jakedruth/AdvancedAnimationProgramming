/*
	Advanced Animation Programming
	By Jake Ruth

    IKTail.cs - Script that is used to control the spider's tail
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class IKTail : MonoBehaviour
    {
        public int parentCount = 2;

        public Transform rootSpace;
        public Transform locator;
        public Transform restTransform;

        private ProceduralGrab _proceduralGrab;
        private float _height;

        private void Awake()
        {
            _proceduralGrab = new ProceduralGrab(transform, locator, parentCount);
            _height = restTransform.localPosition.y;
        }

        void Update()
        {
            // Find the position above the ground that the tail can rest at
            Ray ray = new Ray(restTransform.position + restTransform.up, -restTransform.up);
            Vector3 pos = restTransform.position;
            Quaternion targetRot = locator.rotation;
            bool rayHit = Physics.Raycast(ray, out RaycastHit hit);
            if (rayHit)
            {
                pos = hit.point - ray.direction * _height;
                targetRot = MathAA.GetRotationFromRaycastHitAndForward(hit, restTransform.forward);
            }

            //Debug.DrawLine(ray.origin, ray.GetPoint(rayHit ? hit.distance : 2f), Color.red);

            // move the locator 
            locator.position = Vector3.MoveTowards(locator.position, pos, 1.0f * Time.deltaTime);
            locator.rotation = Quaternion.RotateTowards(locator.rotation, targetRot, 90f * Time.deltaTime);

            _proceduralGrab.ResolveIK(locator.position, locator.rotation, locator.position + rootSpace.up);
        }
    }
}