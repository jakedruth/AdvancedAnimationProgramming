using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAnimation
{
    public class IKLeg : MonoBehaviour
    {
        public int parentCount = 2;

        public float footMaxDistance;
        public float overshootDistance;
        public float minMoveDuration;
        public float maxMoveDuration;
        public float stepHeight;

        public Transform rootSpace;
        public Transform locator;
        public Transform constraint;
        public Transform restTransform;
        private float _restHeight;

        private ProceduralGrab _proceduralGrab;

        public bool IsMoving { get; private set; }

        private float _timeDilation;

        // Start is called before the first frame update
        private void Awake()
        {
            _proceduralGrab = new ProceduralGrab(transform, locator, parentCount);
            _restHeight = restTransform.localPosition.y;

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

        public void TryMove(float moveParameter, float turnParameter)
        {
            _timeDilation = Mathf.Clamp01(Mathf.Abs(moveParameter) + Mathf.Abs(turnParameter));

            if (IsMoving)
                return;

            float sqrDist = GetSqrDistFromRest(locator.position);
            float maxDelta = footMaxDistance * footMaxDistance;

            if (sqrDist > maxDelta)
            {
                StartMoving(moveParameter);
            }
        }

        public float GetSqrDistFromRest(Vector3 point)
        {
            Plane plane = new Plane(restTransform.up, restTransform.position);
            Vector3 projectedPoint = plane.ClosestPointOnPlane(point);
            Vector3 displacement = projectedPoint - restTransform.position;
            
            return displacement.sqrMagnitude;
        }

        public void StartMoving(float param = 0)
        {
            if (IsMoving)
                return;

            StartCoroutine(Move(param));
        }

        private IEnumerator Move(float param)
        {
            IsMoving = true;

            float timer = 0;
            Vector3 startPosition = transform.position;
            Quaternion startRotation = locator.rotation;

            Vector3 displacement = restTransform.position - locator.position;
            float stepLength = displacement.magnitude + overshootDistance;
            Vector3 dir = displacement.normalized;

            Vector3 endPosition = startPosition + dir * stepLength;
            Vector3 inFront = restTransform.position + restTransform.forward * overshootDistance * param;

            endPosition = (endPosition + inFront) * 0.5f;

            Quaternion endRotation = restTransform.rotation;

            Ray ray = new Ray(endPosition + restTransform.up * 2, -restTransform.up);
            Debug.DrawRay(ray.origin, ray.direction * 2, Color.cyan, 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                endPosition = hit.point + hit.normal * _restHeight;

                Vector3 localUp = hit.normal;
                Vector3 localRight = Vector3.Cross(localUp, restTransform.forward).normalized;
                Vector3 localForward = Vector3.Cross(localRight, localUp).normalized;

                //Debug.DrawRay(hit.point, localRight, Color.red, 1.0f);
                //Debug.DrawRay(hit.point, localUp, Color.green, 1.0f);
                //Debug.DrawRay(hit.point, localForward, Color.blue, 1.0f);

                endRotation = MathAA.GetRotationFromThreeAxis(localRight, localUp, localForward);

                //endRotation = MathAA.GetRotationFromRaycastHitAndForward(hit, restTransform.forward);
            }

            Vector3 centerPoint = (startPosition + endPosition) / 2;
            centerPoint += restTransform.up * stepHeight;

            float duration = Random.Range(minMoveDuration, maxMoveDuration);

            while (timer < duration && timer >= 0)
            {
                timer += Time.deltaTime * _timeDilation;

                float normalizedTime = Easing.Cubic.InOut(Mathf.Clamp01(timer / duration));
                
                locator.position = MathAA.BezierCurve(normalizedTime, startPosition, centerPoint, endPosition);
                locator.rotation = Quaternion.Slerp(startRotation, endRotation, normalizedTime);

                yield return null;
            }

            yield return null;
            IsMoving = false;
        }

        public void UpdateIK(float dt)
        {
            Vector3 con = locator.position + rootSpace.up;
            _proceduralGrab.ResolveIK(locator.position, locator.rotation, con);
        }
    }
}
