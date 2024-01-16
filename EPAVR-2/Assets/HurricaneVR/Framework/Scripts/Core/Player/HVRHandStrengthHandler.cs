using HurricaneVR.Framework.Core.ScriptableObjects;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Player
{
    public class HVRHandStrengthHandler : MonoBehaviour
    {
        [Header("Debug")]
        public HVRJointSettings CurrentSettings;
        public bool LogStrengthChanges;

        [Tooltip("If true will update the joint every update - useful for tweaking HVRJointSettings in play mode.")]
        public bool AlwaysUpdateJoint;

        public HVRJointSettings JointSettings { get; private set; }

        public HVRJointSettings JointOverride { get; private set; }

        public HVRJointSettings HandGrabberOverride { get; private set; }

        public ConfigurableJoint Joint { get; set; }

        public bool Stopped { get; private set; }

        private JointDrive _stoppedDrive;

        protected virtual void Awake()
        {
            _stoppedDrive = new JointDrive();
            _stoppedDrive.maximumForce = 0f;
            _stoppedDrive.positionSpring = 0f;
            _stoppedDrive.positionDamper = 0f;
        }

        public void Initialize(HVRJointSettings defaultSettings)
        {
            JointSettings = defaultSettings;
            UpdateJoint();
        }

        protected virtual void FixedUpdate()
        {
            if (AlwaysUpdateJoint)
            {
                UpdateJoint();
            }
        }

        protected virtual void UpdateJoint()
        {
            if (Stopped)
                return;

            if (HandGrabberOverride)
            {
                UpdateStrength(HandGrabberOverride);
            }
            else if (JointOverride)
            {
                UpdateStrength(JointOverride);
            }
            else if (JointSettings)
            {
                UpdateStrength(JointSettings);
            }
        }

        protected virtual void UpdateStrength(HVRJointSettings settings)
        {
            if (settings)
                settings.ApplySettings(Joint);

            CurrentSettings = settings;

            if (LogStrengthChanges && settings)
            {
                Debug.Log($"{settings.name} applied.");
            }
        }


        public virtual void OverrideSettings(HVRJointSettings settings)
        {
            JointOverride = settings;
            UpdateJoint();
        }

        public virtual void OverrideHandSettings(HVRJointSettings settings)
        {
            HandGrabberOverride = settings;
            UpdateJoint();
        }

        public virtual void StopOverride()
        {
            JointOverride = null;
            UpdateJoint();
        }

        public virtual void Stop()
        {
            Stopped = true;
            Joint.xDrive = Joint.yDrive = Joint.zDrive = Joint.angularXDrive = Joint.angularYZDrive = Joint.slerpDrive = _stoppedDrive;
        }

        public virtual void Restart()
        {
            Stopped = false;
            UpdateJoint();
        }
    }
}