using System;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine;
using Random = UnityEngine.Random;


namespace AdvAnimation
{
    public class CrawlerController : MonoBehaviour
    {
        // Input Fields
        private GamepadState _prevGamepadState;

        public float maxMoveSpeed;
        public float moveAcceleration;
        private float _moveSpeed;
        private float _moveSpeedParameter;

        public float changeHeightSpeed;

        public float maxTurnSpeed;
        public float turnAcceleration;
        public float maxBodyRotate;
        private float _turnSpeed;
        private float _turnSpeedParameter;

        public IKLeg[] rightLegs;
        public IKLeg[] leftLegs;
        private IKLeg[] _ikComponents;
        public Transform body;
        private Quaternion _bodyStartRotation;
        private float _bodyRestHeight;
        private float _bodyHeight;

        private bool flip;

        private void Awake()
        {
            _ikComponents = new IKLeg[rightLegs.Length + leftLegs.Length];
            Array.Copy(rightLegs, _ikComponents, rightLegs.Length);
            Array.Copy(leftLegs, 0, _ikComponents, rightLegs.Length, leftLegs.Length);

            _bodyStartRotation = body.rotation;
            _bodyHeight = _bodyRestHeight = body.localPosition.y;

            for (int i = 0; i < 8; i++)
            {
                _ikComponents[i].locator.position +=
                    Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Vector3.right * 0.05f;
            }

            StartCoroutine(UpdateLegs());
        }

        private void Update()
        {
            #region Testing Raycasting

            transform.rotation.Normalize();

            Vector3 pos = transform.position + transform.up * 0.3f;
            Ray rayDown = new Ray(pos, -transform.up);
            Ray rayForward = new Ray(pos, transform.forward);

            Vector3 moveDirection = transform.forward * _moveSpeedParameter;

            Ray rayGroundInFront = new Ray(pos, -transform.up + moveDirection * 0.5f);
            //Debug.DrawRay(rayDown.origin, rayDown.direction, Color.red);
            //Debug.DrawRay(rayForward.origin, rayForward.direction, Color.red);

            Quaternion targetRot = transform.rotation;
            Quaternion targetBodyRot = body.rotation;

            if (Physics.Raycast(rayDown, out RaycastHit hit, 1f))
            {
                transform.position = hit.point;
                if (Physics.Raycast(rayGroundInFront, out RaycastHit hitDown))
                {
                    hit = hitDown;
                }

                Vector3 hitUp = hit.normal.normalized;
                Vector3 hitRight = Vector3.Cross(hitUp, transform.forward).normalized;
                Vector3 hitForward = Vector3.Cross(hitRight, hitUp).normalized;

                Debug.DrawRay(hit.point, hitRight, Color.red);
                Debug.DrawRay(hit.point, hitUp, Color.green);
                Debug.DrawRay(hit.point, hitForward, Color.blue);

                Debug.DrawRay(hit.point, transform.right, Color.magenta);
                Debug.DrawRay(hit.point, transform.up, Color.yellow);
                Debug.DrawRay(hit.point, transform.forward, Color.cyan);

                targetRot = MathAA.GetRotationFromThreeAxis(hitRight, hitUp, hitForward);

                //targetRot = MathAA.GetRotationFromThreeAxis(
                //    ((hitRight + transform.right) * 0.5f).normalized, 
                //    ((hitUp + transform.up) * 0.5f).normalized, 
                //    ((hitForward + transform.forward) *0.5f).normalized);
                
                //if (Physics.Raycast(rayForward, out RaycastHit hitForward))
                //{
                //    Vector3 nextUp = hitForward.normal;
                //    Vector3 nextRight = Vector3.Cross(nextUp, transform.forward).normalized;
                //    Vector3 nextForward = Vector3.Cross(nextRight, nextUp).normalized;

                //    Debug.DrawRay(hitForward.point, nextRight, Color.red);
                //    Debug.DrawRay(hitForward.point, nextUp, Color.green);
                //    Debug.DrawRay(hitForward.point, nextForward, Color.blue);

                //    Vector3 avgUp      = ((localUp + nextUp) * 0.5f).normalized;
                //    Vector3 avgRight   = ((localRight + nextRight) * 0.5f).normalized;
                //    Vector3 avgForward = ((localForward + nextForward) * 0.5f).normalized;

                //    Vector3 testRight = Vector3.Cross(avgUp, transform.forward).normalized;
                //    Vector3 testForward = Vector3.Cross(testRight, avgUp).normalized;

                //    if (hitForward.distance <= hitDown.distance * 2)
                //        targetBodyRot = MathAA.GetRotationFromThreeAxis(avgRight, avgUp, testForward);
                //}
                //else
                
                targetBodyRot = MathAA.GetRotationFromThreeAxis(hitRight, hitUp, hitForward);
            }

            float rotateStep = Mathf.Abs(_moveSpeedParameter) * maxTurnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateStep);
            //body.rotation =  transform.rotation * targetBodyRot * Quaternion.Inverse(transform.rotation) * _bodyStartRotation;
            #endregion
        }

        private void LateUpdate()
        {
            #region Get Input

            // Get Controller Inputs
            GamepadState current = GamePad.GetState(GamePad.Index.Any, true);
            Vector3 rawLeftStick = new Vector3(current.LeftStickAxis.x, 0, current.LeftStickAxis.y);
            Vector3 rawRightStick = new Vector3(current.RightStickAxis.x, 0, current.RightStickAxis.y);
            float leftTrigger = current.LeftTrigger;
            float rightTrigger = current.RightTrigger;

            float pitch = 0;
            if (current.RightShoulder)
                pitch -= maxTurnSpeed * Time.deltaTime;
            if (current.LeftShoulder)
                pitch += maxTurnSpeed * Time.deltaTime;

            // Adjust Inputs from keyboard Inputs
            if (Input.GetKey(KeyCode.A))
                rawLeftStick.x -= 1;
            if (Input.GetKey(KeyCode.D))
                rawLeftStick.x += 1;
            if (Input.GetKey(KeyCode.S))
                rawLeftStick.z -= 1;
            if (Input.GetKey(KeyCode.W))
                rawLeftStick.z += 1;
            if (Input.GetKey(KeyCode.Q))
                leftTrigger += 1;
            if (Input.GetKey(KeyCode.E))
                rightTrigger += 1;

            // Clamp needed values between 0 and 1
            rawLeftStick = Vector3.ClampMagnitude(rawLeftStick, 1);
            leftTrigger = Mathf.Clamp01(leftTrigger);
            rightTrigger = Mathf.Clamp01(rightTrigger);

            // Save the current state
            _prevGamepadState = current;

            #endregion

            #region Handle Input

            // Movement
            float targetSpeed = rawLeftStick.z * maxMoveSpeed;
            _moveSpeed = Mathf.MoveTowards(_moveSpeed, targetSpeed, moveAcceleration * Time.deltaTime);
            _moveSpeedParameter = _moveSpeed / maxMoveSpeed;
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;

            // Rotation
            float targetTurn = rawLeftStick.x * maxTurnSpeed;
            _turnSpeed = Mathf.MoveTowards(_turnSpeed, targetTurn, turnAcceleration * Time.deltaTime);
            _turnSpeedParameter = _turnSpeed / maxTurnSpeed;
            float alpha = _turnSpeed * Time.deltaTime;
            //transform.rotation *= Quaternion.AngleAxis(alpha, Vector3.up);
            //transform.rotation = Quaternion.AngleAxis(alpha, transform.up) * transform.rotation;
            transform.Rotate(Vector3.up, alpha);
            //transform.Rotate(Vector3.right, pitch);

            // Body Test
            float beta = maxBodyRotate * _turnSpeedParameter;
            //body.Rotate(Vector3.up, beta);

            float deltaBodyHeightLerpValue = Mathf.Clamp(rightTrigger - leftTrigger + 0.1f * Mathf.Sin(Time.time), -1, 1);
            float targetHeight = deltaBodyHeightLerpValue > 0 
                ? Mathf.Lerp(_bodyRestHeight, 0.8f, deltaBodyHeightLerpValue) 
                : Mathf.Lerp(_bodyRestHeight, 0.2f, -deltaBodyHeightLerpValue);

            //targetHeight += 0.1f * Mathf.Sin(Time.time / 3);

            _bodyHeight = Mathf.MoveTowards(_bodyHeight, targetHeight, changeHeightSpeed * Time.deltaTime);

            body.localPosition = Vector3.up * _bodyHeight;

            #endregion

            #region HandleIK

            for (int i = 0; i < _ikComponents.Length; i++)
            {
                _ikComponents[i].TryMove(_moveSpeedParameter, _turnSpeedParameter);
                _ikComponents[i].UpdateIK(Time.deltaTime);
            }

            //for (int i = 0; i < rightLegs.Length; i++)
            //{
            //    if (i % 2 == 0)
            //    {
            //        if (flip)
            //            rightLegs[i].TryMove(leftLegs[i].IsMoving);
            //        else 
            //            leftLegs[i].TryMove(rightLegs[i].IsMoving);
            //    }
            //    else
            //    {
            //        if (flip)
            //            leftLegs[i].TryMove(rightLegs[i].IsMoving);
            //        else rightLegs[i].TryMove(leftLegs[i].IsMoving);
            //    }
            //}

            flip = !flip;

            #endregion
        }

        IEnumerator UpdateLegs()
        {
            yield return null;
            //while (true)
            //{
            //    do
            //    {
            //        for (int i = 0; i < rightLegs.Length; i++)
            //        {
            //            rightLegs[i].UpdateComponent(Time.deltaTime);
            //        }

            //        yield return null;
            //    } while (rightLegs[0].IsMoving);

            //    do
            //    {
            //        for (int i = 0; i < leftLegs.Length; i++)
            //        {
            //            leftLegs[i].UpdateComponent(Time.deltaTime);
            //        }

            //        yield return null;
            //    } while (leftLegs[0].IsMoving);
            //}
        }
    }
}