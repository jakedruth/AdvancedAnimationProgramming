using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class IKComponent : MonoBehaviour
    {
        public int parentCount = 2;

        public float footMaxDistance;
        public float overshootDistance;
        public float lerpSpeed;
        public float moveDuration;

        public Transform rootSpace;
        public Transform locator;
        public Transform constraint;
        public Transform restTransform;
        private ProceduralGrab _proceduralGrab;

        public bool IsMoving { get; private set; }

        // Start is called before the first frame update
        private void Awake()
        {
            _proceduralGrab = new ProceduralGrab(transform, locator, parentCount);
            //locator.position = transform.position;
            locator.SetParent(null);
        }

        private void OnEnable()
        {
            _proceduralGrab.onOverExtended += StartMoving;
        }

        private void OnDisable()
        {
            _proceduralGrab.onOverExtended -= StartMoving;
        }

        public void TryMove(bool oppositeLegIsMoving)
        {
            if (IsMoving)
                return;

            Vector3 displacement = locator.position - restTransform.position;

            float delta = footMaxDistance * (oppositeLegIsMoving ? 2f : 1f);
            delta *= delta;

            if (displacement.sqrMagnitude > delta)
            {
                StartMoving();
            }
        }

        public void StartMoving()
        {
            if (IsMoving)
                return;

            StartCoroutine(Move());
        }

        private IEnumerator Move()
        {
            IsMoving = true;

            float timer = 0;
            Vector3 startPosition = transform.position;
            Quaternion startRotation = locator.rotation;

            Vector3 displacement = restTransform.position - locator.position;
            float distance = displacement.magnitude + overshootDistance;
            Vector3 dir = displacement.normalized;

            Vector3 endPosition = startPosition + dir * distance;
            Quaternion endRotation = restTransform.rotation;

            Vector3 centerPoint = (startPosition + endPosition) / 2;
            centerPoint += restTransform.up * 0.5f;

            while (timer < moveDuration)
            {
                timer += Time.deltaTime;

                float normalizedTime = Easing.Cubic.InOut(Mathf.Clamp01(timer / moveDuration));
                
                locator.position = MathAA.BezierCurve(normalizedTime, startPosition, centerPoint, endPosition);
                locator.rotation = Quaternion.Slerp(startRotation, endRotation, normalizedTime);

                yield return null;
            }

            yield return null;
            IsMoving = false;
        }

        public void UpdateIK(float dt)
        {
            _proceduralGrab.ResolveIK(locator.position, locator.rotation, constraint.position);
        }
    }
}
