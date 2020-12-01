using System;
using System.Collections;
using System.Collections.Generic;
using GamepadInput;
using UnityEngine;

namespace AdvAnimation
{
    public class PlayerController : MonoBehaviour
    {
        // Input Variables
        private GamepadState _prevGamepadState;
        private CharacterController _characterController;

        private const float DEAD_ZONE = 0.01f; // = 0.1 ^ 2
        [Header("Movement Variables")]
        public float maxSpeed;
        public float jumpSpeed;
        public float accelerationRate;
        public float gravity;
        private Vector3 _horizontalVelocity;
        private Vector3 _verticalVelocity;
        public float rotateSpeed;

        private ProceduralLook _proceduralLook;
        [Header("Procedural Looking Variables")]
        public Transform headBone;
        public Transform headLocator;
        public float lookDist;
        private GameObject[] _lookObjects;
        private Vector3 _startDeltaLookAtPoint;
        private Vector3 _targetLookAt;

        //[Header("Procedural Grab Variables")]
        [Header("Right Arm")]
        public Transform rightHand;
        public Transform rightHandLocator;
        public Transform rightHandConstraint;
        private ProceduralGrab _proceduralRightArm;

        [Header("Left Arm")]
        public Transform leftHand;
        public Transform leftHandLocator;
        public Transform leftHandConstraint;
        private ProceduralGrab _proceduralLeftArm;

        [Header("Right Leg")]
        public Transform rightFoot;
        public Transform rightFootLocator;
        public Transform rightFootConstraint;
        private ProceduralGrab _proceduralRightLeg;
        private Vector3 _rightFootStartPos;

        [Header("Left Leg")]
        public Transform leftFoot;
        public Transform leftFootLocator;
        public Transform leftFootConstraint;
        private ProceduralGrab _proceduralLeftLeg;
        private Vector3 _leftFootStartPos;

        [Range(0, 1)] public float snapBack;

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            // set default values
            // movement
            _verticalVelocity = _horizontalVelocity = Vector3.zero;

            // procedural look variables
            _proceduralLook = new ProceduralLook(headBone);
            _startDeltaLookAtPoint = headLocator.position - transform.position;
            _targetLookAt = headLocator.position;

            // procedural grab variables
            _proceduralRightArm = new ProceduralGrab(rightHand, rightHandLocator, 2);
            _proceduralLeftArm  = new ProceduralGrab(leftHand,  leftHandLocator,  2);
            _proceduralRightLeg = new ProceduralGrab(rightFoot, rightFootLocator, 2);
            _proceduralLeftLeg  = new ProceduralGrab(leftFoot,  leftFootLocator,  2);

            // Starting Locator Values
            _rightFootStartPos = transform.InverseTransformPoint(rightFootLocator.position);

            _leftFootStartPos = transform.InverseTransformPoint(leftFootLocator.position);
        }

        // Start is called before the first frame update
        void Start()
        {
            _lookObjects = GameObject.FindGameObjectsWithTag("LookAt");
        }

        // Update is called once per frame
        void Update()
        {
            #region Get Input

            GamepadState current = GamePad.GetState(GamePad.Index.Any, true);
            Vector3 rawInputLeft = new Vector3(current.LeftStickAxis.x, 0, current.LeftStickAxis.y);
            Vector3 rawInputRight = new Vector3(current.rightStickAxis.x, 0, current.rightStickAxis.y);
            float rightTrigger = current.RightTrigger;
            float leftTrigger = current.LeftTrigger;
            bool jumpKeyDown = current.A && !_prevGamepadState.A || Input.GetKeyDown(KeyCode.Space);

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

            #endregion

            #region Handle Movement

            // Calculate the current horizontal velocity based of the target velocity defined by the player input
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, rawInputLeft * maxSpeed, accelerationRate * Time.deltaTime);

            // Apply Gravity
            _verticalVelocity.y -= gravity * Time.deltaTime;

            // check to see if the jump key is pressed
            if (jumpKeyDown)
            {
                _verticalVelocity.y = jumpSpeed;
                Debug.Log(_verticalVelocity.y);
            }

            // Rotate the model if the raw input is greater than the dead zone
            if (rawInputLeft.sqrMagnitude >= DEAD_ZONE)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_horizontalVelocity),
                    rotateSpeed * Time.deltaTime);
            }

            // Apply the total velocity to the character controller (Unity Script/ Component)
            _characterController.Move((_horizontalVelocity + _verticalVelocity) * Time.deltaTime);

            #endregion

            #region Raycasting for the feet
            
            // Set a the max foot position relative
            Vector3 maxFootUp = new Vector3(0, 0.5f, 0);

            // Ray cast from the right foot downwards
            Ray rightFootRay = new Ray(transform.position + transform.rotation * _rightFootStartPos + Vector3.up, Vector3.down);
            if (Physics.Raycast(rightFootRay, out RaycastHit hit))
            {
                // set the right foot locator based on the hit
                rightFootLocator.position = hit.point + transform.rotation * Vector3.up * _rightFootStartPos.y;

                // Adjust the foot height based off the trigger
                rightFootLocator.position += maxFootUp * rightTrigger;

                // calculate a target rotation based off the Raycast Hit Normal
                Vector3 localUp = hit.normal;
                Vector3 localForward = Vector3.Cross(localUp, Vector3.left);
                Quaternion targetRot = Quaternion.AngleAxis(transform.eulerAngles.y, localUp) * Quaternion.LookRotation(localForward);
                rightFootLocator.rotation = targetRot;
            }

            // Ray cast from the left foot downwards
            Ray leftFootRay = new Ray(transform.position + transform.rotation * _leftFootStartPos + Vector3.up, Vector3.down);
            if (Physics.Raycast(leftFootRay, out hit))
            {
                // Set the left foot locator based on the hit
                leftFootLocator.position = hit.point + transform.rotation * Vector3.up * _leftFootStartPos.y;
                
                // Adjust the foot height based off the trigger
                leftFootLocator.position += maxFootUp * leftTrigger;

                // calculate a target rotation based off the Raycast Hit Normal
                Vector3 localUp = hit.normal;
                Vector3 localForward = Vector3.Cross(localUp, Vector3.left);
                Quaternion targetRot = Quaternion.AngleAxis(transform.eulerAngles.y, localUp) * Quaternion.LookRotation(localForward);
                leftFootLocator.rotation = targetRot;
            } 

            #endregion

            #region Handle Head Look At

            // Rotate the head if the Right Stick has input
            if (rawInputRight.sqrMagnitude >= DEAD_ZONE)
            {
                float angle = Vector3.SignedAngle(Vector3.forward, rawInputRight, Vector3.up);
                _targetLookAt = transform.position + Quaternion.AngleAxis(angle, Vector3.up) * _startDeltaLookAtPoint;
            }
            else
            {
                // Rotate the head to look at the closest Object with tag
                Transform closestLookAt = FindClosestLookAtObjectTransform();
                _targetLookAt = closestLookAt != null
                    ? closestLookAt.transform.position
                    : transform.position + transform.rotation * _startDeltaLookAtPoint;
            }

            // lerp the head locator's position based on what the target is
            Vector3 pos = Vector3.Lerp(headLocator.position, _targetLookAt, 10 * Time.deltaTime);
            headLocator.position = pos;
            _proceduralLook.LookAt(headLocator.position);

            #endregion

            #region Handle Grabing

            // Calculate all of the IK for each hand and foot
            _proceduralRightArm.ResolveIK(rightHandLocator.position, rightHandLocator.rotation, rightHandConstraint.position);
            _proceduralLeftArm.ResolveIK( leftHandLocator.position,  leftHandLocator.rotation,  leftHandConstraint.position);
            _proceduralRightLeg.ResolveIK(rightFootLocator.position, rightFootLocator.rotation, rightFootConstraint.position);
            _proceduralLeftLeg.ResolveIK( leftFootLocator.position,  leftFootLocator.rotation,  leftFootConstraint.position);

            #endregion
        }

        private Transform FindClosestLookAtObjectTransform()
        {
            Transform closest = null;
            float closestSqrDist = lookDist * lookDist;

            // Look for the closest transform based off the list of objects to look at
            for (int i = 0; i < _lookObjects.Length; i++)
            {
                Vector3 displacement = _lookObjects[i].transform.position - headBone.position;
                if (displacement.sqrMagnitude <= closestSqrDist)
                {
                    closest = _lookObjects[i].transform;
                    closestSqrDist = displacement.sqrMagnitude;
                }
            }

            return closest;
        }

        private void OnDrawGizmosSelected()
        {
            // draw the locators green and constraints as red
            Gizmos.color = Color.red;
            const float size = 0.1f;
            Gizmos.DrawWireSphere(headLocator.position, size);
            Gizmos.DrawWireSphere(rightHandLocator.position, size);
            Gizmos.DrawWireSphere(leftHandLocator.position,  size);
            Gizmos.DrawWireSphere(rightFootLocator.position,  size);
            Gizmos.DrawWireSphere(leftFootLocator.position,  size);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(rightHandConstraint.position, Vector3.one * size);
            Gizmos.DrawWireCube(leftHandConstraint.position, Vector3.one * size);
            Gizmos.DrawWireCube(rightFootConstraint.position, Vector3.one * size);
            Gizmos.DrawWireCube(leftFootConstraint.position, Vector3.one * size);
        }


    }
}