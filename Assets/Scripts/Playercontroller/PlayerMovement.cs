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

        [SerializeField] private float moveSpeed;
        [Range(0,10)]
        [SerializeField] private float movementSmooth;
        private float currentMoveSpeed;

        [SerializeField] private float rotationSpeed;
        [Range(0, 10)]
        [SerializeField] private float rotationSmooth;
        private float currentRotationSpeed;

        public float jumpHeight;
        private bool isJumping;

        private void FixedUpdate()
        {
            Physics.SyncTransforms();
            MoveRotationSpeed();
            JumpAction();
        }
        #region Ground Check
        private void CheckGround()
        {

        }
        #endregion

        #region Action
        private void JumpAction()
        {
            isJumping = PlayerInput.IsJump();

            if (!isJumping)
            {
                return;
            }

             isJumping = false;
             var vel = rigidbody.velocity;
             vel.y = jumpHeight;
             rigidbody.velocity = vel;
            
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

