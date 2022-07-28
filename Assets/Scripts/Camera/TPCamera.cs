using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPCamera : MonoBehaviour
{

    public Transform target;
    public vThirdPersonCameraState currentState;
    public vThirdPersonCameraState lerpState;
    public vThirdPersonCameraListData CameraStateList;
    public float mouseX;
    public float mouseY;
    public float clipPlaneMargin;
    public Vector3 refVelocity;
    public Vector3 currentRotation;
    private Vector3 current_cameraPos;
    private Vector3 desired_camraPos;
     private PlayerInput inputActions;
    public bool showGizmos;

    private void Start()
    {
        inputActions = new PlayerInput();
        inputActions.Enable();
        ChangeState("Default");
    }


    public void LateUpdate()
    {
        CameraMovement(inputActions.Camera.MouseX.ReadValue<float>(), inputActions.Camera.MouseY.ReadValue<float>());
    }

    public void CameraMovement(float x , float y)
    {
        if (!target)
            return;

            mouseX += x * currentState.xMouseSensitivity;
            mouseY -= y * currentState.yMouseSensitivity;
            mouseX = Mathf.Clamp(mouseX, currentState.xMinLimit, currentState.xMaxLimit);
            mouseY = Mathf.Clamp(mouseY, currentState.yMinLimit, currentState.yMaxLimit);
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(mouseY, mouseX), ref refVelocity, 0.1f);
            transform.eulerAngles = currentRotation;
            transform.position = target.position - transform.forward * currentState.defaultDistance + transform.up * currentState.height + transform.right * currentState.right;
 
    }

    public void ChangeState(string stateName)
    {
        vThirdPersonCameraState state = CameraStateList != null ? CameraStateList.tpCameraStates.Find(delegate (vThirdPersonCameraState obj) { return obj.Name.Equals(stateName); }) : new vThirdPersonCameraState("Default");
        currentState = state;

    }

    public  ClipPlanePoints NearClipPlanePoints(Camera camera, Vector3 pos, float clipPlaneMargin)
    {
        var clipPlanePoints = new ClipPlanePoints();

        var transform = camera.transform;
        var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
        var aspect = camera.aspect;
        var distance = camera.nearClipPlane;
        var height = distance * Mathf.Tan(halfFOV);
        var width = height * aspect;
        height *= 1 + clipPlaneMargin;
        width *= 1 + clipPlaneMargin;
        clipPlanePoints.LowerRight = pos + transform.right * width;
        clipPlanePoints.LowerRight -= transform.up * height;
        clipPlanePoints.LowerRight += transform.forward * distance;

        clipPlanePoints.LowerLeft = pos - transform.right * width;
        clipPlanePoints.LowerLeft -= transform.up * height;
        clipPlanePoints.LowerLeft += transform.forward * distance;

        clipPlanePoints.UpperRight = pos + transform.right * width;
        clipPlanePoints.UpperRight += transform.up * height;
        clipPlanePoints.UpperRight += transform.forward * distance;

        clipPlanePoints.UpperLeft = pos - transform.right * width;
        clipPlanePoints.UpperLeft += transform.up * height;
        clipPlanePoints.UpperLeft += transform.forward * distance;

        return clipPlanePoints;
    }

    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;
        if (showGizmos)
        {
            Debug.DrawRay(from, _to.LowerLeft - from, color);
            Debug.DrawLine(_to.LowerLeft, _to.LowerRight, color);
            Debug.DrawLine(_to.UpperLeft, _to.UpperRight, color);
            Debug.DrawLine(_to.UpperLeft, _to.LowerLeft, color);
            Debug.DrawLine(_to.UpperRight, _to.LowerRight, color);
            Debug.DrawRay(from, _to.LowerRight - from, color);
            Debug.DrawRay(from, _to.UpperLeft - from, color);
            Debug.DrawRay(from, _to.UpperRight - from, color);
        }
        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            currentState.cullingMinDist = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (currentState.cullingMinDist > hitInfo.distance) currentState.cullingMinDist = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (currentState.cullingMinDist > hitInfo.distance) currentState.cullingMinDist = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (currentState.cullingMinDist > hitInfo.distance) currentState.cullingMinDist = hitInfo.distance;
        }

        return value;
    }
}

[System.Serializable]
 public struct ClipPlanePoints
{
    public Vector3 UpperLeft;
    public Vector3 UpperRight;
    public Vector3 LowerLeft;
    public Vector3 LowerRight;
}