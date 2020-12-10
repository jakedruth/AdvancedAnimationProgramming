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
            Ray ray = new Ray(restTransform.position + restTransform.up, -restTransform.up);
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
            Vector3 pos = restTransform.position;
            Quaternion targetRot = locator.rotation;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                pos = hit.point - ray.direction * _height;
                targetRot = MathAA.GetRotationFromRaycastHitAndForward(hit, restTransform.forward);
            }

            locator.position = Vector3.MoveTowards(locator.position, pos, 1.0f * Time.deltaTime);
            locator.rotation = Quaternion.RotateTowards(locator.rotation, targetRot, 90f * Time.deltaTime);

            _proceduralGrab.ResolveIK(locator.position, locator.rotation, rootSpace.up);
        }
    }
}