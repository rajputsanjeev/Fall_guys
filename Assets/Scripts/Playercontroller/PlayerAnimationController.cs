using Expension;
using PlayerController.InputController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private Vector3 lastCharacterAngle;

        private float inputMagnitude;
        private float rotationMagnitude;

        public const float walkSpeed = 0.5f;
        public const float runningSpeed = 1f;
        public const float sprintSpeed = 1.5f;

        private MovementSmoothDamp movementSmoothDamp = new MovementSmoothDamp();

        private void FixedUpdate()
        {
            //Calculate the rotating magnitude
            CalculateRotationMagnitude();

            //calculate the direction by normalize the vecrore
            Vector3 _direction = PlayerInput.GetMovementInput();

            // set the animator speed
            SetAnimatorMoveSpeed(_direction);

            // update animator by input
            UpdateAnimatorParameters();

            //jump state
            if (PlayerInput.IsJump())
            {
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            }
        }

        private void SetAnimatorMoveSpeed(Vector3 direction)
        {
            Vector3 relativeInput = transform.InverseTransformDirection(direction);
            inputMagnitude = Mathf.Lerp(relativeInput.magnitude, runningSpeed, Time.deltaTime);
        }

        protected virtual void CalculateRotationMagnitude()
        {
            var eulerDifference = this.transform.eulerAngles - lastCharacterAngle;
            if (eulerDifference.sqrMagnitude < 0.01)
            {
                lastCharacterAngle = transform.eulerAngles;
                rotationMagnitude = 0f;
                return;
            }

            var magnitude = (eulerDifference.NormalizeAngle().y / movementSmoothDamp.rotationSpeed);
            rotationMagnitude = (float)System.Math.Round(magnitude, 2);
            lastCharacterAngle = transform.eulerAngles;
        }


        private void UpdateAnimatorParameters()
        {
            animator.SetFloat(AnimatorParameters.inputMagnitide, inputMagnitude, movementSmoothDamp.inputMagnitude, Time.fixedDeltaTime);
            animator.SetFloat(AnimatorParameters.magnitudeRotation, rotationMagnitude, movementSmoothDamp.leanSmooth, Time.fixedDeltaTime);
        }

        public partial class AnimatorParameters 
        {
            public static int inputMagnitide = Animator.StringToHash("InputMagnitude");
            public static int magnitudeRotation = Animator.StringToHash("MagnitudeRotation");
        }

        public class MovementSmoothDamp 
        {
            public float inputMagnitude = 0.2f;
            public  float rotationSpeed = 5f;
            public float leanSmooth = 0.2f;
        }

    }

}

