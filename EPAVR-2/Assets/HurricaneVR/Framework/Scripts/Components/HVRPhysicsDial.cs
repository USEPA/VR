using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.ScriptableObjects;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class HVRPhysicsDial : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Local axis of rotation")]
        public HVRAxis Axis;

        [Tooltip("Rigidbody to connect the joint to")]
        public Rigidbody ConnectedBody;

        [Tooltip("If true the angular velocity will be zero'd out on release.")]
        public bool StopOnRelease = true;

        public bool DisableGravity = true;
        
        [Header("Joint Limits")]
        public bool LimitRotation;

        [Tooltip("Minimum Angle about the axis of rotation")]
        public float MinAngle;

        [Tooltip("Maximum rotation about the axis of rotation")]
        public float MaxAngle;

        [Header("Joint Settings")]
        [Tooltip("Angular Damper when the dial is grabbed")]
        public float GrabbedDamper = 3;

        [Tooltip("Angular Damper when the dial is not grabbed")]
        public float Damper = 3;

        public float Spring;

        [Header("Editor")]
        [SerializeField]
        protected Quaternion JointStartRotation;

        [Header("Debugging Tools")]
        public float TargetAngularVelocity = 0f;

        public Rigidbody Rigidbody { get; private set; }

        public HVRGrabbable Grabbable { get; private set; }

        protected ConfigurableJoint Joint { get; set; }

        protected virtual void Awake()
        {
            Rigidbody = this.GetRigidbody();

            Rigidbody.useGravity = !DisableGravity;

            Grabbable = GetComponent<HVRGrabbable>();
            if (Grabbable)
            {
                Grabbable.HandGrabbed.AddListener(OnDialGrabbed);
                Grabbable.HandReleased.AddListener(OnDialReleased);
            }

            if (MinAngle > 0)
            {
                MinAngle *= -1;
            }

            if (MaxAngle < 0)
            {
                MaxAngle *= -1;
            }

            MinAngle = Mathf.Clamp(MinAngle, MinAngle, 0);
            MaxAngle = Mathf.Clamp(MaxAngle, 0, MaxAngle);

            SetupJoint();
        }

        protected virtual void Start()
        {
           
        }

        protected virtual void Update()
        {
            Joint.targetAngularVelocity = new Vector3(TargetAngularVelocity, 0f, 0f);
        }

        private void OnDialReleased(HVRHandGrabber arg0, HVRGrabbable arg1)
        {
            Joint.SetAngularXDrive(Spring, Damper, 10000f);
            if (StopOnRelease)
            {
                Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void OnDialGrabbed(HVRHandGrabber arg0, HVRGrabbable arg1)
        {
            Joint.SetAngularXDrive(0f, GrabbedDamper, 10000f);
        }

        private void SetupJoint()
        {
            var currentRotation = transform.localRotation;
            transform.localRotation = JointStartRotation;
            Joint = gameObject.AddComponent<ConfigurableJoint>();
            Joint.connectedBody = ConnectedBody;
            Joint.LockLinearMotion();
            Joint.LockAngularYMotion();
            Joint.LockAngularZMotion();

            Joint.axis = Axis.GetVector();

            if (LimitRotation)
            {
                ResetLimits();
            }
            else
            {
                Joint.angularXMotion = ConfigurableJointMotion.Free;
            }

            Joint.secondaryAxis = Joint.axis.OrthogonalVector();
            Joint.SetAngularXDrive(Spring, Damper, 10000f);
            Joint.projectionAngle = 1f;
            Joint.projectionDistance = .01f;
            Joint.projectionMode = JointProjectionMode.PositionAndRotation;
            transform.localRotation = currentRotation;
        }

        public void SetLimits(float minAngle, float maxAngle)
        {
            Joint.LimitAngularXMotion();
            Joint.SetAngularXHighLimit(minAngle);
            Joint.SetAngularXLowLimit(maxAngle);
        }

        public void ResetLimits()
        {
            Joint.LimitAngularXMotion();
            Joint.SetAngularXHighLimit(-MinAngle);
            Joint.SetAngularXLowLimit(-MaxAngle);
        }
    }
}