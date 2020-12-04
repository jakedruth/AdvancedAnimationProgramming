using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class IKComponent : MonoBehaviour
    {
        public float footMaxDistance;
        public float overshootDistance;
        public float lerpSpeed;

        public bool autoUpdate = true;
        public Transform restPosition;
        public Transform locator;
        public Transform constraint;
        private ProceduralGrab _proceduralGrab;
        private Vector3 _targetPos;

        // Start is called before the first frame update
        private void Start()
        {
            _proceduralGrab = new ProceduralGrab(transform, locator, 2);
            _targetPos = locator.position;
        }

        // Update is called once per frame
        private void Update()
        {
            if (autoUpdate)
                UpdateIK();
        }

        public void UpdateIK()
        {
            Vector3 displacement = restPosition.position - _targetPos;
            if (displacement.sqrMagnitude > footMaxDistance * footMaxDistance)
            {
                float distance = displacement.magnitude + overshootDistance;
                Vector3 dir = displacement.normalized;
                _targetPos = _targetPos + dir * distance;
            }

            locator.position = Vector3.Lerp(locator.position, _targetPos, lerpSpeed * Time.deltaTime);

            _proceduralGrab.ResolveIK(locator.position, locator.rotation, constraint.position);
        }
    }
}
