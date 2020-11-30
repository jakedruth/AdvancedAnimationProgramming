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

        private const float DEAD_ZONE = 0.01f; // = 0.1 ^ 2
        [Header("Movement Variables")]
        public float maxSpeed;
        public float accelerationRate;
        private Vector3 _velocity;
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
        public Transform rightLeg;
        public Transform rightLegLocator;
        public Transform rightLegConstraint;
        private ProceduralGrab _proceduralRightLeg;


        [Range(0, 1)] public float snapBack;

        void Awake()
        {
            // set default values
            // movement
            _velocity = Vector3.zero;

            // procedural look variables
            _proceduralLook = new ProceduralLook(headBone);
            _startDeltaLookAtPoint = headLocator.position - transform.position;
            _targetLookAt = headLocator.position;

            // procedural grab variables
            _proceduralRightArm = new ProceduralGrab(rightHand, rightHandLocator, 2);
            _proceduralLeftArm  = new ProceduralGrab(leftHand,  leftHandLocator,  2);
            _proceduralRightLeg = new ProceduralGrab(rightLeg,  rightLegLocator,  2);
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

            _velocity = Vector3.MoveTowards(_velocity, rawInputLeft * maxSpeed, accelerationRate * Time.deltaTime);

            if (rawInputLeft.sqrMagnitude >= DEAD_ZONE)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_velocity),
                    rotateSpeed * Time.deltaTime);
            }

            transform.position += _velocity * Time.deltaTime;

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

            Vector3 pos = Vector3.Lerp(headLocator.position, _targetLookAt, 10 * Time.deltaTime);
            headLocator.position = pos;
            _proceduralLook.LookAt(headLocator.position);

            #endregion

            #region Handle Grabing

            //_proceduralRightArm.GrabAt(rightHandLocator.position, rightHandConstraint.position, _direction[direction]);
            _proceduralRightArm.ResolveIK(rightHandLocator.position, rightHandLocator.rotation, rightHandConstraint.position, snapBack);
            _proceduralLeftArm.ResolveIK( leftHandLocator.position,  leftHandLocator.rotation,  leftHandConstraint.position,  snapBack);
            _proceduralRightLeg.ResolveIK(rightLegLocator.position,  rightLegLocator.rotation,  rightLegConstraint.position,  snapBack);
            #endregion
        }

        private Transform FindClosestLookAtObjectTransform()
        {
            Transform closest = null;
            float closestSqrDist = lookDist * lookDist;

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
            Gizmos.DrawWireSphere(rightLegLocator.position,  size);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(rightHandConstraint.position, Vector3.one * size);
            Gizmos.DrawWireCube(leftHandConstraint.position, Vector3.one * size);
            Gizmos.DrawWireCube(rightLegConstraint.position, Vector3.one * size);
        }


    }
}