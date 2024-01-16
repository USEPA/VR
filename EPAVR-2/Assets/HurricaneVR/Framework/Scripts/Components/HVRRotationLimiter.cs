using System;
using HurricaneVR.Framework.Core.ScriptableObjects;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;


namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HVRRotationTracker))]
    public class HVRRotationLimiter : MonoBehaviour
    {
        public const float PhysxMaxLimit = 177f;

        public Rigidbody ConnectedBody;
        public int MinAngle;
        public int MaxAngle;
        public float JointResetThreshold = 90f;


        public Rigidbody Rigidbody { get; private set; }

        public HVRRotationTracker Tracker { get; private set; }

        private ConfigurableJoint _joint;
        private float _angleAtCreation;
        
        [Header("Debugging")]
        public float maxDelta;
        public float minDelta;

        protected virtual void Start()
        {
            Rigidbody = this.GetRigidbody();
            Tracker = GetComponent<HVRRotationTracker>();

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
        }

        protected virtual void FixedUpdate()
        {
            var angle = Mathf.Clamp(Tracker.Angle, MinAngle, MaxAngle);
            minDelta = Math.Abs(MinAngle - angle);
            maxDelta = MaxAngle - angle;

            var force = false;
            if (_joint)
            {
                var angleFromJointCreation = angle - _angleAtCreation;
                var angleDelta = Mathf.Abs(angleFromJointCreation);

                if (angleDelta > JointResetThreshold)
                {
                    Destroy(_joint);
                    force = true;
                }
            }

            if (!_joint || force)
            {
                if (minDelta < PhysxMaxLimit || maxDelta < PhysxMaxLimit)
                {
                    _joint = gameObject.AddComponent<ConfigurableJoint>();
                    _joint.axis = Tracker.AxisOfRotation;
                    _joint.secondaryAxis = Tracker.AxisOfRotation.OrthogonalVector();
                    _joint.LimitAngularXMotion();
                    _joint.connectedBody = ConnectedBody;

                    _angleAtCreation = Tracker.Angle;

                    if (minDelta < PhysxMaxLimit)
                    {
                        _joint.SetAngularXHighLimit(minDelta);
                    }
                    else
                    {
                        _joint.SetAngularXHighLimit(PhysxMaxLimit);
                    }

                    if (maxDelta < PhysxMaxLimit)
                    {
                        _joint.SetAngularXLowLimit(-maxDelta);
                    }
                    else
                    {
                        _joint.SetAngularXLowLimit(-PhysxMaxLimit);
                    }
                }
            }
        }
    }
}