using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController.InputController;
using UnityEngine.InputSystem;

namespace PlayerController
{
    public enum PlayerMovementType 
    { 
      RIGIDBODY_VELOCITY,
      RIGIDBODY_MOVEPOSITION,
      TRANSFORM_TRANSLATE,
      MOVE_TOWARD
    }

    public class PlayerMovement : MonoBehaviour
    {
        public PlayerMovementType playerMovement = PlayerMovementType.RIGIDBODY_VELOCITY;

       [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private float moveSpeed;

        [Range(0,10)]
        [SerializeField] private float movementSmooth;
        private float currentMoveSpeed;

        [SerializeField] private float rotationSpeed;
        [Range(0, 10)]
        [SerializeField] private float rotationSmooth;
        private float currentRotationSpeed;

        private Vector3 moveDirection;
        private Vector3 moveInput;

        [Header ("Jump")]
        public float jumpHeight = 9.8f;

        public float groundDistance;
        private RaycastHit groundHit;
        [SerializeField] private LayerMask grounded;

        public bool jumpWithRigidbodyForce;
        public float groundDetectionDistance = 10f;
        [Range(0, 10)]
        public float groundMinDistance = 0.1f;
        [Range(0, 10)]
        public float groundMaxDistance = 0.5f;
        public float jumpForce = 10f;
        private PlayerInput inputActions;

        #region Unity Function
        private void Awake()
        {
            inputActions = new PlayerInput();
            inputActions.Enable();
            inputActions.PlayerController.Jump.performed += JumpBehaviour;
        }
        private void Update()
        {
            Physics.SyncTransforms();
            moveInput = inputActions.PlayerController.Movement.ReadValue<Vector2>();
            UpdateTargetDirectionAccordingTocamera();
            MoveRotationSpeed();
            UpdateRotation();
            ChekGroundDistance();
        }
        #endregion

        #region Jump Action
        private void JumpBehaviour(InputAction.CallbackContext context)
        {
            if (IsGrounded() && context.performed)
            {
                if (jumpWithRigidbodyForce)
                {
                    rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
                else
                {
                    var vel = rigidbody.velocity;
                    vel.y = jumpHeight;
                    rigidbody.velocity = vel;
                }
            }
        }
        #endregion

        #region Ground Check
        public bool IsGrounded()
        {
           Physics.SphereCast(capsuleCollider.bounds.center,capsuleCollider.radius , Vector3.down ,out groundHit,2f,grounded ,QueryTriggerInteraction.UseGlobal);
            return (groundHit.collider != null);
        }
        #endregion

        #region Ground Distance 
        private void ChekGroundDistance()
        {
            if (capsuleCollider != null)
            {
                // radius of the SphereCast
                float radius = capsuleCollider.radius * 0.9f;
                var dist = groundDetectionDistance;
                // ray for RayCast
                Ray ray2 = new Ray(transform.position + new Vector3(0, capsuleCollider.height / 2, 0), Vector3.down);
                // raycast for check the ground distance
                if (Physics.Raycast(ray2, out groundHit, (capsuleCollider.height / 2) + dist, grounded) && !groundHit.collider.isTrigger)
                {
                    dist = transform.position.y - groundHit.point.y;
                }
                // sphere cast around the base of the capsule to check the ground distance
                if (dist >= groundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * (capsuleCollider.radius);
                    Ray ray = new Ray(pos, -Vector3.up);
                    if (Physics.SphereCast(ray, radius, out groundHit, capsuleCollider.radius + groundMaxDistance, grounded) && !groundHit.collider.isTrigger)
                    {
                        Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, grounded);
                        float newDist = transform.position.y - groundHit.point.y;
                        if (dist > newDist)
                        {
                            dist = newDist;
                        }
                    }
                }
                groundDistance = (float)System.Math.Round(dist, 2);
            }
        }
        #endregion

        #region Locomotion
        private void MoveRotationSpeed()
        {
            //Move character smoothly from 0 to moveSpeed
            if (moveDirection.magnitude <= 0)
            {
                currentMoveSpeed = 0;
                currentRotationSpeed = 0;
                return;
            }

            //GetPlayerMovementSpeed
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, movementSmooth*Time.fixedDeltaTime);
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, rotationSpeed, rotationSmooth*Time.fixedDeltaTime);
            MoveDirection();

        }

        #region Movement
        private void MoveDirection()
        {
            MoveCharacter();
        }

        public void UpdateTargetDirectionAccordingTocamera()
        {
            var forward = Camera.main.transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            var right = Camera.main.transform.TransformDirection(Vector3.right);

            moveDirection = moveInput.y* forward + moveInput.x*right;
        }

        public virtual void MoveCharacter()
          {
            switch (playerMovement)
            {
                case PlayerMovementType.RIGIDBODY_MOVEPOSITION:
                    rigidbody.MovePosition(rigidbody.position + moveDirection * Time.fixedDeltaTime * currentMoveSpeed);
                    break;

                case PlayerMovementType.RIGIDBODY_VELOCITY:
                    moveDirection.y = 0;
                    //to find target oisition
                    Vector3 targetPosition = rigidbody.position + moveDirection * currentMoveSpeed *Time.fixedDeltaTime;

                    // to find target speed
                    float targetSpeed = currentMoveSpeed * moveDirection.magnitude;

                    Vector3 targetVelocity = (targetPosition - transform.position) / Time.fixedDeltaTime;
                    moveDirection.y = rigidbody.velocity.y;
                    rigidbody.velocity = targetVelocity;    
                    break;

                case PlayerMovementType.TRANSFORM_TRANSLATE:
                    transform.Translate(moveDirection * Time.deltaTime * currentMoveSpeed);
                    break;

                case PlayerMovementType.MOVE_TOWARD:
                    transform.position = Vector3.MoveTowards(transform.position, moveDirection * Time.fixedDeltaTime * currentMoveSpeed,0.1f);
                    break;
                default:
                    break;
            }
          }
        #endregion

        #region Rotation Handler
        /// <summary>
        /// Determine the direction the player will face based on input and the referenceTransform
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void UpdateRotation()
        {

            //There are two method you can use one of them
            //if (moveInput.magnitude > 0)
            //{
            //    Quaternion rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            //    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, currentRotationSpeed);
            //}

            Vector3 lookDirection = moveDirection.normalized;
            Quaternion freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;

            if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
            var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), 5 * Time.deltaTime); ;
        }
        #endregion

        #endregion
    }
}

