using HurricaneVR.Framework.Core.HandPoser.Data;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Serialization;
using Time = UnityEngine.Time;

namespace HurricaneVR.Framework.Core.HandPoser
{
    public class HVRHandAnimator : MonoBehaviour
    {
        [FormerlySerializedAs("PosePosAndRot")]
        [Header("Settings")]
        [Tooltip("True for floaty hands, false for FinalIK hands")]
        public bool PoseHand = true;

        [Tooltip("If true the default poser will pause the hand")]
        public bool DefaultPoseHand = true;

        [Tooltip("Finger bend speed when dynamic pose is active")]
        public float DynamicPoseSpeed = 16f;

        [Header("Components")]
        public HVRPhysicsPoser PhysicsPoser;
        public HVRPosableHand Hand;
        public HVRHandPoser DefaultPoser;


        [Header("Debug View")]
        public HVRHandPoser CurrentPoser;

        /// <summary>
        /// Current hand pose, moves towards BlendedPose based on the speed defined on the Primary Pose
        /// </summary>
        private HVRHandPoseData CurrentPose;

        /// <summary>
        /// Resting hand pose when not holding anything
        /// </summary>
        private HVRHandPoseData DefaultPose;

        /// <summary>
        /// Maintains the state the hand should be moving towards
        /// </summary>
        private HVRHandPoseData BlendedPose;

        /// <summary>
        /// Current Primary Pose of the active Hand Poser
        /// </summary>
        private HVRHandPoseData PrimaryPose;


        public bool IsMine { get; set; } = true;

        /// <summary>
        /// Defaults to the finger curl arrays managed by the framework in Start(). Can be overriden after start with a float[5] array if you want to supply your own curl data.
        /// </summary>
        public float[] FingerCurlSource { get; set; }

        /// <summary>
        /// Enable to disable finger curl influence on the hand pose
        /// </summary>
        public bool IgnoreCurls { get; set; } = false;

        public bool DynamicPose { get; set; }

        private bool _poseHand = true;
        private float[] _fingerCurls;
        private float _lerp;

        protected virtual void Start()
        {
            _fingerCurls = new float[5];

            if (!PhysicsPoser)
            {
                PhysicsPoser = GetComponent<HVRPhysicsPoser>();
            }

            if (!DefaultPoser)
            {
                DefaultPoser = GetComponent<HVRHandPoser>();
            }

            if (!Hand)
            {
                Hand = GetComponent<HVRPosableHand>();
            }


            DefaultPose = DefaultPoser.PrimaryPose.Pose.GetPose(Hand.IsLeft).DeepCopy();
            CurrentPose = DefaultPose.DeepCopy();
            BlendedPose = DefaultPose.DeepCopy();

            if (IsMine)
            {
                FingerCurlSource = Hand.IsLeft ? HVRController.LeftFingerCurls : HVRController.RightFingerCurls;
            }

            ResetToDefault();


            if (DefaultPoser.Blends != null)
            {
                for (var i = 0; i < DefaultPoser.Blends.Count; i++)
                {
                    if (DefaultPoser.Blends[i].Type == BlendType.Immediate)
                    {
                        DefaultPoser.Blends[i].Type = BlendType.Manual;
                    }
                }
            }
        }

        protected virtual void LateUpdate()
        {
            UpdateFingerCurls();
            UpdatePoser();
        }

        protected virtual void UpdateFingerCurls()
        {
            if (FingerCurlSource == null)
                return;

            for (int i = 0; i < 5; i++)
            {
                _fingerCurls[i] = FingerCurlSource[i];
            }
        }

        public void ZeroFingerCurls()
        {
            for (int i = 0; i < 5; i++)
            {
                _fingerCurls[i] = 0f;
            }
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        private void UpdatePoser()
        {
            if (DynamicPose)
            {
                PrimaryPose.CopyTo(BlendedPose);

                for (int i = 0; i < 5; i++)
                {
                    _lerps[i] = Time.deltaTime * DynamicPoseSpeed;
                }

                ApplyBlend(CurrentPose, BlendedPose, DefaultPoser.PrimaryPose);
                Hand.Pose(CurrentPose, false);
                return;
            }

            if (CurrentPoser == null)
            {
                return;
            }

            UpdateBlends();
            ApplyBlending();
            Hand.Pose(CurrentPose, _poseHand);
        }

        private void UpdateBlends()
        {
            if (!IsMine)
                return;

            var primaryLerp = UpdateBlend(CurrentPoser.PrimaryPose);

            _handLerp += primaryLerp;

            for (int i = 0; i < 5; i++)
            {
                _lerps[i] = primaryLerp;
            }

            if (CurrentPoser.Blends == null)
            {
                return;
            }

            for (int i = 0; i < CurrentPoser.Blends.Count; i++)
            {
                var blend = CurrentPoser.Blends[i];
                if (blend.Disabled)
                {
                    continue;
                }

                var val = UpdateBlend(blend);

                if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & HVRHandPoseMask.Hand) == HVRHandPoseMask.Hand)
                {
                    _handLerp += val;
                }

                for (int j = 0; j < 5; j++)
                {
                    HVRHandPoseMask mask;
                    if (j == 0) mask = HVRHandPoseMask.Thumb;
                    else if (j == 1) mask = HVRHandPoseMask.Index;
                    else if (j == 2) mask = HVRHandPoseMask.Middle;
                    else if (j == 3) mask = HVRHandPoseMask.Ring;
                    else if (j == 4) mask = HVRHandPoseMask.Pinky;
                    else continue;

                    if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & mask) == mask)
                    {
                        _lerps[j] += val;
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                _lerps[i] = Mathf.Clamp01(_lerps[i]);
            }
        }

        private float UpdateBlend(HVRHandPoseBlend blend)
        {
            if (blend.Type == BlendType.Immediate)
            {
                blend.Value = 1f;
            }
            else if (blend.ButtonParameter)
            {
                var button = HVRController.GetButtonState(Hand.Side, blend.Button);
                if (blend.Type == BlendType.BooleanParameter)
                {
                    blend.Value = button.Active ? 1f : 0f;
                }
                else if (blend.Type == BlendType.FloatParameter)
                {
                    blend.Value = button.Value;
                    return blend.Value * blend.Weight;
                }
            }
            else if (!string.IsNullOrWhiteSpace(blend.AnimationParameter) && blend.AnimationParameter != "None")
            {
                if (blend.Type == BlendType.BooleanParameter)
                {
                    blend.Value = HVRAnimationParameters.GetBoolParameter(Hand.Side, blend.AnimationParameter) ? 1f : 0f;
                }
                else if (blend.Type == BlendType.FloatParameter)
                {
                    blend.Value = HVRAnimationParameters.GetFloatParameter(Hand.Side, blend.AnimationParameter);
                    return blend.Value * blend.Weight;
                }
            }

            return Time.deltaTime * blend.Value * blend.Speed * blend.Weight;
        }


        private void ApplyBlending()
        {
            PrimaryPose.CopyTo(BlendedPose);
            ApplyFingerCurls(DefaultPose, PrimaryPose, BlendedPose, CurrentPoser.PrimaryPose);

            if (CurrentPoser.Blends != null)
            {
                for (int i = 0; i < CurrentPoser.Blends.Count; i++)
                {
                    var blend = CurrentPoser.Blends[i];

                    if (blend.Disabled || !blend.Pose)
                    {
                        continue;
                    }

                    var blendPose = blend.Pose.GetPose(Hand.Side);

                    if (blendPose == null)
                        continue;

                    ApplyFingerCurls(PrimaryPose, blendPose, BlendedPose, blend);
                    UpdateTarget(BlendedPose, blendPose, BlendedPose, blend);
                }
            }

            ApplyBlend(CurrentPose, BlendedPose, CurrentPoser.PrimaryPose);
        }

        private readonly float[] _lerps = new float[5];
        private float _handLerp;

        /// <summary>
        /// Adjusts the pose target by lerping between the hand relaxed pose and the current target hand pose
        /// </summary>
        private void ApplyFingerCurls(HVRHandPoseData startPose, HVRHandPoseData targetPose, HVRHandPoseData adjustedTarget, HVRHandPoseBlend blend)
        {
            for (int i = 0; i < adjustedTarget.Fingers.Length; i++)
            {
                var adjustedTargetFinger = adjustedTarget.Fingers[i];
                var startFinger = startPose.Fingers[i];
                var targetFinger = targetPose.Fingers[i];

                var fingerType = blend.GetFingerType(i);

                if (fingerType != HVRFingerType.Close)
                    continue;

                var fingerStart = blend.GetFingerStart(i);
                var curl = _fingerCurls[i];

                if (IgnoreCurls)
                {
                    curl = 0f;
                }

                var remainder = 1 - fingerStart;
                curl = fingerStart + curl * remainder;
                curl = Mathf.Clamp(curl, 0f, 1f);


                for (int j = 0; j < adjustedTargetFinger.Bones.Count; j++)
                {
                    adjustedTargetFinger.Bones[j].Position = Vector3.Lerp(startFinger.Bones[j].Position, targetFinger.Bones[j].Position, curl);
                    adjustedTargetFinger.Bones[j].Rotation = Quaternion.Lerp(startFinger.Bones[j].Rotation, targetFinger.Bones[j].Rotation, curl);
                }
            }
        }


        private void UpdateTarget(HVRHandPoseData startPose, HVRHandPoseData targetPose, HVRHandPoseData blendedPose, HVRHandPoseBlend blend)
        {
            var lerp = blend.Value * blend.Weight;

            if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & HVRHandPoseMask.Hand) == HVRHandPoseMask.Hand)
            {
                blendedPose.Position = Vector3.Lerp(startPose.Position, targetPose.Position, _handLerp);
                blendedPose.Rotation = Quaternion.Lerp(startPose.Rotation, targetPose.Rotation, _handLerp);
            }

            for (var i = 0; i < blendedPose.Fingers.Length; i++)
            {
                var blendedFinger = blendedPose.Fingers[i];
                var targetFinger = targetPose.Fingers[i];
                var startFinger = startPose.Fingers[i];

                HVRHandPoseMask mask;
                if (i == 0) mask = HVRHandPoseMask.Thumb;
                else if (i == 1) mask = HVRHandPoseMask.Index;
                else if (i == 2) mask = HVRHandPoseMask.Middle;
                else if (i == 3) mask = HVRHandPoseMask.Ring;
                else if (i == 4) mask = HVRHandPoseMask.Pinky;
                else continue;

                if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & mask) == mask)
                {
                    for (var j = 0; j < blendedFinger.Bones.Count; j++)
                    {
                        var blendedBone = blendedFinger.Bones[j];
                        var targetBone = targetFinger.Bones[j];
                        var startBone = startFinger.Bones[j];

                        blendedBone.Position = Vector3.Lerp(startBone.Position, targetBone.Position, lerp);
                        blendedBone.Rotation = Quaternion.Lerp(startBone.Rotation, targetBone.Rotation, lerp);
                    }
                }
            }
        }

        private void ApplyBlend(HVRHandPoseData currentHand, HVRHandPoseData targetHandPose, HVRHandPoseBlend blend)
        {
            var lerp = blend.Value * blend.Weight;

            if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & HVRHandPoseMask.Hand) == HVRHandPoseMask.Hand)
            {
                currentHand.Position = Vector3.Lerp(currentHand.Position, targetHandPose.Position, lerp);
                currentHand.Rotation = Quaternion.Lerp(currentHand.Rotation, targetHandPose.Rotation, lerp);
            }

            for (var i = 0; i < currentHand.Fingers.Length; i++)
            {
                var currentFinger = currentHand.Fingers[i];
                var targetFinger = targetHandPose.Fingers[i];

                HVRHandPoseMask mask;
                if (i == 0) mask = HVRHandPoseMask.Thumb;
                else if (i == 1) mask = HVRHandPoseMask.Index;
                else if (i == 2) mask = HVRHandPoseMask.Middle;
                else if (i == 3) mask = HVRHandPoseMask.Ring;
                else if (i == 4) mask = HVRHandPoseMask.Pinky;
                else continue;

                if (blend.Mask == HVRHandPoseMask.None || (blend.Mask & mask) == mask)
                {
                    for (var j = 0; j < currentFinger.Bones.Count; j++)
                    {
                        var currentBone = currentFinger.Bones[j];
                        var targetBone = targetFinger.Bones[j];

                        currentBone.Position = Vector3.Lerp(currentBone.Position, targetBone.Position, _lerps[i]);
                        currentBone.Rotation = Quaternion.Lerp(currentBone.Rotation, targetBone.Rotation, _lerps[i]);

                        //if (i == 1 && j == 0)
                        //{
                        //    if (Hand.Side == HVRHandSide.Right)
                        //        Debug.Log($"{Quaternion.Angle(currentBone.Rotation, targetBone.Rotation)}");
                        //}
                    }
                }
            }
        }



        public void ResetIfNotDefault()
        {
            if (CurrentPoser != DefaultPoser)
                ResetToDefault();
        }

        public void ResetToDefault()
        {
            _poseHand = true;
            DynamicPose = false;
            if (DefaultPoser != null)
            {
                SetCurrentPoser(DefaultPoser, DefaultPoseHand);
            }
            else
            {
                Debug.Log("Default poser not set.");
            }
        }

        public void StartDynamicPose(HVRHandPoseData pose)
        {
            CurrentPoser = null;
            DynamicPose = true;
            PrimaryPose = pose;
            _poseHand = false;
        }

        public void SetCurrentPoser(HVRHandPoser poser, bool poseHand = true)
        {
            _poseHand = poseHand;
            if (!PoseHand)
            {
                //hand grabber handles posing the IKTarget since the posable hand component is placed on the avatar itself
                _poseHand = false;
            }

            CurrentPoser = poser;
            if (poser == null)
                return;

            if (poser.PrimaryPose == null)
            {
                return;
            }

            PrimaryPose = poser.PrimaryPose.Pose.GetPose(Hand.IsLeft);
        }
    }
}
