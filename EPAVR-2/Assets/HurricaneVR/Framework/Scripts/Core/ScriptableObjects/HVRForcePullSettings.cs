using HurricaneVR.Framework.Core.Utils;
using UnityEngine;

namespace HurricaneVR.Framework.Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "HurricaneVR/Force Pull Settings", fileName = "ForcePullSettings")]
    public class HVRForcePullSettings : ScriptableObject
    {
        [Tooltip("Distance to the hand when auto grab will occur for non dynamic posed grabs.")]
        [Range(.1f, .3f)]
        public float DistanceThreshold = .1f;

        [Tooltip("Distance to the hand when auto grab will occur for dynamic posed grabs.")]
        [Range(.1f, .3f)]
        public float DynamicGrabThreshold = .2f;

        [Tooltip("Max linear velocity the object will move toward your hand")]
        public float Speed = 10f;


        [Tooltip("Velocity magnitude cap after releasing this object and not grabbing it")]
        public float MaxMissSpeed = 1f;
        [Tooltip("Max Angular Velocity after releasing this object and not grabbing it")]
        public float MaxMissAngularSpeed = 1f;

        [Header("Pose Rotation Trigger")]
        [Tooltip("What causes the rotation to the pose rotation to start?")]
        public ForcePullRotationTrigger RotationTrigger = ForcePullRotationTrigger.PercentTraveled;

        [Tooltip("DistancetoHand Trigger: Once distance from the hand to the object is below this value, the object will rotate into pose orientation.")]
        [DrawIf("RotationTrigger", ForcePullRotationTrigger.DistanceToHand)]
        public float RotateTriggerDistance = 1.5f;

        [Tooltip("TimeSinceStart Trigger: Start rotating after pulling for this amount of time if TimeSinceStart mode")]
        [DrawIf("RotationTrigger", ForcePullRotationTrigger.TimeSinceStart)]
        public float RotateTriggerTime = .40f;

        [Tooltip("PercentTraveled: Start rotating after traveling this percentage of initial distance to the hand.")]
        [DrawIf("RotationTrigger", ForcePullRotationTrigger.PercentTraveled)]
        public float RotateTriggerPercent = 30f;


        [Header("Pose Rotation Style")]
        public ForceRotationStyle RotationStyle = ForceRotationStyle.RotateOverRemaining;

        [Tooltip("Rotation Max Velocity is calculated based on Speed over this distance")]
        [DrawIf("RotationStyle", ForceRotationStyle.RotateOverDistance)]
        public float RotateOverDistance = .5f;

        [Header("Joint Drive")]
        public float Spring = 3000;
        public float Damper = 100;
        public float MaxForce = 1000f;

        [Header("Rotation Drive")]
        public float SlerpSpring = 100f;
        public float SlerpDamper = 5f;
        public float SlerpMaxForce = 1000f;
    }

    public enum ForcePullRotationTrigger
    {
        DistanceToHand, //once within this range to the hand
        TimeSinceStart, //after elapsed time since start of pull
        PercentTraveled //remaining distance / distance at time of grab
    }

    public enum ForceRotationStyle
    {
        RotateOverDistance,
        RotateOverRemaining
    }
}