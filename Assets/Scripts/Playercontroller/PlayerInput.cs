using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController.InputController
{
    public class PlayerInputController
    {
        public static Vector3 GetMovementInput()
        {
            Vector3 moveVector = Vector3.zero;
            float horizontalAxis = Input.GetAxis("Horizontal");
            float verticalAxis = Input.GetAxis("Vertical");
            moveVector = Vector3.forward * horizontalAxis + Vector3.right* verticalAxis;
            moveVector = moveVector.normalized;
            return moveVector;
        }

        public static Quaternion GetCameraInput()
        {
            Vector3 cameraInput = Vector3.zero;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            return Quaternion.Euler(new Vector3(mouseX, mouseY));
        }

        public static bool IsJump()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }
    }
}

