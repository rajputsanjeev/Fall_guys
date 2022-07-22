using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController.InputController;

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

        public float jumpHeight = 9.8f;

        public float groundDistance;
        private RaycastHit groundHit;
       [SerializeField] private LayerMask grounded;

        [Tooltip("The length of the Ray cast to detect ground ")]
        public float groundDetectionDistance = 10f;
        [Tooltip("Distance to became not grounded")]
        [Range(0, 10)]
        public float groundMinDistance = 0.1f;
        [Range(0, 10)]
        public float groundMaxDistance = 0.5f;
        public bool jumpWithRigidbodyForce;
        private float extraGravity = -10f;
        public float gravityScale = 10f;

        private void FixedUpdate()
        {
            Physics.SyncTransforms();
            MoveRotationSpeed();
            ChekGroundDistance();
            JumpBehaviour();
        }

        #region Ground Check
        public bool IsGrounded()
        {
           Physics.SphereCast(capsuleCollider.bounds.center,capsuleCollider.radius , Vector3.down ,out groundHit,2f,grounded ,QueryTriggerInteraction.UseGlobal);
            return (groundHit.collider != null);
        }
        #endregion

        #region Action
        private void JumpBehaviour()
        {
            Debug.Log("IsGrounded " + IsGrounded());

            if (IsGrounded() && PlayerInput.IsJump())
            {
                if (jumpWithRigidbodyForce)
                {
                    rigidbody.AddForce(Vector3.up * 35f, ForceMode.Impulse);
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
            if (PlayerInput.GetMovementInput().magnitude <= 0)
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

        #region

        private void MoveDirection()
        {
            Vector3 _direction = PlayerInput.GetMovementInput();
            MoveCharacter(_direction);
            UpdateRotation(_direction);
        }

        public virtual void MoveCharacter(Vector3 _direction)
          {
            switch (playerMovement)
            {
                case PlayerMovementType.RIGIDBODY_MOVEPOSITION:
                    rigidbody.MovePosition(rigidbody.position + _direction * Time.fixedDeltaTime * currentMoveSpeed);
                    break;

                case PlayerMovementType.RIGIDBODY_VELOCITY:
                    _direction.y = 0;
                    //to find target oisition
                    Vector3 targetPosition = rigidbody.position + _direction * currentMoveSpeed *Time.fixedDeltaTime;

                    // to find target speed
                    float tragetSpeed = currentMoveSpeed * _direction.magnitude;

                    Vector3 targetVelocity = (targetPosition - transform.position) / Time.fixedDeltaTime;
                    _direction.y = rigidbody.velocity.y;
                    rigidbody.velocity = targetVelocity;    
                    break;

                case PlayerMovementType.TRANSFORM_TRANSLATE:
                    transform.Translate(_direction * Time.deltaTime * currentMoveSpeed);
                    break;

                case PlayerMovementType.MOVE_TOWARD:
                    transform.position = Vector3.MoveTowards(transform.position,_direction * Time.fixedDeltaTime * currentMoveSpeed,0.1f);
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
        public virtual void UpdateRotation(Vector3 _direction)
        {
            if (PlayerInput.GetMovementInput().magnitude > 0)
            {
                Quaternion rotation = Quaternion.LookRotation(PlayerInput.GetMovementInput(), Vector3.up);
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, currentRotationSpeed);
            }
        }
        #endregion

        #endregion
    }
}

