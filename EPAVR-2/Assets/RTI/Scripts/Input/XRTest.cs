using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace L58.EPAVR
{
    public class XRTest : MonoBehaviour
    {
        #region Public Variables
        [Header("Debug Values")]
        public float RightHandGripValue = 0.0f;
        public float RightHandTriggerValue = 0.0f;
        public Vector3 RightHandPosition;
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (SimulationManager.Instance)
            {
                // Get the grip amount
                SimulationManager.Instance.InputRefs.GetAction(XRActionMap.RightHand, XRAction.Grip)
                    .action.performed += i => RightHandGripValue = i.ReadValue<float>();
                SimulationManager.Instance.InputRefs.GetAction(XRActionMap.RightHand, XRAction.Trigger)
                    .action.performed += i => RightHandTriggerValue = i.ReadValue<float>();
                SimulationManager.Instance.InputRefs.GetAction(XRActionMap.RightHand, XRAction.GripPressed)
                    .action.performed += GripPressed;
                SimulationManager.Instance.InputRefs.GetAction(XRActionMap.RightHand, XRAction.TriggerPressed)
                    .action.performed += TriggerPressed;
                SimulationManager.Instance.InputRefs.GetAction(XRActionMap.RightHand, XRAction.Position)
                    .action.performed += i => RightHandPosition = i.ReadValue<Vector3>();
            }
        }

        public void GripPressed(InputAction.CallbackContext context)
        {
            UnityEngine.Debug.Log($"GRIP PRESSED || Time: {Time.time}");
        }

        public void TriggerPressed(InputAction.CallbackContext context)
        {
            UnityEngine.Debug.Log($"TRIGGER PRESSED || Time: {Time.time}");
        }

    }
}

