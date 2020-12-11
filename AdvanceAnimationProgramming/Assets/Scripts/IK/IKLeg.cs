/*
	Advanced Animation Programming
	By Jake Ruth

    IKLeg.cs - Script that is used to control a spider's leg with IK
*/

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

        public bool isMoving { get; private set; }

        private float _timeDilation;

        // Start is called before the first frame update
        private void Awake()
        {
            _proceduralGrab = new ProceduralGrab(transform, locator, parentCount);
            _restHeight = restTransform.localPosition.y;
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
            // Reset the time dilation based on input from controller
            _timeDilation = Mathf.Clamp01(Mathf.Abs(moveParameter) + Mathf.Abs(turnParameter));

            // No need to move, if already moving
            if (isMoving)
                return;

            // Calculate the distance from the locator to the resting position
            float sqrDist = GetSqrDistFromRest(locator.position);
            float maxDelta = footMaxDistance * footMaxDistance;

            if (sqrDist > maxDelta)
            {
                StartMoving(moveParameter);
            }
        }

        public float GetSqrDistFromRest(Vector3 point)
        {
            // Make sure the distance is based of a plane's projects
            // Other wise, a valid foot placement could be too far.
            // This occurs when the spider is on one surface and then locator is on another surface (curving)
            Plane plane = new Plane(restTransform.up, restTransform.position);
            Vector3 projectedPoint = plane.ClosestPointOnPlane(point);
            Vector3 displacement = projectedPoint - restTransform.position;
            
            return displacement.sqrMagnitude;
        }

        public void StartMoving(float param = 0)
        {
            if (isMoving)
                return;

            StartCoroutine(Move(param));
        }

        private IEnumerator Move(float param)
        {
            // Set is moving to true
            isMoving = true;

            // create variables for moving
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

            // attempt to raycast from the rest
            Ray ray = new Ray(endPosition + restTransform.up * 2, -restTransform.up);
            Debug.DrawRay(ray.origin, ray.direction * 2, Color.cyan, 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // adjust the endPosition and endRotation to match the raycasted's point
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

            // Find a midpoint between the start and end
            Vector3 centerPoint = (startPosition + endPosition) / 2;
            
            // adjust the height of the midpoint by a step height
            centerPoint += restTransform.up * stepHeight;

            // Calculate a random duration for an animation
            float duration = Random.Range(minMoveDuration, maxMoveDuration);

            // start a timer loop
            while (timer < duration && timer >= 0)
            {
                timer += Time.deltaTime * _timeDilation;

                // normalize the time and apply an easing function
                float normalizedTime = Easing.Cubic.InOut(Mathf.Clamp01(timer / duration));
                
                // set the position and rotation of the locator
                locator.position = MathAA.BezierCurve(normalizedTime, startPosition, centerPoint, endPosition);
                locator.rotation = Quaternion.Slerp(startRotation, endRotation, normalizedTime);

                yield return null;
            }

            yield return null;
            isMoving = false;
        }

        public void UpdateIK(float dt)
        {
            // Update the IK so the leg is moving towards the locator
            Vector3 pole = locator.position + rootSpace.up;
            _proceduralGrab.ResolveIK(locator.position, locator.rotation, pole);
        }
    }
}
