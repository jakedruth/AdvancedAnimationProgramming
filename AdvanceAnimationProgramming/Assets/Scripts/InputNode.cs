using System;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine;


namespace AdvAnimation
{
    public enum LocomotionControlType
    {
        DIRECT_VALUE,
        CONTROL_VELOCITY,
        CONTROL_ACCELERATION,
        FAKE_VELOCITY,
        FAKE_ACCELERATION,
    }

    public class InputNode : MonoBehaviour
    {
        private GamepadState _prevGamepadState;

        public float scaleValue;
        public LocomotionControlType positionControlType;
        public LocomotionControlType rotationControlType;
        [Range(0, 1)]
        public float lerpValue;

        private Vector3 _pos;
        private Vector3 _posVel;
        private Vector3 _posAcc;
        private float _rot;
        private float _rotVel;
        private float _rotAcc;

        // Start is called before the first frame update
        void Start()
        {
            _prevGamepadState = GamePad.GetState(GamePad.Index.Any, true);
        }

        // Update is called once per frame
        void Update()
        {
            GamepadState current = GamePad.GetState(GamePad.Index.Any, true);

            Vector3 rawInputLeft = new Vector3(current.LeftStickAxis.x, 0, current.LeftStickAxis.y);
            Vector3 rawInputRight = new Vector3(current.rightStickAxis.x, 0, current.rightStickAxis.y);

            if (current.Right && !_prevGamepadState.Right)
                GoToNextLocomotionControlType(ref positionControlType, true);

            if (current.Left && !_prevGamepadState.Left)
                GoToNextLocomotionControlType(ref positionControlType, false);

            if (current.Up && !_prevGamepadState.Up)
                GoToNextLocomotionControlType(ref rotationControlType, true);

            if (current.Down && !_prevGamepadState.Down)
                GoToNextLocomotionControlType(ref rotationControlType, false);

            if (Input.GetKey(KeyCode.A))
                rawInputLeft.x -= 1;
            if (Input.GetKey(KeyCode.D))
                rawInputLeft.x += 1;
            if (Input.GetKey(KeyCode.S))
                rawInputLeft.z -= 1;
            if (Input.GetKey(KeyCode.W))
                rawInputLeft.z += 1;
            rawInputLeft = Vector3.ClampMagnitude(rawInputLeft, 1);

            if (Input.GetKey(KeyCode.J))
                rawInputRight.x -= 1;
            if (Input.GetKey(KeyCode.L))
                rawInputRight.x += 1;
            if (Input.GetKey(KeyCode.K))
                rawInputRight.z -= 1;
            if (Input.GetKey(KeyCode.I))
                rawInputRight.z += 1;
            rawInputRight = Vector3.ClampMagnitude(rawInputRight, 1);

            _prevGamepadState = current;

            switch (positionControlType)
            {
                case LocomotionControlType.DIRECT_VALUE:
                    // Set Position
                    _pos = rawInputLeft * scaleValue;
                    break;
                case LocomotionControlType.CONTROL_VELOCITY:
                    // Set Position
                    _posVel = rawInputLeft;
                    _pos += _posVel * Time.deltaTime;
                    break;
                case LocomotionControlType.CONTROL_ACCELERATION:
                    // Set Position
                    _posAcc = rawInputLeft;
                    _posVel += _posAcc * Time.deltaTime;
                    _pos += _posVel * Time.deltaTime;
                    break;
                case LocomotionControlType.FAKE_VELOCITY:
                    // Set Position
                    _pos = Vector3.Lerp(_pos, rawInputLeft, lerpValue);
                    break;
                case LocomotionControlType.FAKE_ACCELERATION:
                    // Set Position
                    _posAcc = Vector3.Lerp(_posAcc, rawInputLeft, lerpValue);
                    _posVel += _posAcc * Time.deltaTime;
                    _pos += _posVel * Time.deltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (rotationControlType)
            {
                case LocomotionControlType.DIRECT_VALUE:
                    // Set Rotation
                    _rot = rawInputRight.x * 180;
                    break;
                case LocomotionControlType.CONTROL_VELOCITY:
                    // Set Rotation
                    _rotVel = rawInputRight.x * 20f;
                    _rot += _rotVel * Time.deltaTime;
                    break;
                case LocomotionControlType.CONTROL_ACCELERATION:
                    // Set Rotation
                    _rotAcc = rawInputRight.x * 20f;
                    _rotVel += _rotAcc * Time.deltaTime;
                    _rot += _rotVel * Time.deltaTime;
                    break;
                case LocomotionControlType.FAKE_VELOCITY:
                    // Set Rotation
                    _rot = Mathf.Lerp(_rot, rawInputRight.x * 180, lerpValue);
                    break;
                case LocomotionControlType.FAKE_ACCELERATION:
                    // Set Rotation
                    _rotAcc = Mathf.Lerp(_rotAcc, rawInputRight.x * 20, lerpValue);
                    _rotVel += _rotAcc * Time.deltaTime;
                    _rot += _rotVel * Time.deltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            transform.position = _pos;
            transform.rotation = Quaternion.AngleAxis(_rot, Vector3.up);
        }

        public void GoToNextLocomotionControlType(ref LocomotionControlType controlType, bool next)
        {
            controlType = next ? controlType.Next() : controlType.Prev();

            _pos = _posVel = _posAcc = Vector3.zero;
            _rot = _rotVel = _rotAcc = 0f;
        }
    }

}