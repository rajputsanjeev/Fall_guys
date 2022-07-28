using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "Invector/Resources/New CameraState List Data", order = 1)]
public class vThirdPersonCameraListData : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField] public List<vThirdPersonCameraState> tpCameraStates;

        public vThirdPersonCameraListData()
        {
            tpCameraStates = new List<vThirdPersonCameraState>();
            tpCameraStates.Add(new vThirdPersonCameraState("Default"));
        }
    }
