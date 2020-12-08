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
        public float changeHeightSpeed;

        public float maxTurnSpeed;
        public float turnAcceleration;
        public float maxBodyRotate;
        private float _turnSpeed;
        public IKComponent[] rightLegs;
        public IKComponent[] leftLegs;
        private IKComponent[] _ikComponents;
        public Transform body;
        private Quaternion _bodyStartLocal;
        private float _bodyRestHeight;
        private float _bodyHeight;

        private bool flip;

        private void Awake()
        {
            _ikComponents = new IKComponent[rightLegs.Length + leftLegs.Length];
            Array.Copy(rightLegs, _ikComponents, rightLegs.Length);
            Array.Copy(leftLegs, 0, _ikComponents, rightLegs.Length, leftLegs.Length);

            _bodyStartLocal = body.localRotation;
            _bodyHeight = _bodyRestHeight = body.localPosition.y;

            for (int i = 0; i < 8; i++)
            {
                _ikComponents[i].locator.position += Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Vector3.right * 0.05f;
            }

            StartCoroutine(UpdateLegs());
        }

        private void Update()
        {
            #region Testing Raycasting

            Vector3 pos = transform.position + transform.up * 0.3f;
            Ray rayDown = new Ray(pos, -transform.up);
            Ray rayForward = new Ray(pos, transform.forward);
            //Debug.DrawRay(rayDown.origin, rayDown.direction, Color.red);
            //Debug.DrawRay(rayForward.origin, rayForward.direction, Color.red);

            Quaternion targetRot = transform.rotation;
            Quaternion targetBodyRot = body.rotation;

            if (Physics.Raycast(rayDown, out RaycastHit hitDown))
            {
                transform.position = hitDown.point;
                Vector3 localUp = hitDown.normal;
                Vector3 localRight = Vector3.Cross(localUp, transform.forward).normalized;
                Vector3 localForward = Vector3.Cross(localRight, localUp).normalized;

                targetRot = MathAA.GetRotationFromThreeAxis(localRight, localUp, localForward);

                Debug.DrawRay(hitDown.point, localRight, Color.red);
                Debug.DrawRay(hitDown.point, localUp, Color.green);
                Debug.DrawRay(hitDown.point, localForward, Color.blue);

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
                {
                    targetBodyRot = MathAA.GetRotationFromThreeAxis(localRight, localUp, localForward);
                }
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, maxTurnSpeed * Time.deltaTime);
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
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;

            // Rotation
            float targetTurn = rawLeftStick.x * maxTurnSpeed;
            _turnSpeed = Mathf.MoveTowards(_turnSpeed, targetTurn, turnAcceleration * Time.deltaTime);
            transform.Rotate(Vector3.up, _turnSpeed * Time.deltaTime);
            //transform.Rotate(Vector3.right, pitch);

            // Body Test
            float angle = maxBodyRotate * _turnSpeed / maxTurnSpeed;
            //body.Rotate(Vector3.up, angle);

            float deltaBodyHeightLerpValue = rightTrigger - leftTrigger;
            float targetHeight = deltaBodyHeightLerpValue > 0 
                ? Mathf.Lerp(_bodyRestHeight, 0.8f, deltaBodyHeightLerpValue) 
                : Mathf.Lerp(_bodyRestHeight, 0.2f, -deltaBodyHeightLerpValue);

            _bodyHeight = Mathf.MoveTowards(_bodyHeight, targetHeight, changeHeightSpeed * Time.deltaTime);

            body.localPosition = Vector3.up * _bodyHeight;

            #endregion

            #region HandleIK

            for (int i = 0; i < _ikComponents.Length; i++)
            {
                _ikComponents[i].TryMove(false);
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