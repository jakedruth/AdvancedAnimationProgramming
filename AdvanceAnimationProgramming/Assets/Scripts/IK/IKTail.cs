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
            Ray ray = new Ray(restTransform.position, -restTransform.up);
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
            Vector3 pos = restTransform.position;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                pos = hit.point - ray.direction * _height;
                locator.rotation = MathAA.GetRotationFromRaycastHitAndForward(hit, rootSpace.forward);
            }

            locator.position = Vector3.MoveTowards(locator.position, pos, 1.0f * Time.deltaTime);

            _proceduralGrab.ResolveIK(locator.position, locator.rotation, rootSpace.up);
        }
    }
}