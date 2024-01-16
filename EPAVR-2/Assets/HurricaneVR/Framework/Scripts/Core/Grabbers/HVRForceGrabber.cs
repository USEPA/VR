using System;
using System.Collections;
using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Core.HandPoser;
using HurricaneVR.Framework.Core.ScriptableObjects;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Grabbers
{
    public class HVRForceGrabber : HVRGrabberBase
    {
        [Header("Components")]
        public HVRForceGrabberLaser Laser;
        public HVRHandGrabber HandGrabber;
        public HVRGrabbableHoverBase GrabIndicator;
        public HVRHandPoser GrabPoser;
        public HVRHandPoser HoverPoser;

        [Header("Settings")]
        public HVRForceGrabMode GrabStyle = HVRForceGrabMode.ForcePull;

        public AudioClip SFXGrab;

        [DrawIf("GrabStyle", HVRForceGrabMode.ForcePull)]
        public HVRForcePullSettings ForcePullSettings;

        //[Header("Gravity Gloves Style Settings")]
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public bool RequiresFlick;

        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float ForceTime = 1f;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float YOffset = .3f;

        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float FlickStartThreshold = 1.25f;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float FlickEndThreshold = .25f;

        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float QuickMoveThreshold = 1.25f;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float QuickMoveResetThreshold = .25f;

        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float MaximumVelocityPostCollision = 5f;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float MaximumVelocityAutoGrab = 5f;

        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public bool AutoGrab = true;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float AdditionalAutoGrabTime = 1f;
        [DrawIf("GrabStyle", HVRForceGrabMode.GravityGloves)] public float AutoGrabDistance = .2f;

        public HVRPlayerInputs Inputs => HandGrabber.Inputs;

        private bool _grabbableCollided;

        public override Vector3 JointAnchorWorldPosition => HandGrabber.JointAnchorWorldPosition;


        private bool _canFlick;
        private bool _canQuickStart;
        private Coroutine _additionalGrabRoutine;
        private HVRGrabbableHoverBase _grabIndicator;
        private GameObject _forceAnchor;
        private Rigidbody _forceRB;

        public float VelocityMagnitude => HandGrabber.HVRTrackedController.VelocityMagnitude;
        public float AngularVelocityMagnitude => HandGrabber.HVRTrackedController.AngularVelocityMagnitude;

        public HVRHandSide HandSide => HandGrabber.HandSide;

        public bool IsForceGrabbing { get; private set; }

        public bool IsAiming { get; private set; }

        protected override void Start()
        {
            base.Start();

            if (!HandGrabber)
            {
                HandGrabber = GetComponentInChildren<HVRHandGrabber>();
            }

            if (!HandGrabber)
            {
                Debug.LogWarning("Cannot find HandGrabber. Make sure to assign or have it on this level or below.");
            }

            CheckForceAnchor();

            if (!ForcePullSettings)
            {
                ForcePullSettings = ScriptableObject.CreateInstance<HVRForcePullSettings>();
            }
        }

        private void CheckForceAnchor()
        {
            if (!_forceAnchor)
            {
                _forceAnchor = new GameObject("ForceAnchor");
                _forceRB = _forceAnchor.AddComponent<Rigidbody>();
                _forceRB.isKinematic = true;
            }
        }


        protected override void Update()
        {
            base.Update();

            if (RequiresFlick && GrabStyle == HVRForceGrabMode.GravityGloves)
            {
                CheckFlick();
                CheckDrawRay();
            }

            CheckGripButtonGrab();
            UpdateGrabIndicator();
        }

        private void CheckFlick()
        {
            if (IsGrabbing || !IsHovering || !Inputs.GetForceGrabActive(HandSide))
            {
                return;
            }

            if (_canFlick && AngularVelocityMagnitude > FlickStartThreshold)
            {
                TryGrab(HoverTarget);
                _canFlick = false;
            }

            if (AngularVelocityMagnitude < FlickEndThreshold)
            {
                _canFlick = true;
            }

            if (VelocityMagnitude < QuickMoveResetThreshold)
            {
                _canQuickStart = true;
            }

            if (_canQuickStart && VelocityMagnitude > QuickMoveThreshold)
            {
                TryGrab(HoverTarget);
                _canQuickStart = false;
            }
        }

        private void CheckGripButtonGrab()
        {
            if ((!RequiresFlick || GrabStyle == HVRForceGrabMode.ForcePull) && !IsGrabbing && IsHovering && Inputs.GetForceGrabActivated(HandSide))
            {
                TryGrab(HoverTarget);
            }
        }


        private void CheckDrawRay()
        {
            if (!IsGrabbing && HoverTarget && Inputs.GetForceGrabActive(HandSide))
            {
                Laser.Enable(HoverTarget.transform);
            }
            else
            {
                Laser.Disable();
            }
        }


        protected override void CheckUnHover()
        {
            if (RequiresFlick && GrabStyle == HVRForceGrabMode.GravityGloves && !HandGrabber.IsGrabbing && Inputs.GetForceGrabActive(HandSide) && HoverTarget && !HoverTarget.IsBeingForcedGrabbed && !HoverTarget.IsBeingHeld)
            {
                IsAiming = true;
                return;
            }
            IsAiming = false;
            base.CheckUnHover();
        }

        public override bool CanGrab(HVRGrabbable grabbable)
        {
            if (grabbable.IsSocketed)
                return false;
            if (!grabbable.ForceGrabbable || grabbable.IsBeingForcedGrabbed || grabbable.IsBeingHeld)
                return false;
            if (HandGrabber.IsGrabbing || HandGrabber.IsHovering || HandGrabber.IsHoveringSocket)
                return false;
            if (!grabbable.Rigidbody)
                return false;
            return base.CanGrab(grabbable);
        }

        public override bool CanHover(HVRGrabbable grabbable)
        {
            if (!CanGrab(grabbable))
                return false;
            return base.CanHover(grabbable);
        }

        protected virtual void OnGrabbedHaptics()
        {
            if (IsMine)
            {
                HandGrabber.Controller.Vibrate(HVRInputManager.Instance.GrabHaptics.ForceGrab);
            }
        }

        protected override void OnGrabbed(HVRGrabArgs args)
        {
            //Debug.Log($"force grabbed!");
            base.OnGrabbed(args);

            if (_additionalGrabRoutine != null)
            {
                StopCoroutine(_additionalGrabRoutine);
            }

            if (HandGrabber.HandAnimator)
            {
                if (GrabPoser)
                {
                    HandGrabber.SetAnimatorPose(GrabPoser);
                }
                else
                {
                    ResetAnimator();
                }
            }

            IsForceGrabbing = true;
            if (GrabStyle == HVRForceGrabMode.GravityGloves)
            {
                StartCoroutine(GravityGloves(args.Grabbable));
            }
            else
            {
                CheckForceAnchor();
                StartCoroutine(ForcePull(args.Grabbable));
            }

            Grabbed.Invoke(this, args.Grabbable);
            args.Grabbable.Collided.AddListener(OnGrabbableCollided);
            args.Grabbable.Grabbed.AddListener(OnGrabbableGrabbed);

            if (SFXGrab)
                if (SFXPlayer.Instance) SFXPlayer.Instance.PlaySFX(SFXGrab, transform.position);

            OnGrabbedHaptics();
        }

        protected override void OnHoverEnter(HVRGrabbable grabbable)
        {
            base.OnHoverEnter(grabbable);

            OnHoverHaptics();

            if (grabbable.ShowForceGrabIndicator)
            {
                if (grabbable.ForceGrabIndicator)
                {
                    _grabIndicator = grabbable.ForceGrabIndicator;
                }
                else
                {
                    _grabIndicator = GrabIndicator;
                }

                if (_grabIndicator)
                {
                    _grabIndicator.Enable();
                    _grabIndicator.Hover();
                }
            }

            if (HoverPoser)
            {
                HandGrabber.SetAnimatorPose(HoverPoser);
            }
        }

        protected virtual void OnHoverHaptics()
        {
            if (IsMine)
            {
                HandGrabber.Controller.Vibrate(HVRInputManager.Instance.GrabHaptics.ForceHover);
            }
        }

        protected override void OnHoverExit(HVRGrabbable grabbable)
        {
            base.OnHoverExit(grabbable);

            if (_grabIndicator)
            {
                _grabIndicator.Unhover();
                _grabIndicator.Disable();
            }

            if (!IsGrabbing)
            {
                ResetAnimator();
            }
        }

        private void ResetAnimator()
        {
            if (HandGrabber.HandAnimator)
            {
                if (GrabPoser && HandGrabber.HandAnimator.CurrentPoser == GrabPoser || HoverPoser && HandGrabber.HandAnimator.CurrentPoser == HoverPoser)
                {
                    HandGrabber.HandAnimator.ResetToDefault();
                    HandGrabber.ResetCloneAnimator();
                }
            }
        }

        public IEnumerator ForcePull(HVRGrabbable grabbable)
        {
            var rb = grabbable.Rigidbody;
            var drag = rb.drag;
            var angularDrag = rb.angularDrag;
            var useGrav = rb.useGravity;
            HandGrabber.DisableHandCollision(grabbable);

            rb.useGravity = false;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            grabbable.IsBeingForcedGrabbed = true;
            IsHoldActive = true;

            var grabPoint = grabbable.GetGrabPointTransform(HandGrabber, GrabpointFilter.ForceGrab);
            if (!grabPoint)
                grabPoint = grabbable.transform;

            var posableGrabPoint = grabPoint.GetComponent<HVRPosableGrabPoint>();

            var isPhysicsGrab = grabbable.GrabType == HVRGrabType.PhysicPoser;
            if (!isPhysicsGrab && grabbable.GrabType != HVRGrabType.Offset)
            {
                isPhysicsGrab = !posableGrabPoint && grabbable.PhysicsPoserFallback;
            }


            var direction = HandGrabber.JointAnchorWorldPosition - grabPoint.position;
            var startDistance = direction.magnitude;
            var distance = startDistance;

            var settings = grabbable.ForcePullOverride;
            if (!settings)
                settings = ForcePullSettings;

            var Spring = settings.Spring;
            var Damper = settings.Damper;
            var MaxForce = settings.MaxForce;

            var SlerpDamper = settings.SlerpDamper;
            var SlerpMaxForce = settings.SlerpMaxForce;
            var SlerpSpring = settings.SlerpSpring;

            var DynamicGrabThreshold = settings.DynamicGrabThreshold;
            var DistanceThreshold = settings.DistanceThreshold;
            var Speed = settings.Speed;
            var DistanceToRotate = settings.RotateTriggerDistance;
            var RotateOverDistance = settings.RotateOverDistance;

            var MaxMissSpeed = settings.MaxMissSpeed;
            var MaxMissAngularSpeed = settings.MaxMissAngularSpeed;

            _forceAnchor.transform.position = grabPoint.transform.position;
            _forceAnchor.transform.rotation = grabbable.transform.rotation;

            if (posableGrabPoint)
            {
                _forceAnchor.transform.rotation = posableGrabPoint.GetPoseWorldRotation(HandSide);
            }

            var joint = _forceAnchor.AddComponent<ConfigurableJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.SetLinearDrive(Spring, Damper, MaxForce);
            joint.SetSlerpDrive(SlerpSpring, SlerpDamper, SlerpMaxForce);
            joint.connectedBody = rb;
            joint.connectedAnchor = rb.transform.InverseTransformPoint(grabPoint.position);

            var limit = isPhysicsGrab ? DynamicGrabThreshold : DistanceThreshold;


            var rotating = false;
            var rotateSpeed = 0f;
            var elapsed = 0f;
            var needsRotating = posableGrabPoint;

            while (GrabbedTarget && Inputs.GetForceGrabActive(HandSide) && distance > limit)
            {
                direction = HandGrabber.JointAnchorWorldPosition - grabPoint.position;
                distance = direction.magnitude;

                if (HandGrabber.IsValidGrabbable(GrabbedTarget) && HandGrabber.TryAutoGrab(GrabbedTarget, posableGrabPoint))
                {
                    break;
                }

                if ((isPhysicsGrab || grabbable.GrabType == HVRGrabType.Offset) && distance < DynamicGrabThreshold && HandGrabber.TryAutoGrab(grabbable, posableGrabPoint))
                {
                    rb.angularVelocity = Vector3.zero;
                    rb.velocity = Vector3.zero;
                    break;
                }


                _forceAnchor.transform.position = Vector3.MoveTowards(_forceAnchor.transform.position, JointAnchorWorldPosition, Speed * Time.fixedDeltaTime);
                var dir = _forceAnchor.transform.position - grabPoint.position;
                if (dir.magnitude > .3f)
                {
                    _forceAnchor.transform.position = grabPoint.position + dir.normalized * .3f;
                }

                if (needsRotating && !rotating)
                {
                    if (settings.RotationTrigger == ForcePullRotationTrigger.DistanceToHand)
                    {
                        rotating = distance < DistanceToRotate && posableGrabPoint;
                    }
                    else if (settings.RotationTrigger == ForcePullRotationTrigger.PercentTraveled)
                    {
                        rotating = (startDistance - distance) / startDistance > settings.RotateTriggerPercent / 100f;
                    }
                    else if (settings.RotationTrigger == ForcePullRotationTrigger.TimeSinceStart)
                    {
                        rotating = elapsed > settings.RotateTriggerTime;
                    }

                    if (rotating)
                    {
                        if (settings.RotationStyle == ForceRotationStyle.RotateOverDistance)
                        {
                            var rotatateDistance = Mathf.Min(RotateOverDistance, distance);
                            var time = rotatateDistance / Speed;
                            rotateSpeed = Quaternion.Angle(joint.transform.rotation, HandGrabber.CachedWorldRotation) / time;
                        }
                        else if (settings.RotationStyle == ForceRotationStyle.RotateOverRemaining)
                        {
                            var time = distance / Speed;
                            rotateSpeed = Quaternion.Angle(joint.transform.rotation, HandGrabber.CachedWorldRotation) / time;
                        }
                    }
                }

                if (rotating)
                {
                    joint.transform.rotation = Quaternion.RotateTowards(joint.transform.rotation, HandGrabber.CachedWorldRotation, rotateSpeed * Time.fixedDeltaTime);
                }

                yield return new WaitForFixedUpdate();

                elapsed += Time.fixedDeltaTime;
            }

            ResetAnimator();


            joint.connectedBody = null;
            Destroy(joint);

            IsForceGrabbing = false;
            IsHoldActive = false;

            if (grabbable)
            {

                rb.useGravity = useGrav;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxMissSpeed);
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, MaxMissAngularSpeed);

                if (IsGrabbing)
                {
                    direction = HandGrabber.JointAnchorWorldPosition - grabPoint.position;
                    if (direction.magnitude < limit)
                    {
                        if (HandGrabber.TryAutoGrab(grabbable, posableGrabPoint))
                        {
                            rb.angularVelocity = Vector3.zero;
                            rb.velocity = Vector3.zero;
                        }
                        else
                        {
                            HandGrabber.EnableHandCollision(grabbable);
                            ForceRelease();
                        }
                    }

                    grabbable.IsBeingForcedGrabbed = false;
                }
            }
        }

        public IEnumerator GravityGloves(HVRGrabbable grabbable)
        {
            var needsCollisionEnabled = true;
            var grabPoint = grabbable.GetGrabPointTransform(HandGrabber, GrabpointFilter.ForceGrab);
            if (!grabPoint)
                grabPoint = grabbable.transform;

            var posableGrabPoint = grabPoint.GetComponent<HVRPosableGrabPoint>();

            var rb = grabbable.Rigidbody;
            var drag = rb.drag;
            var angularDrag = rb.angularDrag;
            var useGrav = rb.useGravity;

            try
            {
                HandGrabber.DisableHandCollision(grabbable);

                _grabbableCollided = false;
                IsHoldActive = true;

                grabbable.IsBeingForcedGrabbed = true;
                grabbable.Rigidbody.useGravity = false;
                grabbable.Rigidbody.drag = 0f;
                grabbable.Rigidbody.angularDrag = 0f;

                fts.solve_ballistic_arc_lateral(false,
                    grabPoint.position,
                    ForceTime,
                    JointAnchorWorldPosition,
                    JointAnchorWorldPosition.y + YOffset,
                    out var velocity,
                    out var gravity);

                grabbable.Rigidbody.velocity = velocity;

                var elapsed = 0f;

                while (GrabbedTarget)
                {
                    if (elapsed > ForceTime)
                    {
                        break;
                    }

                    var currentVector = JointAnchorWorldPosition - grabPoint.position;

                    currentVector.y = 0;

                    var percentTime = elapsed / ForceTime;
                    var yExtra = YOffset * (1 - percentTime);

                    if (percentTime < .3) _grabbableCollided = false;
                    else if (_grabbableCollided)
                    {
                        if (grabbable.Rigidbody.velocity.magnitude > MaximumVelocityPostCollision)
                            grabbable.Rigidbody.velocity = grabbable.Rigidbody.velocity.normalized * MaximumVelocityPostCollision;
                        ForceRelease();
                        //Debug.Log($"Collided while force grabbing.");
                        break;
                    }


                    fts.solve_ballistic_arc_lateral(
                        false,
                        grabPoint.position,
                        ForceTime - elapsed,
                        JointAnchorWorldPosition,
                        JointAnchorWorldPosition.y + yExtra,
                        out velocity, out gravity);

                    grabbable.Rigidbody.velocity = velocity;
                    grabbable.Rigidbody.AddForce(-Vector3.up * gravity, ForceMode.Acceleration);

                    if (AutoGrab && HandGrabber.IsValidGrabbable(GrabbedTarget) && HandGrabber.TryAutoGrab(GrabbedTarget, posableGrabPoint))
                    {
                        needsCollisionEnabled = false;
                        IsForceGrabbing = false;
                        break;
                    }

                    if (AutoGrab && (JointAnchorWorldPosition - grabPoint.position).magnitude < AutoGrabDistance)
                    {
                        if (HandGrabber.TryAutoGrab(GrabbedTarget, posableGrabPoint))
                        {
                            needsCollisionEnabled = false;
                            IsForceGrabbing = false;
                            break;
                        }
                    }

                    if (currentVector.magnitude < .1f)
                    {
                        //Debug.Log($"<.1f");
                        break;
                    }

                    if (posableGrabPoint)
                    {
                        var delta = HandGrabber.CachedWorldRotation * Quaternion.Inverse(posableGrabPoint.GetPoseWorldRotation(HandGrabber.HandSide));

                        delta.ToAngleAxis(out var angle, out var axis);

                        if (angle > 180.0f) angle -= 360.0f;

                        var remaining = ForceTime - elapsed;

                        if (percentTime > .3f && Mathf.Abs(angle) > 1 && remaining > .01)
                        {
                            grabbable.Rigidbody.angularVelocity = axis * (angle * Mathf.Deg2Rad) / ForceTime;
                        }
                        else
                        {
                            grabbable.Rigidbody.angularVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        grabbable.Rigidbody.angularVelocity = Vector3.zero;
                    }

                    elapsed += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }

                ResetAnimator();
            }
            finally
            {
                if (needsCollisionEnabled)
                {
                    HandGrabber.EnableHandCollision(grabbable);
                }

                IsHoldActive = false;
                if (grabbable)
                {
                    rb.useGravity = useGrav;
                    rb.drag = drag;
                    rb.angularDrag = angularDrag;
                    grabbable.IsBeingForcedGrabbed = false;
                    grabbable.Collided.RemoveListener(OnGrabbableCollided);
                    grabbable.Grabbed.RemoveListener(OnGrabbableGrabbed);
                }

                if (IsGrabbing)
                {
                    ForceRelease();
                }

                IsForceGrabbing = false;
            }

            if (AutoGrab && AdditionalAutoGrabTime > 0 && !grabbable.IsBeingHeld)
            {
                _additionalGrabRoutine = StartCoroutine(ContinueAutoGrab(grabbable, posableGrabPoint));
            }
        }

        private IEnumerator ContinueAutoGrab(HVRGrabbable grabbable, HVRPosableGrabPoint grabPoint)
        {
            var t = grabPoint ? grabPoint.transform : grabbable.transform;

            var elapsed = 0f;
            while (grabbable && elapsed < AdditionalAutoGrabTime && !grabbable.IsBeingHeld)
            {
                if (grabbable.Rigidbody.velocity.magnitude > MaximumVelocityAutoGrab)
                    grabbable.Rigidbody.velocity *= .9f;


                if ((JointAnchorWorldPosition - t.position).magnitude < AutoGrabDistance)
                {
                    if (HandGrabber.TryAutoGrab(grabbable, grabPoint))
                    {
                        break;
                    }
                }

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _additionalGrabRoutine = null;
        }

        private void OnGrabbableGrabbed(HVRGrabberBase arg0, HVRGrabbable grabbable)
        {
            //Debug.Log($"Grabbed while force grabbing.");
        }

        private void OnGrabbableCollided(HVRGrabbable g)
        {
            _grabbableCollided = true;
        }

        private void UpdateGrabIndicator()
        {
            if (!IsHovering || !_grabIndicator)
                return;

            if (_grabIndicator.LookAtCamera && HVRManager.Instance.Camera)
            {
                _grabIndicator.transform.LookAt(HVRManager.Instance.Camera);
            }

            if (_grabIndicator.HoverPosition == HVRHoverPosition.Self)
                return;

            var grabPoint = HoverTarget.GetGrabPointTransform(HandGrabber, GrabpointFilter.ForceGrab);

            var position = HoverTarget.transform.position;
            if (grabPoint && _grabIndicator.HoverPosition == HVRHoverPosition.GrabPoint)
            {
                position = HandGrabber.GetGrabIndicatorPosition(HoverTarget, grabPoint, true);
            }

            _grabIndicator.transform.position = position;
        }
    }

    public enum HVRForceGrabMode
    {
        GravityGloves,
        ForcePull
    }
}