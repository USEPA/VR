using System;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRControllerOffset : MonoBehaviour
    {
        public HVRHandSide HandSide;

        public Transform Teleport;

        private HVRDevicePoseOffset _offsets;

        public Vector3 ControllerPositionOffset => _offsets != null ? _offsets.Position : Vector3.zero;
        public Vector3 ControllerRotationOffset => _offsets != null ? _offsets.Rotation : Vector3.zero;

        [Header("Debugging")]
        public Vector3 GrabPointPositionOffset;
        public Vector3 GrabPointRotationOffset;

        public Vector3 MiscPositionOffset;
        public Vector3 MiscRotationOffset;

        public bool LiveUpdateOffsets;
        private Quaternion _teleportStartRotation;

        protected virtual void Awake()
        {
            if (Teleport)
            {
                _teleportStartRotation = Teleport.localRotation;
            }
        }

        private void Start()
        {
            if (HandSide == HVRHandSide.Left)
            {
                if (HVRInputManager.Instance.LeftController)
                    ControllerConnected(HVRInputManager.Instance.LeftController);
                HVRInputManager.Instance.LeftControllerConnected.AddListener(ControllerConnected);
            }
            else
            {
                if (HVRInputManager.Instance.RightController)
                    ControllerConnected(HVRInputManager.Instance.RightController);
                HVRInputManager.Instance.RightControllerConnected.AddListener(ControllerConnected);
            }

           
        }

        public void Update()
        {
            if (LiveUpdateOffsets)
            {
                ApplyOffsets();
            }
        }

        public void SetMiscPositionOffset(Vector3 position, Vector3 rotation)
        {
            MiscPositionOffset = position;
            MiscRotationOffset = rotation;
            ApplyOffsets();
        }


        public void SetGrabPointOffsets(Vector3 position, Vector3 rotation)
        {
            GrabPointPositionOffset = position;
            GrabPointRotationOffset = rotation;
            ApplyOffsets();
        }

        public void ResetGrabPointOffsets()
        {
            GrabPointPositionOffset = Vector3.zero;
            GrabPointRotationOffset = Vector3.zero;
            ApplyOffsets();
        }

        public void ApplyOffsets()
        {
            var position = ControllerPositionOffset + GrabPointPositionOffset + MiscPositionOffset;

            if (HandSide == HVRHandSide.Left)
            {
                position.x *= -1f;
            }

            transform.localPosition = position;

            var controllerRotation = Quaternion.Euler(ControllerRotationOffset);
            var grabPointRotation = Quaternion.Euler(GrabPointRotationOffset);
            var miscRotation = Quaternion.Euler(MiscRotationOffset);

            var finalRotation = controllerRotation * grabPointRotation * miscRotation;
            var angles = finalRotation.eulerAngles;

            if (HandSide == HVRHandSide.Left)
            {
                angles.y *= -1f;
                angles.z *= -1f;
            }

            if (Teleport)
            {
                Teleport.localRotation = _teleportStartRotation * Quaternion.Inverse(grabPointRotation);
            }

            transform.localEulerAngles = angles;
        }

        private void ControllerConnected(HVRController controller)
        {
            var offsets = HVRInputManager.Instance.ControllerOffsets;
            if (!offsets)
            {
                Debug.LogWarning($"HVRInputManager.ControllerOffsets are not assigned.");
                return;
            }

            _offsets = offsets.GetDeviceOffset(controller.Side);
            //LiveUpdateOffsets = true;
            ApplyOffsets();
        }

    }

}
