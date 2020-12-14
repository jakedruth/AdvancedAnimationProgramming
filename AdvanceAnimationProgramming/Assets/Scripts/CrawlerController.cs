/*
	Advanced Animation Programming
	By Jake Ruth

    CrawlerController.cs - The controller for my Crawler/ spider
*/

using System;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using Unity.Profiling;
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
        public float heightBobDuration;

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

        private ClipController _clipControllerHigh;
        private ClipController _clipControllerLow;

        private void Awake()
        {
            // Combine right and left legs into one array
            _ikComponents = new IKLeg[rightLegs.Length + leftLegs.Length];
            Array.Copy(rightLegs, _ikComponents, rightLegs.Length);
            Array.Copy(leftLegs, 0, _ikComponents, rightLegs.Length, leftLegs.Length);

            // Record important starting data
            _bodyStartRotation = body.rotation;
            _bodyHeight = _bodyRestHeight = body.localPosition.y;

            CreateClip();
        }

        private void CreateClip()
        {
            KeyframePool keyframePool = new KeyframePool(
                new Keyframe(0.0f, heightBobDuration / 2, 0.2f),
                new Keyframe(heightBobDuration / 2, heightBobDuration, 0.4f),
                new Keyframe(0.0f, heightBobDuration / 2, 0.6f),
                new Keyframe(heightBobDuration / 2, heightBobDuration, 0.8f));

            Clip clipHigh = new Clip("Idle-High", keyframePool, 2, 3, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD));
            Clip clipLow  = new Clip("Idle-Low",  keyframePool, 0, 1, new Transition(TransitionType.FORWARD), new Transition(TransitionType.BACKWARD));
            ClipPool clipPool = new ClipPool(clipHigh, clipLow);
            _clipControllerHigh = new ClipController("Spider High Controller", clipPool, 0);
            _clipControllerLow  = new ClipController("Spider Low Controller", clipPool, 1);
        }

        private void Update()
        {
            #region Update Clip Controllers

            //float dilation = 0.2f + Mathf.Clamp01(_moveSpeedParameter + _turnSpeedParameter) * 0.8f;

            _clipControllerHigh.Update(Time.deltaTime);
            _clipControllerLow.Update(Time.deltaTime);

            #endregion

            #region Get Input

            // Get Controller Inputs
            GamepadState current = GamePad.GetState(GamePad.Index.Any, true);
            Vector3 rawLeftStick = new Vector3(current.LeftStickAxis.x, 0, current.LeftStickAxis.y);
            Vector3 rawRightStick = new Vector3(current.RightStickAxis.x, 0, current.RightStickAxis.y);
            float leftTrigger = current.LeftTrigger;
            float rightTrigger = current.RightTrigger;

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

            // Clamp necessary values
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

            // The following 4 lines do the same thing. Testing to see if something would change
            //transform.rotation *= Quaternion.AngleAxis(alpha, Vector3.up);
            //transform.rotation = Quaternion.AngleAxis(alpha, transform.up) * transform.rotation;
            transform.Rotate(Vector3.up, alpha);
            //transform.Rotate(Vector3.right, pitch);

            // Calculate the body's height displacement
            float blendT = (1 + leftTrigger - rightTrigger) * 0.5f;

            // Calculate the target height by blending two clip controllers based on the triggers
            float targetHeight = ClipController.BlendControllerEvaluations(_clipControllerHigh, _clipControllerLow,
                ClipController.EvaluationType.CATMULL_ROM, blendT);
            
            // Calculate the body's height
            _bodyHeight = Mathf.MoveTowards(_bodyHeight, targetHeight, changeHeightSpeed * Time.deltaTime);

            // Calculate the bodies position based on the bodies height
            body.localPosition = Vector3.up * _bodyHeight;

            #endregion

            #region Body Rotation

            transform.rotation.Normalize();

            // Create rays originating from the body
            Vector3 pos = transform.position + transform.up * 0.3f;
            Ray rayDown = new Ray(pos, -transform.up);
            Vector3 moveDirection = transform.forward * _moveSpeedParameter;

            Ray rayGroundInFront = new Ray(pos, -transform.up + moveDirection * 0.5f);

            // Create a copy of the current rotation
            Quaternion targetRot = transform.rotation;

            if (Physics.Raycast(rayDown, out RaycastHit hit, 1f))
            {
                // Adjust the position of the Crawler to the raycast
                transform.position = hit.point;
                if (Physics.Raycast(rayGroundInFront, out RaycastHit hitDown))
                {
                    hit = hitDown;
                }

                // Calculate an up, right, and forward axis based of the raycast hit's surface
                Vector3 hitUp = hit.normal.normalized;
                Vector3 hitRight = Vector3.Cross(hitUp, transform.forward).normalized;
                Vector3 hitForward = Vector3.Cross(hitRight, hitUp).normalized;

                // Render new axis for debugging
                Debug.DrawRay(hit.point, hitRight, Color.red);
                Debug.DrawRay(hit.point, hitUp, Color.green);
                Debug.DrawRay(hit.point, hitForward, Color.blue);

                // Render the current 3 axis for debugging
                Debug.DrawRay(hit.point, transform.right, Color.magenta);
                Debug.DrawRay(hit.point, transform.up, Color.yellow);
                Debug.DrawRay(hit.point, transform.forward, Color.cyan);

                // Calculate a quaternion from a right up and forward axis
                targetRot = MathAA.GetRotationFromThreeAxis(hitRight, hitUp, hitForward);
            }

            float rotateStep = Mathf.Abs(_moveSpeedParameter) * maxTurnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateStep);

            #endregion
        }

        private void LateUpdate()
        {
            

            #region HandleIK

            for (int i = 0; i < _ikComponents.Length; i++)
            {
                _ikComponents[i].TryMove(_moveSpeedParameter, _turnSpeedParameter);
                _ikComponents[i].UpdateIK(Time.deltaTime);
            }

            #endregion
        }
    }
}