using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.HurricaneVR.Framework.Shared.Utilities;
using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Core.Bags;
using HurricaneVR.Framework.Core.HandPoser;
using HurricaneVR.Framework.Core.HandPoser.Data;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core.ScriptableObjects;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HurricaneVR.Framework.Core.Grabbers
{
    public class HVRHandGrabber : HVRGrabberBase
    {
        internal const int TrackedVelocityCount = 10;


        [Tooltip("HVRSocketBag used for placing and removing from sockets")]
        public HVRSocketBag SocketBag;

        [Header("HandSettings")]

        [Tooltip("Set to true if the HandModel is an IK target")]
        public bool InverseKinematics;

        [Tooltip("If true the default hand layer will be applied to this object on start")]
        public bool ApplyHandLayer = true;

        [Header("Grab Settings")]

        [Tooltip("If true the hand will move to the grabbable instead of pulling the grabbable to the hand")]
        public bool HandGrabs;

        [Tooltip("Hand move speed when HandGrabs = true")]
        public float HandGrabSpeed = 5f;

        [Tooltip("When dynamic grabbing the palm faces closest point on the collider surface before closing the fingers.")]
        public bool DynamicGrabPalmAdjust;

        [Tooltip("If in a networked game, can someone take this an object from your hand?")]
        public bool AllowMultiplayerSwap;


        [Tooltip("Hold down or Toggle grabbing")]
        public HVRGrabTrigger GrabTrigger = HVRGrabTrigger.Active;

        [Tooltip("Left or right hand.")]
        public HVRHandSide HandSide;

        public HVRHandPoser GrabPoser;
        public HVRHandPoser HoverPoser;

        [Tooltip("If true the hand model will be cloned for collision use, and colliders removed off the original hand. This will prevent" +
                 "unwanted center of mass and inertia tensor recalculations on grabbable objects due to hand model parenting.")]
        public bool CloneHandModel = true;

        [Tooltip("Ignores hand model parenting distance check.")]
        public bool IgnoreParentingDistance;

        [Tooltip("Ignores hand model parenting angle check.")]
        public bool IgnoreParentingAngle;

        [Tooltip("Angle to meet before hand model parents to the grabbable.")]
        public float ParentingMaxAngleDelta = 20f;

        [Tooltip("Distance to meet before hand model parents to the grabbable")]
        public float ParentingMaxDistance = .01f;

        [Tooltip("Settings used to pull and rotate the object into position")]
        public HVRJointSettings PullingSettings;

        [Tooltip("Layer mask to determine line of sight to the grabbable.")]
        public LayerMask RaycastLayermask;

        [Header("Components")]

        [Tooltip("The hand animator component, loads from children on startup if not supplied.")]
        public HVRHandAnimator HandAnimator;

        [Tooltip("Component that holds collider information about the hands. Auto populated from children if not set.")]
        public HVRHandPhysics HandPhysics;
        public HVRPlayerInputs Inputs;
        public HVRPhysicsPoser PhysicsPoser;
        public HVRForceGrabber ForceGrabber;

        public HVRControllerOffset ControllerOffset;
        public HVRTeleportCollisonHandler CollisionHandler;

        [Header("Grab Indicators")]
        public HVRGrabbableHoverBase GrabIndicator;
        public HVRGrabbableHoverBase TriggerGrabIndicator;
        public HVRGrabbableHoverBase DynamicPoseIndicator;
        public DynamicPoseGrabIndicator DynamicPoseIndicatorMode = DynamicPoseGrabIndicator.Palm;

        [Tooltip("Default hand pose to fall back to.")]
        public HVRHandPoser FallbackPoser;

        [Header("Required Transforms")]

        [Tooltip("Object holding the hand model.")]
        public Transform HandModel;

        [Tooltip("Configurable joints are anchored here")]
        public Transform JointAnchor;
        [Tooltip("Used to shoot ray casts at the grabbable to check if there is line of sight before grabbing.")]
        public Transform RaycastOrigin;
        [Tooltip("The transform that is handling device tracking.")]
        public Transform TrackedController;

        [Tooltip("Sphere collider that checks when collisions should be re-enabled between a released grabbable and this hand.")]
        public Transform OverlapSizer;

        [Header("Throw Settings")]

        [Tooltip("Factor to apply to the linear velocity of the throw.")]
        public float ReleasedVelocityFactor;

        [Tooltip("Factor to apply to the angular to linear calculation.")]
        public float ReleasedAngularConversionFactor = 1.0f;

        [Tooltip("Hand angular velocity must exceed this to add linear velocity based on angular velocity.")]
        public float ReleasedAngularThreshold = 1f;

        [Tooltip("Number of frames to average velocity for throwing.")]
        public int ThrowLookback = 5;

        [Tooltip("Number of frames to skip while averaging velocity.")]
        public int ThrowLookbackStart;


        [Tooltip("If true throwing takes only the top peak velocities for throwing.")]
        public bool TakePeakVelocities;
        [DrawIf("TakePeakVelocities", true)]
        public int CountPeakVelocities = 3;

        [Tooltip("Uses the center of mass that should match with current controller type you are using.")]
        public HVRThrowingCenterOfMass ThrowingCenterOfMass;

        [Tooltip("Invoked when the hand and object are too far apart")]
        public VRHandGrabberEvent BreakDistanceReached = new VRHandGrabberEvent();

        [Header("Debugging")]

        [Tooltip("If enabled displays vectors involved in throwing calculation.")]
        public bool DrawCenterOfMass;
        public bool GrabToggleActive;
        [SerializeField]
        private HVRGrabbable _triggerHoverTarget;
        public HVRSocket HoveredSocket;
        [SerializeField]
        private HVRGrabbable _hoverTarget;


        private bool _dynamicIndicatorEnabled;
        private bool _grabIndicatorEnabled;
        private HVRGrabbableHoverBase _grabIndicator;
        private HVRGrabbableHoverBase _triggerIndicator;

        public override bool IsHandGrabber => true;

        public HVRHandStrengthHandler StrengthHandler { get; set; }

        public Transform HandModelParent { get; private set; }
        public Vector3 HandModelPosition { get; private set; }
        public Quaternion HandModelRotation { get; private set; }
        public Vector3 HandModelScale { get; private set; }

        public HVRRigidBodyOverrides RigidOverrides { get; private set; }

        public Dictionary<HVRGrabbable, Coroutine> OverlappingGrabbables = new Dictionary<HVRGrabbable, Coroutine>();

        public GameObject TempGrabPoint { get; internal set; }

        public HVRController Controller => HandSide == HVRHandSide.Left ? HVRInputManager.Instance.LeftController : HVRInputManager.Instance.RightController;

        public bool IsLineGrab { get; private set; }

        public bool IsInitialLineGrab => IsLineGrab && !_primaryGrabPointGrab && PosableGrabPoint.LineInitialCanReposition;


        public HVRGrabbable TriggerHoverTarget
        {
            get { return _triggerHoverTarget; }
            set
            {
                _triggerHoverTarget = value;
                IsTriggerHovering = value;
            }
        }

        public bool IsTriggerHovering { get; private set; }

        public HVRTrackedController HVRTrackedController { get; private set; }

        public override Transform GrabPoint
        {
            get => base.GrabPoint;
            set
            {
                if (!value)
                {
                    PosableGrabPoint = null;
                }
                else if (GrabPoint != value)
                {
                    PosableGrabPoint = value.GetComponent<HVRPosableGrabPoint>();
                }

                base.GrabPoint = value;
            }
        }


        public HVRPosableGrabPoint PosableGrabPoint { get; private set; }

        private Transform _triggerGrabPoint;
        public Transform TriggerGrabPoint
        {
            get => _triggerGrabPoint;
            set
            {
                if (!value)
                {
                    TriggerPosableGrabPoint = null;
                }
                else if (GrabPoint != value)
                {
                    TriggerPosableGrabPoint = value.GetComponent<HVRPosableGrabPoint>();
                }

                _triggerGrabPoint = value;
            }
        }


        public HVRPosableGrabPoint TriggerPosableGrabPoint { get; private set; }

        /// <summary>
        /// When a grab is initiated, this should be set to the hand models rotation relative to the grabbable object transform
        /// </summary>

        public Quaternion PoseLocalRotation { get; set; }

        /// <summary>
        /// World Pose Rotation of the currently active grab point 
        /// </summary>
        public Quaternion PoseWorldRotation
        {
            get
            {
                return GrabbedTarget.transform.rotation * PoseLocalRotation;
            }
        }

        public Vector3 PoseWorldPosition
        {
            get
            {
                if (PosableGrabPoint)
                {
                    return PosableGrabPoint.transform.TransformPoint(PosableGrabPoint.GetPosePositionOffset(HandSide));
                }

                if (IsPhysicsPose)
                {
                    return GrabbedTarget.transform.TransformPoint(PhysicsHandPosition);
                }

                return GrabPoint.position;
            }
        }


        internal Quaternion PhysicsHandRotation
        {
            get { return PoseLocalRotation; }
            set { PoseLocalRotation = value; }
        }

        internal Vector3 PhysicsHandPosition { get; set; }
        internal byte[] PhysicsPoseBytes { get; private set; }

        public override Quaternion ControllerRotation => TrackedController.rotation;

        public Transform Palm => PhysicsPoser.Palm;

        public bool IsClimbing { get; private set; }

        public bool IsPhysicsPose { get; set; }

        public Vector3 LineGrabAnchor => GrabbedTarget.transform.InverseTransformPoint(PosableGrabPoint.WorldLineMiddle);

        public Vector3 GrabAnchorLocal { get; private set; }

        public Vector3 GrabAnchorWorld => GrabbedTarget.transform.TransformPoint(GrabAnchorLocal + _lineOffset);

        public override Vector3 JointAnchorWorldPosition => JointAnchor.position;

        public Vector3 HandAnchorWorld => transform.TransformPoint(HandAnchorLocal);

        public Vector3 HandAnchorLocal { get; private set; }

        public bool IsHoveringSocket => HoveredSocket;

        public int PoserIndex => _posableHand ? _posableHand.PoserIndex : 0;

        public Quaternion CachedWorldRotation => transform.rotation * HandModelRotation;
        public Quaternion HandWorldRotation => HandModel.rotation;

        public readonly CircularBuffer<Vector3> RecentVelocities = new CircularBuffer<Vector3>(TrackedVelocityCount);
        public readonly CircularBuffer<Vector3> RecentAngularVelocities = new CircularBuffer<Vector3>(TrackedVelocityCount);

        public bool CanActivate { get; private set; }

        public bool CanRelease { get; set; } = true;

        protected Vector3 LineGrabHandVector => transform.rotation * HandModelRotation * _lineGrabHandRelativeDirection;

        protected Vector3 LineGrabVector => PosableGrabPoint.WorldLine.normalized * (_flippedLinePose ? -1f : 1f);

        #region Private

        private SphereCollider _overlapCollider;
        private readonly Collider[] _overlapColliders = new Collider[1000];
        private bool _hasPosed;
        private bool _handMoving;
        private Quaternion _previousRotation = Quaternion.identity;
        private float _pullingTimer;
        private Transform _collisionTransform;
        private HVRHandAnimator _collisionAnimator;
        public ConfigurableJoint Joint { get; protected set; }
        private Transform _fakeHand;
        private Transform _fakeHandAnchor;
        private bool _isForceAutoGrab;
        private Vector3 _lineOffset;
        private bool _tightlyHeld;
        private bool _flippedLinePose;
        private Quaternion _startRotation;
        private bool _primaryGrabPointGrab;
        private bool _socketGrab;
        private HVRPosableHand _posableHand;
        private HVRPosableHand _collisionHand;
        private bool _hasForceGrabber;
        private HVRHandPoseData _physicsPose;
        private HVRHandPoseData _savedPose;
        private Vector3 _lineGrabHandRelativeDirection;
        private WaitForFixedUpdate _wffu;
        private bool _pulling;
        protected bool IsGripGrabActivated;
        protected bool IsTriggerGrabActivated;
        protected bool IsGripGrabActive;
        protected bool IsTriggerGrabActive;

        private bool _checkingSwap;

        #endregion

        protected virtual void Awake()
        {
            if (TrackedController)
                HVRTrackedController = TrackedController.GetComponent<HVRTrackedController>();

            RigidOverrides = GetComponent<HVRRigidBodyOverrides>();
            _wffu = new WaitForFixedUpdate();
        }

        protected override void Start()
        {
            base.Start();

            if (ApplyHandLayer)
            {
                transform.SetLayerRecursive(HVRLayers.Hand);
            }

            if (!CollisionHandler && transform.root)
            {
                CollisionHandler = transform.root.GetComponentInChildren<HVRTeleportCollisonHandler>();
            }

            //created in awake of the hand components if not exist, snapshot in start
            StrengthHandler = GetComponent<HVRHandStrengthHandler>();

            if (!Inputs)
            {
                Inputs = GetComponentInParent<HVRPlayerInputs>();
            }

            if (!ForceGrabber)
            {
                ForceGrabber = GetComponentInChildren<HVRForceGrabber>();
                _hasForceGrabber = ForceGrabber;
            }

            if (!HandAnimator)
            {
                if (HandModel)
                {
                    HandAnimator = HandModel.GetComponentInChildren<HVRHandAnimator>();
                }
                else
                {
                    HandAnimator = GetComponentInChildren<HVRHandAnimator>();
                }
            }

            if (!PhysicsPoser)
            {
                if (HandModel)
                {
                    PhysicsPoser = HandModel.GetComponentInChildren<HVRPhysicsPoser>();
                }
                else
                {
                    PhysicsPoser = GetComponentInChildren<HVRPhysicsPoser>();
                }
            }

            if (!HandPhysics)
            {
                HandPhysics = GetComponentInChildren<HVRHandPhysics>();
            }

            _posableHand = PhysicsPoser.Hand;

            if (HandModel)
            {
                HandModelParent = HandModel.parent;
                HandModelPosition = HandModel.localPosition;
                HandModelRotation = HandModel.localRotation;
                HandModelScale = HandModel.localScale;

                if (InverseKinematics && CloneHandModel)
                {
                    CloneHandModel = false;
                }

                //no need to clone hand model if the hand doesn't have colliders on it
                if (CloneHandModel)
                {
                    if (HandModel.GetComponentsInChildren<Collider>().Length == 0)
                        CloneHandModel = false;
                }

                if (CloneHandModel)
                {
                    //cloning the hand model and leaving only hand posing components and colliders
                    var handClone = Instantiate(HandModel.gameObject);
                    foreach (var component in handClone.GetComponentsInChildren<Component>())
                    {
                        if (component is Collider || component is HVRPosableHand || component is HVRHandAnimator || component is Transform ||
                            component is HVRHandPoser) continue;
                        Destroy(component);
                    }

                    //removing colliders from the original hand model
                    foreach (var col in HandModel.GetComponentsInChildren<Collider>())
                    {
                        Destroy(col);
                    }

                    _collisionTransform = handClone.transform;
                    ResetHandTransform(_collisionTransform);
                    _collisionAnimator = _collisionTransform.GetComponentInChildren<HVRHandAnimator>();
                    _collisionHand = _collisionTransform.GetComponent<HVRPosableHand>();
                }

                ResetRigidBodyProperties();

                var go = new GameObject("FakeHand");
                go.transform.parent = transform;
                go.transform.localPosition = HandModelPosition;
                go.transform.localRotation = HandModelRotation;
                _fakeHand = go.transform;

                go = new GameObject("FakeHandJointAnchor");
                go.transform.parent = _fakeHand;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                _fakeHandAnchor = go.transform;
            }

            this.ExecuteNextUpdate(() => HandPhysics.SetupColliders());

            if (OverlapSizer)
            {
                _overlapCollider = OverlapSizer.GetComponent<SphereCollider>();
            }

            if (!SocketBag)
                SocketBag = GetComponentInChildren<HVRSocketBag>();

            if (!ThrowingCenterOfMass)
                ThrowingCenterOfMass = GetComponentInChildren<HVRThrowingCenterOfMass>();

            ResetTrackedVelocities();

            if (!ControllerOffset)
            {
                if (TrackedController)
                {
                    ControllerOffset = TrackedController.GetComponentInChildren<HVRControllerOffset>();
                }
            }
        }

        protected override void Update()
        {
            if (PerformUpdate)
            {
                CheckCanActivate();
                CheckActivateGrabbable();

                CheckBreakDistance();
                TrackVelocities();

                UpdateGrabInputs();
                CheckGrabControlSwap();
                CheckUntoggleGrab();
                IsHoldActive = UpdateHolding();
                CheckSocketUnhover();
                CheckSocketHover();
                CheckUnHover();
                CheckTriggerUnHover();
                CheckRelease();
                CheckHover();
                CheckTriggerHover();
                UpdateGrabIndicator();
                UpdateTriggerGrabIndicator();
                CheckGrab();
            }

            UpdatePose();
            CheckPoseHand();

            _previousRotation = transform.rotation;
            _hoverTarget = HoverTarget;
        }

        protected override void FixedUpdate()
        {
            UpdatePullingGrabbable();
            UpdateLineGrab();
        }

        private void CheckGrabControlSwap()
        {
            if (!_checkingSwap)
                return;

            if (_grabbableControl == _currentGrabControl)
            {
                _checkingSwap = false;
                return;
            }

            //checking for socket to grabbable grab button changes
            if (_grabbableControl == HVRGrabControls.GripOnly && _currentGrabControl == HVRGrabControls.TriggerOnly)
            {
                if (IsGripGrabActive && !IsTriggerGrabActive)
                {
                    _currentGrabControl = HVRGrabControls.GripOnly;
                    _checkingSwap = false;
                }
            }
            else if (_grabbableControl == HVRGrabControls.GripOnly && _currentGrabControl == HVRGrabControls.GripOrTrigger)
            {
                if (IsGripGrabActive && !IsTriggerGrabActive)
                {
                    _currentGrabControl = HVRGrabControls.GripOnly;
                    _checkingSwap = false;
                }
            }
            else if (_grabbableControl == HVRGrabControls.TriggerOnly && _currentGrabControl == HVRGrabControls.GripOnly)
            {
                if (IsTriggerGrabActive && !IsGripGrabActive)
                {
                    _currentGrabControl = HVRGrabControls.TriggerOnly;
                    _checkingSwap = false;
                }
            }
            else if (_grabbableControl == HVRGrabControls.TriggerOnly && _currentGrabControl == HVRGrabControls.GripOrTrigger)
            {
                if (IsTriggerGrabActive && !IsGripGrabActive)
                {
                    _currentGrabControl = HVRGrabControls.TriggerOnly;
                    _checkingSwap = false;
                }
            }
            else if (_grabbableControl == HVRGrabControls.GripOrTrigger && _currentGrabControl == HVRGrabControls.TriggerOnly)
            {
                if (IsGripGrabActive && !IsTriggerGrabActive || (GrabToggleActive && !IsTriggerGrabActive && !IsGripGrabActive))
                {
                    _currentGrabControl = HVRGrabControls.GripOrTrigger;
                    _checkingSwap = false;
                }
            }
            else if (_grabbableControl == HVRGrabControls.GripOrTrigger && _currentGrabControl == HVRGrabControls.GripOnly)
            {
                if (IsTriggerGrabActive && !IsGripGrabActive || (GrabToggleActive && !IsTriggerGrabActive && !IsGripGrabActive))
                {
                    _currentGrabControl = HVRGrabControls.GripOrTrigger;
                    _checkingSwap = false;
                }
            }
        }

        protected virtual void CheckActivateGrabbable()
        {
            if (IsGrabbing && CanActivate)
            {
                if (Controller.TriggerButtonState.JustActivated)
                {
                    GrabbedTarget.InternalOnActivate(this);
                }
                else if (Controller.TriggerButtonState.JustDeactivated)
                {
                    GrabbedTarget.InternalOnDeactivate(this);
                }
            }
        }

        private void UpdatePose()
        {
            if (!IsLineGrab && IsGrabbing && GrabbedTarget.Stationary && !GrabbedTarget.ParentHandModel && _hasPosed)
            {
                HandModel.rotation = PoseWorldRotation;
                HandModel.position = PoseWorldPosition;
            }
        }

        protected void ResetTrackedVelocities()
        {
            for (var i = 0; i < TrackedVelocityCount; i++)
            {
                RecentVelocities.Enqueue(Vector3.zero);
                RecentAngularVelocities.Enqueue(Vector3.zero);
            }
        }

        private void DetermineGrabPoint(HVRGrabbable grabbable)
        {
            if (IsGrabbing)
                return;

            GrabPoint = GetGrabPoint(grabbable);
        }

        internal Transform GetGrabPoint(HVRGrabbable grabbable, GrabpointFilter grabType = GrabpointFilter.Normal)
        {
            return grabbable.GetGrabPointTransform(this, grabType);
        }

        private void CheckCanActivate()
        {
            if (!CanActivate && !IsTriggerGrabActive)
            {
                CanActivate = true;
            }
        }

        protected override void CheckUnHover()
        {
            if (!HoverTarget)
                return;

            var closestValid = ClosestValidHover(false);

            if (!CanHover(HoverTarget) || closestValid != HoverTarget)
            {
                UnhoverGrabbable(this, HoverTarget);
            }
        }

        protected override bool CheckHover()
        {
            if (IsHovering || !AllowHovering)
            {
                if (IsHovering && !HoverTarget)
                {
                    HoverTarget = null;
                }
                else
                {
                    return true;
                }
            }

            var closestValid = ClosestValidHover(false);
            if (closestValid == null)
                return false;

            HoverGrabbable(this, closestValid);
            return true;
        }

        protected virtual void CheckTriggerUnHover()
        {
            if (!TriggerHoverTarget)
                return;

            var closestValid = ClosestValidHover(true);

            if (!CanHover(TriggerHoverTarget) || closestValid != TriggerHoverTarget)
            {
                OnTriggerHoverExit(this, TriggerHoverTarget);
            }
        }


        protected virtual bool CheckTriggerHover()
        {
            if (IsTriggerHovering || !AllowHovering)
            {
                if (IsTriggerHovering && !TriggerHoverTarget)
                {
                    TriggerHoverTarget = null;
                }
                else
                {
                    return true;
                }
            }

            var closestValid = ClosestValidHover(true);
            if (closestValid == null)
                return false;

            OnTriggerHoverEnter(this, closestValid);
            return true;
        }



        private void CheckUntoggleGrab()
        {
            if (GrabToggleActive && !_checkingSwap)
            {
                if (_currentGrabControl == HVRGrabControls.GripOrTrigger)
                {
                    if (!IsLineGrab && (IsGripGrabActivated || (IsTriggerGrabActivated && Inputs.CanTriggerGrab)))
                    {
                        GrabToggleActive = false;
                    }
                    else if (IsLineGrab && IsGripGrabActivated && !IsTriggerGrabActive)
                    {
                        //if line grab and trigger is pressed - don't allow untoggle
                        GrabToggleActive = false;
                    }
                }
                else if (_currentGrabControl == HVRGrabControls.TriggerOnly && IsTriggerGrabActivated)
                {
                    GrabToggleActive = false;
                }
                else if (_currentGrabControl == HVRGrabControls.GripOnly && IsGripGrabActivated)
                {
                    GrabToggleActive = false;
                }

                if (!GrabToggleActive)
                {
                    IsGripGrabActivated = false;
                    IsTriggerGrabActivated = false;
                }
            }
        }

        private HVRGrabControls _currentGrabControl;
        private HVRGrabControls _grabbableControl;

        protected virtual bool UpdateHolding()
        {
            if (!IsGrabbing)
                return false;

            if (!CanRelease)
                return true;

            var grabTrigger = GrabTrigger;

            if (GrabbedTarget.OverrideGrabTrigger)
            {
                grabTrigger = GrabbedTarget.GrabTrigger;
            }

            switch (grabTrigger)
            {
                case HVRGrabTrigger.Active:
                    {
                        if (GrabToggleActive)
                        {
                            return true;
                        }

                        if (IsLineGrab)
                        {
                            return IsGripGrabActive || IsTriggerGrabActive;
                        }

                        var grabActive = false;
                        switch (_currentGrabControl)
                        {
                            case HVRGrabControls.GripOrTrigger:
                                grabActive = IsGripGrabActive || (IsTriggerGrabActive && Inputs.CanTriggerGrab);
                                break;
                            case HVRGrabControls.GripOnly:
                                grabActive = IsGripGrabActive;
                                break;
                            case HVRGrabControls.TriggerOnly:
                                grabActive = IsTriggerGrabActive;
                                break;
                        }

                        return grabActive;
                    }
                case HVRGrabTrigger.Toggle:
                    {
                        return GrabToggleActive;
                    }
                case HVRGrabTrigger.ManualRelease:
                    return true;
            }

            return false;
        }

        protected override void CheckGrab()
        {

            if (!AllowGrabbing || IsGrabbing || GrabbedTarget)
            {
                return;
            }

            if (HoveredSocket && CanGrabFromSocket(HoveredSocket) && GrabActivated(HoveredSocket.GrabControl))
            {
                _primaryGrabPointGrab = true;
                _socketGrab = true;
                GrabPoint = null;

                if (TryGrab(HoveredSocket.GrabbedTarget, true))
                {
                    _currentGrabControl = HoveredSocket.GrabControl;

                    HoveredSocket.OnHandGrabberExited();
                    HoveredSocket = null;
                    //Debug.Log($"grabbed from socket directly");
                }
            }

            if (HoverTarget)
            {
                var grabControl = HoverTarget.GrabControl;
                if (HoverTarget.IsSocketed)
                    grabControl = HoverTarget.Socket.GrabControl;

                if (GrabActivated(grabControl) && TryGrab(HoverTarget))
                {
                    _currentGrabControl = grabControl;
                    return;
                }
            }

            if (TriggerHoverTarget)
            {
                var grabControl = TriggerHoverTarget.GrabControl;
                if (TriggerHoverTarget.IsSocketed)
                    grabControl = TriggerHoverTarget.Socket.GrabControl;
                if (GrabActivated(grabControl) && TryGrab(TriggerHoverTarget))
                {
                    _currentGrabControl = grabControl;
                    return;
                }
            }
        }


        private void UpdateGrabInputs()
        {
            IsTriggerGrabActivated = Inputs.GetTriggerGrabState(HandSide).JustActivated;
            IsGripGrabActivated = Inputs.GetGrabActivated(HandSide);

            IsTriggerGrabActive = Inputs.GetTriggerGrabState(HandSide).Active;
            IsGripGrabActive = Inputs.GetGripHoldActive(HandSide);
        }

        private bool GrabActivated(HVRGrabControls grabControl)
        {
            switch (grabControl)
            {
                case HVRGrabControls.GripOrTrigger:
                    return IsGripGrabActivated || (IsTriggerGrabActivated && Inputs.CanTriggerGrab);
                case HVRGrabControls.GripOnly:
                    return IsGripGrabActivated;
                case HVRGrabControls.TriggerOnly:
                    return IsTriggerGrabActivated;
            }

            return false;
        }


        protected virtual void UpdateGrabIndicator()
        {
            if (!IsHovering || !_grabIndicator)
                return;

            if (_grabIndicator.LookAtCamera && HVRManager.Instance.Camera)
            {
                _grabIndicator.transform.LookAt(HVRManager.Instance.Camera);
            }

            if (_grabIndicator.HoverPosition == HVRHoverPosition.Self)
                return;

            if (_grabIndicator.HoverPosition == HVRHoverPosition.GrabPoint)
            {
                DetermineGrabPoint(HoverTarget);

                if (PosableGrabPoint)
                {
                    EnableGrabIndicator();
                    DisableDynamicIndicator();
                    _grabIndicator.transform.position = GetGrabIndicatorPosition(HoverTarget, PosableGrabPoint);
                    return;
                }

                var isDynamic = HoverTarget.GrabType == HVRGrabType.PhysicPoser || HoverTarget.PhysicsPoserFallback;

                if (isDynamic)
                {
                    if (DynamicPoseIndicatorMode == DynamicPoseGrabIndicator.None)
                    {
                        DisableGrabIndicator();
                        DisableDynamicIndicator();
                        return;
                    }

                    var indicator = _grabIndicator;

                    if (DynamicPoseIndicator)
                    {
                        if (!_dynamicIndicatorEnabled)
                        {
                            EnableDynamicIndicator();
                            DisableGrabIndicator();
                        }

                        indicator = DynamicPoseIndicator;

                        if (DynamicPoseIndicator.LookAtCamera && HVRManager.Instance.Camera)
                        {
                            DynamicPoseIndicator.transform.LookAt(HVRManager.Instance.Camera);
                        }
                    }

                    if (DynamicPoseIndicatorMode == DynamicPoseGrabIndicator.Palm)
                    {
                        indicator.transform.position = FindClosestPoint(HoverTarget, out var inside);
                        return;
                    }

                    if (DynamicPoseIndicatorMode == DynamicPoseGrabIndicator.Transform)
                    {
                        indicator.transform.position = HoverTarget.transform.position;
                    }

                    return;
                }
            }

            DisableDynamicIndicator();
            EnableGrabIndicator();
            _grabIndicator.transform.position = HoverTarget.transform.position;
        }

        protected virtual void UpdateTriggerGrabIndicator()
        {
            if (!IsTriggerHovering || !_triggerIndicator || IsGrabbing || TriggerHoverTarget == HoverTarget)
                return;

            if (_triggerIndicator.LookAtCamera && HVRManager.Instance.Camera)
            {
                _triggerIndicator.transform.LookAt(HVRManager.Instance.Camera);
            }

            if (_triggerIndicator.HoverPosition == HVRHoverPosition.Self)
                return;

            if (_triggerIndicator.HoverPosition == HVRHoverPosition.GrabPoint)
                TriggerGrabPoint = GetGrabPoint(TriggerHoverTarget, GrabpointFilter.Normal);

            if (TriggerPosableGrabPoint && _triggerIndicator.HoverPosition == HVRHoverPosition.GrabPoint)
            {
                _triggerIndicator.transform.position = GetGrabIndicatorPosition(TriggerHoverTarget, TriggerPosableGrabPoint);
            }
            else
            {
                _triggerIndicator.transform.position = TriggerHoverTarget.transform.position;
            }
        }

        internal Vector3 GetGrabIndicatorPosition(HVRGrabbable grabbable, Transform grabPoint, bool useGrabPoint = false)
        {
            var posableGrabPoint = grabPoint.GetComponent<HVRPosableGrabPoint>();
            if (posableGrabPoint)
            {
                return GetGrabIndicatorPosition(grabbable, posableGrabPoint, useGrabPoint);
            }

            return grabPoint.position;
        }

        internal Vector3 GetGrabIndicatorPosition(HVRGrabbable grabbable, HVRPosableGrabPoint grabPoint, bool useGrabPoint = false)
        {
            if (grabPoint.IsLineGrab && !useGrabPoint && grabPoint.LineInitialCanReposition)
            {
                return grabbable.transform.TransformPoint(GetLocalLineGrabPoint(grabbable, transform.TransformPoint(GetLineGrabHandAnchor(PosableGrabPoint))));
            }

            if (grabPoint.GrabIndicatorPosition)
                return grabPoint.GrabIndicatorPosition.position;

            return grabPoint.transform.position;
        }

        protected override void OnHoverEnter(HVRGrabbable grabbable)
        {
            base.OnHoverEnter(grabbable);

            GrabPoint = GetGrabPoint(grabbable, GrabpointFilter.Normal);

            OnHoverHaptics();

            if (grabbable.ShowGrabIndicator)
            {
                if (grabbable.GrabIndicator)
                {
                    _grabIndicator = grabbable.GrabIndicator;
                }
                else
                {
                    _grabIndicator = GrabIndicator;
                }

                EnableGrabIndicator();
            }

            if (HoverPoser)
            {
                SetAnimatorPose(HoverPoser);
            }
        }

        protected virtual void OnHoverHaptics()
        {
            if (IsMine)
            {
                Controller.Vibrate(HVRInputManager.Instance.GrabHaptics.HandHover);
            }
        }


        protected override void OnHoverExit(HVRGrabbable grabbable)
        {
            base.OnHoverExit(grabbable);

            DisableGrabIndicator();
            DisableDynamicIndicator();

            if (!IsGrabbing)
            {
                ResetAnimator();
            }
        }
        private void EnableGrabIndicator()
        {
            if (_grabIndicatorEnabled) return;
            if (_grabIndicator)
            {
                _grabIndicatorEnabled = true;
                _grabIndicator.Enable();
                _grabIndicator.Hover();
            }
        }

        private void DisableGrabIndicator()
        {
            if (!_grabIndicatorEnabled) return;
            if (_grabIndicator)
            {
                _grabIndicatorEnabled = false;
                _grabIndicator.Unhover();
                _grabIndicator.Disable();
            }
        }

        private void EnableDynamicIndicator()
        {
            if (_dynamicIndicatorEnabled) return;
            if (DynamicPoseIndicator)
            {
                _dynamicIndicatorEnabled = true;
                DynamicPoseIndicator.Enable();
                DynamicPoseIndicator.Hover();
            }
        }

        private void DisableDynamicIndicator()
        {
            if (!_dynamicIndicatorEnabled) return;
            if (DynamicPoseIndicator)
            {
                _dynamicIndicatorEnabled = false;
                DynamicPoseIndicator.Unhover();
                DynamicPoseIndicator.Disable();
            }
        }


        protected virtual void OnTriggerHoverEnter(HVRHandGrabber grabber, HVRGrabbable grabbable)
        {
            TriggerHoverTarget = grabbable;
            TriggerGrabPoint = GetGrabPoint(grabbable, GrabpointFilter.Normal);
            if (grabbable.ShowTriggerGrabIndicator)
            {
                if (grabbable.GrabIndicator)
                {
                    _triggerIndicator = grabbable.GrabIndicator;
                }
                else
                {
                    _triggerIndicator = TriggerGrabIndicator;
                }

                if (_triggerIndicator)
                {
                    _triggerIndicator.Enable();
                    _triggerIndicator.Hover();
                }
            }
        }

        protected virtual void OnTriggerHoverExit(HVRHandGrabber grabber, HVRGrabbable grabbable)
        {
            TriggerHoverTarget = null;

            if (_triggerIndicator)
            {
                _triggerIndicator.Unhover();
                _triggerIndicator.Disable();
            }
        }

        private void TrackVelocities()
        {
            var deltaRotation = transform.rotation * Quaternion.Inverse(_previousRotation);
            deltaRotation.ToAngleAxis(out var angle, out var axis);
            angle *= Mathf.Deg2Rad;
            var angularVelocity = axis * (angle * (1.0f / Time.fixedDeltaTime));

            RecentVelocities.Enqueue(Rigidbody.velocity);
            RecentAngularVelocities.Enqueue(angularVelocity);
        }

        protected virtual void CheckSocketUnhover()
        {
            if (!HoveredSocket)
                return;

            var closest = ClosestValidSocket();

            if (IsGrabbing || IsForceGrabbing || !CanGrabFromSocket(HoveredSocket) || closest != HoveredSocket)
            {
                HoveredSocket.OnHandGrabberExited();
                HoveredSocket = null;

                if (HVRSettings.Instance.VerboseHandGrabberEvents)
                    Debug.Log($"socket exited");
            }
        }

        protected virtual bool CanGrabFromSocket(HVRSocket socket)
        {
            if (!socket)
            {
                return false;
            }

            if (!socket.CanGrabbableBeRemoved(this))
            {
                return false;
            }

            return socket.GrabDetectionType == HVRGrabDetection.Socket && socket.GrabbedTarget;
        }

        protected virtual void CheckSocketHover()
        {
            if (IsGrabbing || IsHoveringSocket || !SocketBag || IsForceGrabbing)
                return;

            var closest = ClosestValidSocket();
            if (closest)
            {
                HoveredSocket = closest;
                HoveredSocket.OnHandGrabberEntered();
            }
        }

        protected virtual HVRSocket ClosestValidSocket()
        {
            for (var i = 0; i < SocketBag.ValidSockets.Count; i++)
            {
                var socket = SocketBag.ValidSockets[i];

                if (!CanGrabFromSocket(socket))
                    continue;

                return socket;
            }

            return null;
        }

        private void UpdatePullingGrabbable()
        {
            if (!IsGrabbing || !GrabPoint || !PullingGrabbable)
                return;

            _pullingTimer += Time.fixedDeltaTime;

            var angleDelta = Quaternion.Angle(PoseWorldRotation, CachedWorldRotation);

            var alreadyGrabbed = GrabbedTarget.GrabberCount > 1; //hand needs to move to the object always for a 2nd grab on the same object

            var angleComplete = angleDelta < GrabbedTarget.FinalJointMaxAngle;
            var distanceComplete = angleComplete && Vector3.Distance(HandAnchorWorld, GrabAnchorWorld) < .07f;
            var timesUp = _pullingTimer > GrabbedTarget.FinalJointTimeout && GrabbedTarget.FinalJointQuick;

            if (angleComplete && distanceComplete || timesUp || alreadyGrabbed)
            {
                var deltaRot = CachedWorldRotation * Quaternion.Inverse(PoseWorldRotation);
                if (alreadyGrabbed)
                {
                    transform.rotation = Quaternion.Inverse(deltaRot) * transform.rotation;
                }
                else
                {
                    GrabbedTarget.transform.rotation = deltaRot * GrabbedTarget.transform.rotation;
                }

                angleDelta = Quaternion.Angle(PoseWorldRotation, CachedWorldRotation);

                if (HVRSettings.Instance.VerboseHandGrabberEvents)
                    Debug.Log($"final joint created {angleDelta}");

                PullingGrabbable = false;

                SetupConfigurableJoint(GrabbedTarget, true);

            }
        }

        private void CheckBreakDistance()
        {
            if (_handMoving || PullingGrabbable) return;

            if (GrabbedTarget)
            {
                var position = GrabbedTarget.Stationary ? TrackedController.position : JointAnchorWorldPosition;
                if (Vector3.Distance(GrabAnchorWorld, position) > GrabbedTarget.BreakDistance)
                {
                    BreakDistanceReached.Invoke(this, GrabbedTarget);
                    ForceRelease();
                }
            }
        }

        private void CheckPoseHand()
        {
            if (!IsGrabbing || _hasPosed || !GrabbedTarget || _handMoving)
                return;

            var angleDelta = 0f;
            if (!IgnoreParentingAngle)
            {
                angleDelta = Quaternion.Angle(PoseWorldRotation, CachedWorldRotation);
            }

            var distance = 0f;
            if (!IgnoreParentingDistance && Joint)
            {
                distance = Vector3.Distance(HandAnchorWorld, GrabAnchorWorld);
            }

            if ((IgnoreParentingAngle || angleDelta <= ParentingMaxAngleDelta) &&
                (IgnoreParentingDistance || distance <= ParentingMaxDistance) ||
                GrabbedTarget.PoseImmediately ||
                GrabbedTarget.GrabberCount > 1)
            {
                PoseHand(GrabbedTarget.ParentHandModel);
            }
        }

        private void PoseHand(bool parent)
        {
            _hasPosed = true;

            if (IsPhysicsPose)
            {
                SetAnimatorPose(null);
                HandAnimator.StartDynamicPose(_physicsPose);
                if (CloneHandModel && _collisionAnimator) _collisionAnimator.StartDynamicPose(_physicsPose);

                if (parent)
                {
                    ParentHandModel(GrabPoint.transform);

                    //todo lerp so there isn't a snap
                    HandModel.transform.localPosition = PhysicsHandPosition;
                    HandModel.transform.localRotation = PhysicsHandRotation;
                }
                else
                {
                    ResetHandTransform(HandModel);
                }

                return;
            }

            var poser = PosableGrabPoint ? PosableGrabPoint.HandPoser : FallbackPoser;

            if (parent)
            {
                ParentHandModel(GrabPoint);
            }

            SetAnimatorPose(poser, parent);
        }

        private void ParentHandModel(Transform parent)
        {
            if (!parent)
                return;

            var worldRotation = parent.rotation;
            var worldPosition = parent.position;

            var posableGrabPoint = parent.GetComponent<HVRPosableGrabPoint>();
            if (posableGrabPoint && posableGrabPoint.VisualGrabPoint)
            {
                parent = posableGrabPoint.VisualGrabPoint;
                parent.rotation = worldRotation;
                parent.position = worldPosition;
            }

            HandModel.parent = parent;

            if (InverseKinematics && PosableGrabPoint)
            {
                //posable hand not on the IK target, need to set pos / rot manually
                var pose = PosableGrabPoint.HandPoser.PrimaryPose.Pose.GetPose(HandSide);
                HandModel.localRotation = pose.Rotation;
                HandModel.localPosition = pose.Position;
            }

            _hasPosed = true;

            var listener = parent.gameObject.AddComponent<HVRDestroyListener>();
            listener.Destroyed.AddListener(OnGrabPointDestroyed);
        }

        public void SetAnimatorPose(HVRHandPoser poser, bool poseHand = false, bool poseHandClone = false)
        {
            if (CloneHandModel && _collisionAnimator) _collisionAnimator.SetCurrentPoser(poser, poseHandClone);
            if (HandAnimator) HandAnimator.SetCurrentPoser(poser, poseHand);
        }

        public void ResetAnimator()
        {
            if (HandAnimator)
            {
                if (GrabPoser && HandAnimator.CurrentPoser == GrabPoser || HoverPoser && HandAnimator.CurrentPoser == HoverPoser)
                {
                    HandAnimator.ResetToDefault();
                }
            }

            ResetCloneAnimator();
        }

        public void ResetCloneAnimator()
        {
            if (CloneHandModel && _collisionAnimator) _collisionAnimator.ResetToDefault();
        }

        private void OnGrabPointDestroyed(HVRDestroyListener listener)
        {
            if (HandModel && HandModel.parent == listener.transform)
            {
                ResetHandModel();
            }
        }

        public void OverrideHandSettings(HVRJointSettings settings)
        {
            StrengthHandler.OverrideHandSettings(settings);
        }

        public override bool CanHover(HVRGrabbable grabbable)
        {
            if (IsForceGrabbing || (IsGripGrabActive && (!HoverTarget || HoverTarget != grabbable)))
                return false;

            return CanGrab(grabbable);
        }

        private bool IsForceGrabbing => _hasForceGrabber && (ForceGrabber.IsForceGrabbing || ForceGrabber.IsAiming);

        public override bool CanGrab(HVRGrabbable grabbable)
        {
            if (!base.CanGrab(grabbable))
                return false;

            if ((!AllowMultiplayerSwap && !grabbable.AllowMultiplayerSwap) && grabbable.HoldType != HVRHoldType.ManyHands && grabbable.AnyGrabberNotMine())
            {
                return false;
            }

            if (grabbable.PrimaryGrabber && !grabbable.PrimaryGrabber.AllowSwap)
            {
                if (grabbable.HoldType == HVRHoldType.TwoHanded && grabbable.GrabberCount > 1)
                    return false;

                if (grabbable.HoldType == HVRHoldType.OneHand && !_isForceAutoGrab && grabbable.GrabberCount > 0)
                    return false;
            }

            if (GrabbedTarget != null && GrabbedTarget != grabbable)
                return false;

            if (grabbable.IsSocketed && grabbable.Socket.GrabDetectionType == HVRGrabDetection.Socket)
                return false;

            if (grabbable.RequireLineOfSight && !grabbable.IsSocketed && !grabbable.IsBeingForcedGrabbed &&
                !grabbable.IsStabbed && !grabbable.IsStabbing && !CheckLineOfSight(grabbable))
                return false;

            if (grabbable.RequiresGrabbable)
            {
                if (!grabbable.RequiredGrabbable.PrimaryGrabber || !grabbable.RequiredGrabbable.PrimaryGrabber.IsHandGrabber)
                    return false;
            }

            return true;
        }

        protected virtual bool CheckLineOfSight(HVRGrabbable grabbable)
        {
            if (grabbable.HasConcaveColliders)
                return true;
            return CheckForLineOfSight(RaycastOrigin.position, grabbable, RaycastLayermask);
        }

        protected override void OnBeforeGrabbed(HVRGrabArgs args)
        {
            if (HVRSettings.Instance.VerboseHandGrabberEvents)
            {
                Debug.Log($"{name}:OnBeforeGrabbed");
            }

            if (args.Grabbable.GrabType == HVRGrabType.HandPoser)
            {
                if (args.Grabbable == TriggerHoverTarget)
                    GrabPoint = TriggerGrabPoint;

                if (PosableGrabPoint && PosableGrabPoint.Grabbable && args.Grabbable != PosableGrabPoint.Grabbable)
                    GrabPoint = null;

                if (!GrabPoint)
                {
                    if (_socketGrab)
                    {
                        var gp = args.Grabbable.GetGrabPointTransform(this, GrabpointFilter.Socket);
                        if (!gp)//in case any socket grab point is invalid, deleted, inactive
                            gp = args.Grabbable.GetGrabPointTransform(this, GrabpointFilter.Normal);

                        GrabPoint = gp;
                    }
                    else
                    {
                        GrabPoint = args.Grabbable.GetGrabPointTransform(this, GrabpointFilter.Normal);
                    }
                }
            }

            base.OnBeforeGrabbed(args);
        }

        protected override void OnGrabbed(HVRGrabArgs args)
        {
            base.OnGrabbed(args);

            if (HVRSettings.Instance.VerboseHandGrabberEvents)
            {
                Debug.Log($"{name}:OnGrabbed");
            }

            if (HandAnimator)
            {
                if (GrabPoser)
                {
                    SetAnimatorPose(GrabPoser);
                }
                else
                {
                    ResetAnimator();
                }
            }

            var grabbable = args.Grabbable;
            _grabbableControl = grabbable.GrabControl;
            _checkingSwap = true;
            _pulling = true;

            SetToggle(grabbable);

            CanActivate = false;

            var grabTransform = grabbable.transform;

            if (grabbable.Rigidbody) grabTransform = grabbable.Rigidbody.transform;

            if (OverlappingGrabbables.TryGetValue(grabbable, out var routine))
            {
                if (routine != null) StopCoroutine(routine);
                OverlappingGrabbables.Remove(grabbable);
            }

            if (grabbable.DisableHandCollision)
            {
                Rigidbody.detectCollisions = false;
            }

            DisableHandCollision(grabbable);

            if (UseDynamicGrab())
            {
                DynamicGrab();
            }

            if (!GrabPoint || args.Grabbable.GrabType == HVRGrabType.Offset)
            {
                PoseLocalRotation = Quaternion.Inverse(grabTransform.rotation) * CachedWorldRotation;
                OffsetGrab(grabbable);
                return;
            }

            if (!IsPhysicsPose)
            {
                IsLineGrab = PosableGrabPoint && PosableGrabPoint.IsLineGrab;

                if (IsLineGrab)
                {
                    SetupLineGrab(grabbable);
                }

                if (IsLineGrab && !_primaryGrabPointGrab)
                {
                    Quaternion handRotation;

                    if (PosableGrabPoint.LineInitialCanRotate)
                    {
                        handRotation = Quaternion.FromToRotation(LineGrabHandVector, LineGrabVector) * transform.rotation * HandModel.localRotation;
                    }
                    else if (_flippedLinePose)
                    {
                        var poseRot = PosableGrabPoint.GetPoseWorldRotation(HandSide);
                        var delta = poseRot * Quaternion.Inverse(CachedWorldRotation);

                        Quaternion rotation;

                        if (IsV1Closest(LineGrabHandVector, transform.forward, transform.up))
                        {
                            var up = delta * transform.up;
                            rotation = Quaternion.LookRotation(LineGrabVector, up);
                        }
                        else
                        {
                            var forward = delta * transform.forward;
                            rotation = Quaternion.LookRotation(forward, LineGrabVector);
                        }

                        handRotation = rotation * HandModel.localRotation;
                    }
                    else
                    {
                        //just use the base pose rotation if rotation and flipping isn't allowed or no need to flip due to the hand's relative orientation
                        handRotation = PosableGrabPoint.GetPoseWorldRotation(HandSide);
                    }

                    PoseLocalRotation = Quaternion.Inverse(grabTransform.rotation) * handRotation;
                }
                else if (PosableGrabPoint)
                {
                    PoseLocalRotation = PosableGrabPoint.GetGrabbableRelativeRotation(HandSide);
                }
            }

            if ((!_isForceAutoGrab) && (HandGrabs || GrabbedTarget.Stationary || GrabbedTarget.GrabberCount > 1 || GrabbedTarget.IsStabbing
                                        || GrabbedTarget.IsJointGrab && !GrabbedTarget.Rigidbody))
            {
                _pulling = false;
                StartCoroutine(MoveGrab());
            }
            else
            {
                GrabPointGrab(grabbable);
            }

            if (PosableGrabPoint && ControllerOffset)
            {
                ControllerOffset.SetGrabPointOffsets(PosableGrabPoint.HandPositionOffset, PosableGrabPoint.HandRotationOffset);
            }
        }



        public static bool IsV1Closest(Vector3 v, Vector3 v1, Vector3 v2)
        {
            var vNorm = v.normalized;

            var v1Dot = Vector3.Dot(vNorm, v1.normalized);
            var v2Dot = Vector3.Dot(vNorm, v2.normalized);

            return Mathf.Abs(v1Dot) > Mathf.Abs(v2Dot);
        }


        private void SetToggle(HVRGrabbable grabbable)
        {
            var toggle = GrabTrigger == HVRGrabTrigger.Toggle;

            if (grabbable.OverrideGrabTrigger)
            {
                if (grabbable.GrabTrigger == HVRGrabTrigger.Toggle)
                {
                    toggle = true;
                }
            }

            if (toggle)
            {
                GrabToggleActive = true;
            }
        }


        private void OffsetGrab(HVRGrabbable grabbable)
        {
            TempGrabPoint = new GameObject(name + " OffsetGrabPoint");
            TempGrabPoint.transform.parent = GrabbedTarget.transform;
            TempGrabPoint.transform.position = Vector3.zero;
            TempGrabPoint.transform.localRotation = Quaternion.identity;
            GrabPoint = TempGrabPoint.transform;

            if (grabbable.ParentHandModel)
            {
                ParentHandModel(GrabPoint);
            }

            SetAnimatorPose(FallbackPoser);

            Grab(grabbable);
        }

        private void SetupLineGrab(HVRGrabbable grabbable)
        {
            _lineGrabHandRelativeDirection = GetLineGrabRelativeDirection();
            _flippedLinePose = false;
            _lineOffset = Vector3.zero;

            var mid = grabbable.transform.InverseTransformPoint(PosableGrabPoint.WorldLineMiddle);
            var point = IsInitialLineGrab ? transform.TransformPoint(GetLineGrabHandAnchor(PosableGrabPoint)) : GrabPoint.position;
            _lineOffset = GetLocalLineGrabPoint(grabbable, point) - mid;

            if (PosableGrabPoint.CanLineFlip)
            {
                _flippedLinePose = Vector3.Dot(PosableGrabPoint.WorldLine, LineGrabHandVector) < 0;
            }
        }

        private Vector3 GetLineGrabRelativeDirection()
        {
            //calculate the relative vector of the line grab line to the stored pose information
            _fakeHand.parent = GrabPoint;
            _fakeHand.localPosition = PosableGrabPoint.GetPosePositionOffset(HandSide);
            _fakeHand.localRotation = PosableGrabPoint.GetPoseRotationOffset(HandSide);

            var relativeVector = _fakeHand.InverseTransformDirection(PosableGrabPoint.WorldLine);

            _fakeHand.parent = transform;

            return relativeVector;
        }


        private Vector3 GetLocalLineGrabPoint(HVRGrabbable grabbable, Vector3 point)
        {
            var start = grabbable.transform.InverseTransformPoint(PosableGrabPoint.LineStart.position);
            var end = grabbable.transform.InverseTransformPoint(PosableGrabPoint.LineEnd.position);
            var testPoint = grabbable.transform.InverseTransformPoint(point);
            return HVRUtilities.FindNearestPointOnLine(start, end, testPoint);
        }

        protected virtual Vector3 FindClosestPoint(HVRGrabbable grabbable, out bool inside)
        {
            var closest = Palm.transform.position;
            var distance = float.PositiveInfinity;
            inside = false;

            if (grabbable.Colliders == null || grabbable.Colliders.Count == 0)
            {
                return closest;
            }

            for (var i = 0; i < grabbable.Colliders.Count; i++)
            {
                var gc = grabbable.Colliders[i];
                if (!gc.enabled || !gc.gameObject.activeInHierarchy || gc.isTrigger)
                    continue;

                var anchor = Palm.transform.position;
                Vector3 point;
                if (grabbable.HasConcaveColliders && gc is MeshCollider meshCollider && !meshCollider.convex)
                {
                    if (!gc.Raycast(new Ray(anchor, Palm.transform.forward), out var hit, .3f))
                    {
                        continue;
                    }

                    point = hit.point;
                }
                else
                {
                    point = gc.ClosestPoint(anchor);
                }

                if (point == Palm.transform.position || Vector3.Distance(Palm.transform.position, point) < .00001f)
                {
                    inside = true;
                    //palm is inside the collider or your collider is infinitely small or poorly formed and should be replaced
                    return point;
                }

                var d = Vector3.Distance(point, Palm.transform.position);
                if (d < distance)
                {
                    closest = point;
                    distance = d;
                }
            }

            return closest;
        }

        private bool UseDynamicGrab()
        {
            if (GrabbedTarget.GrabType == HVRGrabType.Offset)
                return false;

            if (GrabbedTarget.Colliders.Count == 0)
            {
                return false;
            }

            return GrabbedTarget.GrabType == HVRGrabType.PhysicPoser || ((GrabPoint == null || GrabPoint == GrabbedTarget.transform) && GrabbedTarget.PhysicsPoserFallback);
        }

        private IEnumerator MoveGrab()
        {
            //var clone = Instantiate(HandModel, GrabbedTarget.transform, true);
            //clone.position = PoseWorldPosition;
            //clone.rotation = PoseWorldRotation;
            _handMoving = true;
            var target = PoseWorldPosition;
            var offset = -HandModel.localPosition;
            var start = transform.position;

            if (IsLineGrab)
            {
                GrabAnchorLocal = GetGrabbableAnchor(GrabbedTarget, PosableGrabPoint);
                target = GrabAnchorWorld;
                offset = -GetLineGrabHandAnchor(PosableGrabPoint);
            }


            var time = (target + transform.TransformDirection(offset) - transform.position).magnitude / HandGrabSpeed;
            var elapsed = 0f;

            Rigidbody.detectCollisions = false;
            var startRot = HandModel.rotation;

            while (elapsed < time && GrabbedTarget)
            {
                if (IsLineGrab)
                {
                    target = GrabAnchorWorld;
                }
                else
                {
                    target = PoseWorldPosition;
                }

                transform.position = Vector3.Lerp(start, target + transform.TransformDirection(offset), elapsed / time);
                transform.rotation = Quaternion.Slerp(startRot, PoseWorldRotation, elapsed / time) * Quaternion.Inverse(HandModelRotation);

                elapsed += Time.fixedDeltaTime;
                yield return _wffu;
            }

            _handMoving = false;

            Rigidbody.detectCollisions = true;

            if (!GrabbedTarget)
                yield break;

            if (GrabbedTarget.DisableHandCollision)
            {
                Rigidbody.detectCollisions = false;
            }

            var deltaRot = CachedWorldRotation * Quaternion.Inverse(PoseWorldRotation);
            transform.rotation = Quaternion.Inverse(deltaRot) * transform.rotation;
            transform.position = target + transform.TransformDirection(offset);
            //var angleDelta = Quaternion.Angle(PoseWorldRotation, HandWorldRotation);
            //Debug.Log($"after movegrab {angleDelta}");

            GrabPointGrab(GrabbedTarget);
        }

        private void GrabPointGrab(HVRGrabbable grabbable)
        {
            Grab(grabbable);
            if (grabbable.PoseImmediately)
                PoseHand(GrabbedTarget.ParentHandModel);
        }

        public virtual void NetworkGrab(HVRGrabbable grabbable)
        {
            CommonGrab(grabbable);
        }

        protected virtual void Grab(HVRGrabbable grabbable)
        {
            CommonGrab(grabbable);
            Grabbed.Invoke(this, grabbable);
        }

        private void CommonGrab(HVRGrabbable grabbable)
        {
            SetupGrab(grabbable);
            IsClimbing = grabbable.GetComponent<HVRClimbable>();
            if (grabbable.HandGrabbedClip)
                if (SFXPlayer.Instance) SFXPlayer.Instance.PlaySFX(grabbable.HandGrabbedClip, transform.position);
        }

        public void SetupGrab(HVRGrabbable grabbable)
        {
            if (grabbable.IsJointGrab)
            {
                bool final;
                if (!grabbable.Rigidbody)
                {
                    final = true;
                }
                else
                {
                    //determine if a pull to hand joint should be enabled, or should the final strong joint be enabled
                    final = grabbable.GrabType == HVRGrabType.Offset || grabbable.Stationary ||
                            (grabbable.RemainsKinematic && grabbable.Rigidbody.isKinematic) || !_pulling || _forceFullyGrabbed;
                }

                if (final)
                {
                    SetupConfigurableJoint(grabbable, true);
                }
                else //needs pulling and rotating into position
                {
                    SetupConfigurableJoint(grabbable);

                    PullingGrabbable = true;
                    _pullingTimer = 0f;
                }

                if (grabbable.Rigidbody && (!grabbable.Rigidbody.isKinematic || !grabbable.RemainsKinematic))
                {
                    grabbable.Rigidbody.isKinematic = false;
                    grabbable.Rigidbody.collisionDetectionMode = grabbable.CollisionDetection;
                }
            }

            if (GrabPoint)
            {
                grabbable.HeldGrabPoints.Add(GrabPoint);
            }
        }

        internal Vector3 GetGrabbableAnchor(HVRGrabbable grabbable, HVRPosableGrabPoint posableGrabPoint)
        {
            var grabTransform = grabbable.transform;

            if (grabbable.Rigidbody) grabTransform = grabbable.Rigidbody.transform;

            if (IsLineGrab)
            {
                return grabTransform.InverseTransformPoint(PosableGrabPoint.WorldLineMiddle);
            }

            var positionOffset = HandModelPosition;
            var rotationOffset = HandModelRotation;

            if (posableGrabPoint)
            {
                if (posableGrabPoint.IsJointAnchor)
                    return posableGrabPoint.transform.localPosition;
                positionOffset = posableGrabPoint.GetPosePositionOffset(HandSide);
                rotationOffset = posableGrabPoint.GetPoseRotationOffset(HandSide);
            }
            else if (IsPhysicsPose)
            {
                positionOffset = PhysicsHandPosition;
                rotationOffset = PhysicsHandRotation;
            }
            else
            {
                return grabTransform.InverseTransformPoint(JointAnchorWorldPosition);
            }

            _fakeHand.localPosition = HandModelPosition;
            _fakeHand.localRotation = HandModelRotation;

            if (IsPhysicsPose)
            {
                _fakeHandAnchor.position = Palm.position;
            }
            else
            {
                _fakeHandAnchor.position = JointAnchorWorldPosition;
            }

            _fakeHand.parent = GrabPoint;
            _fakeHand.localPosition = positionOffset;
            _fakeHand.localRotation = rotationOffset;

            var anchor = grabTransform.InverseTransformPoint(_fakeHandAnchor.position);

            _fakeHand.parent = transform;


            return anchor;
        }

        private Vector3 GetHandAnchor()
        {
            if (IsLineGrab)
            {
                return GetLineGrabHandAnchor(PosableGrabPoint);
            }

            if (PosableGrabPoint && PosableGrabPoint.IsJointAnchor)
            {
                var p = Quaternion.Inverse(PosableGrabPoint.GetPoseRotationOffset(HandSide)) * -PosableGrabPoint.GetPosePositionOffset(HandSide);
                p = transform.InverseTransformPoint(HandModel.TransformPoint(p));
                return p;
            }

            if (IsPhysicsPose)
            {
                return Rigidbody.transform.InverseTransformPoint(Palm.position);
            }

            return JointAnchor.localPosition;
        }

        private Vector3 GetLineGrabHandAnchor(HVRPosableGrabPoint grabPoint)
        {
            return Quaternion.Inverse(grabPoint.GetPoseRotationOffset(HandSide) * Quaternion.Inverse(HandModelRotation)) * -grabPoint.GetPosePositionOffset(HandSide) + HandModelPosition;
        }


        public Quaternion JointRotation
        {
            get
            {
                return Quaternion.Inverse(GrabbedTarget.transform.rotation) * CachedWorldRotation * Quaternion.Inverse(PoseLocalRotation);
            }
        }



        private void SetupConfigurableJoint(HVRGrabbable grabbable, bool final = false)
        {
            GrabAnchorLocal = GetGrabbableAnchor(grabbable, PosableGrabPoint);
            HandAnchorLocal = GetHandAnchor();

            var axis = Vector3.right;
            var secondaryAxis = Vector3.up;

            if (IsLineGrab)
            {
                axis = grabbable.Rigidbody.transform.InverseTransformDirection(PosableGrabPoint.WorldLine).normalized;
                secondaryAxis = axis.OrthogonalVector();
            }

            if (Joint)
            {
                Destroy(Joint);
            }

            var grabTransform = grabbable.transform;

            var noRB = false;
            GameObject owner;
            if (grabbable.Rigidbody)
            {
                owner = grabbable.Rigidbody.gameObject;
                grabTransform = grabbable.Rigidbody.transform;
            }
            else
            {
                owner = gameObject;
                noRB = true;
            }

            var startRot = grabTransform.localRotation;

            Joint = owner.AddComponent<ConfigurableJoint>();
            Joint.autoConfigureConnectedAnchor = false;
            Joint.configuredInWorldSpace = false;

            if (noRB)
            {
                Joint.anchor = HandAnchorLocal;
                Joint.connectedAnchor = transform.TransformPoint(HandAnchorLocal);
                Joint.connectedBody = null;
            }
            else
            {
                Joint.anchor = GrabAnchorLocal;
                Joint.connectedAnchor = HandAnchorLocal;
                Joint.connectedBody = Rigidbody;
            }

            Joint.axis = axis;
            Joint.secondaryAxis = secondaryAxis;
            Joint.swapBodies = false;



            if (IsLineGrab)
            {
                Joint.anchor = LineGrabAnchor + _lineOffset;

                if (final)
                {
                    _startRotation = Quaternion.Inverse(Quaternion.Inverse(grabTransform.rotation) * transform.rotation);
                    Joint.SetTargetRotationLocal(Quaternion.Inverse(Quaternion.Inverse(grabTransform.rotation) * transform.rotation), _startRotation);
                }
                else if (PosableGrabPoint.LineInitialCanRotate && !_primaryGrabPointGrab)
                {
                    var handLine = grabTransform.InverseTransformDirection(LineGrabHandVector);
                    var grabbableLine = grabTransform.InverseTransformDirection(LineGrabVector);
                    var handLocal = Quaternion.FromToRotation(grabbableLine, handLine);

                    Joint.SetTargetRotationLocal(startRot * handLocal, startRot);
                }
                else
                {
                    Joint.SetTargetRotationLocal(startRot * JointRotation, startRot);
                }
            }
            else
            {
                Joint.SetTargetRotationLocal(startRot * JointRotation, startRot);
            }



            grabbable.AddJoint(Joint, this);

            HVRJointSettings pullSettings = null;

            if (grabbable.PullingSettingsOverride)
            {
                pullSettings = grabbable.PullingSettingsOverride;
            }
            else if (PullingSettings)
            {
                pullSettings = PullingSettings;
            }

            if (!final && pullSettings != null)
            {
                pullSettings.ApplySettings(Joint);
            }
            else
            {
                HVRJointSettings settings;
                if (grabbable.JointOverride)
                {
                    settings = grabbable.JointOverride;
                }
                else if (IsLineGrab)
                {
                    settings = HVRSettings.Instance.LineGrabSettings;
                }
                else if (HVRSettings.Instance.DefaultJointSettings)
                {
                    settings = HVRSettings.Instance.DefaultJointSettings;
                }
                else
                {
                    Debug.LogError("HVRGrabbable:JointOverride or HVRSettings:DefaultJointSettings must be populated.");
                    return;
                }

                settings.ApplySettings(Joint);

                if (grabbable.TrackingType == HVRGrabTracking.FixedJoint)
                {
                    Joint.xMotion = ConfigurableJointMotion.Locked;
                    Joint.yMotion = ConfigurableJointMotion.Locked;
                    Joint.zMotion = ConfigurableJointMotion.Locked;
                    Joint.angularXMotion = ConfigurableJointMotion.Locked;
                    Joint.angularYMotion = ConfigurableJointMotion.Locked;
                    Joint.angularZMotion = ConfigurableJointMotion.Locked;
                }

                if (IsLineGrab)
                {
                    _tightlyHeld = Inputs.GetGripHoldActive(HandSide);

                    if (!_tightlyHeld || PosableGrabPoint.LineFreeRotation)
                    {
                        SetupLooseLineGrab();
                    }
                }
            }

            if (final)
            {
                UpdateGrabbableCOM(grabbable);
                OnHandAttached();
            }
        }

        protected virtual void OnHandAttached()
        {
            OnGrabbedHaptics();
        }

        protected virtual void OnGrabbedHaptics()
        {
            if (IsMine)
            {
                Controller.Vibrate(HVRInputManager.Instance.GrabHaptics.HandGrab);
            }
        }

        internal void UpdateGrabbableCOM(HVRGrabbable grabbable)
        {
            if (grabbable.Rigidbody && grabbable.PalmCenterOfMass)
            {
                //Debug.Log($"updating grabbable com { grabbable.HandGrabbers.Count}");
                if (grabbable.HandGrabbers.Count == 1)
                {
                    var p1 = grabbable.HandGrabbers[0].JointAnchorWorldPosition;
                    grabbable.Rigidbody.centerOfMass = grabbable.transform.InverseTransformPoint(p1);
                }
                else if (grabbable.HandGrabbers.Count == 2)
                {
                    var p1 = grabbable.HandGrabbers[0].JointAnchorWorldPosition;
                    var p2 = grabbable.HandGrabbers[1].JointAnchorWorldPosition;
                    grabbable.Rigidbody.centerOfMass = grabbable.transform.InverseTransformPoint((p1 + p2) / 2);
                }
            }
        }

        private void UpdateLineGrab()
        {
            if (!IsLineGrab || PullingGrabbable || !Joint)
                return;

            bool tighten;
            bool loosen;

            if (HVRSettings.Instance.LineGrabTriggerLoose)
            {
                tighten = !IsTriggerGrabActive;
                loosen = IsTriggerGrabActive;
            }
            else
            {
                tighten = GrabTrigger == HVRGrabTrigger.Active && (IsGripGrabActive || GrabToggleActive || !CanRelease) ||
                          (GrabTrigger == HVRGrabTrigger.Toggle || GrabToggleActive || !CanRelease) && !IsTriggerGrabActive;

                loosen = GrabTrigger == HVRGrabTrigger.Active && !IsGripGrabActive && !GrabToggleActive && CanRelease ||
                          (GrabTrigger == HVRGrabTrigger.Toggle || GrabToggleActive || !CanRelease) && IsTriggerGrabActive;
            }

            if (PosableGrabPoint.LineCanReposition || PosableGrabPoint.LineCanRotate)
            {
                if (!_tightlyHeld && tighten)
                {
                    _tightlyHeld = true;

                    if (!PosableGrabPoint.LineFreeRotation)
                    {
                        var settings = GrabbedTarget.JointOverride ? GrabbedTarget.JointOverride : HVRSettings.Instance.LineGrabSettings;
                        settings.ApplySettings(Joint);
                        Joint.SetTargetRotationLocal(Quaternion.Inverse(Quaternion.Inverse(GrabbedTarget.transform.rotation) * transform.rotation), _startRotation);
                    }

                    var mid = GrabbedTarget.transform.InverseTransformPoint(PosableGrabPoint.WorldLineMiddle);
                    _lineOffset = GetLocalLineGrabPoint(GrabbedTarget, transform.TransformPoint(HandAnchorLocal)) - mid;
                    Joint.anchor = LineGrabAnchor + _lineOffset;

                    UpdateGrabbableCOM(GrabbedTarget);
                }
                else if (_tightlyHeld && loosen)
                {
                    _tightlyHeld = false;
                    SetupLooseLineGrab();
                }
            }
        }

        private void SetupLooseLineGrab()
        {
            if (PosableGrabPoint.LineCanReposition)
            {
                Joint.xMotion = ConfigurableJointMotion.Limited;
                var limit = Joint.linearLimit;
                limit.limit = PosableGrabPoint.WorldLine.magnitude / 2f;
                Joint.linearLimit = limit;
                Joint.anchor = LineGrabAnchor;

                var xDrive = Joint.xDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = PosableGrabPoint.LooseDamper;
                xDrive.maximumForce = 100000f;
                Joint.xDrive = xDrive;

            }

            if (PosableGrabPoint.LineCanRotate || PosableGrabPoint.LineFreeRotation)
            {
                Joint.angularXMotion = ConfigurableJointMotion.Free;


                var xDrive = Joint.angularXDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = PosableGrabPoint.LooseAngularDamper;
                xDrive.maximumForce = 100000f;
                Joint.angularXDrive = xDrive;
            }

        }

        protected override void OnReleased(HVRGrabbable grabbable)
        {
            if (HVRSettings.Instance.VerboseHandGrabberEvents)
            {
                Debug.Log($"{name}:OnReleased");
            }
            base.OnReleased(grabbable);

            if (ControllerOffset)
            {
                ControllerOffset.ResetGrabPointOffsets();
            }

            _primaryGrabPointGrab = false;
            _socketGrab = false;
            _lineOffset = Vector3.zero;
            PullingGrabbable = false;
            _currentGrabControl = HVRGrabControls.GripOrTrigger;
            _grabbableControl = HVRGrabControls.GripOrTrigger;
            IsLineGrab = false;

            TriggerGrabPoint = null;
            ResetHandModel();

            IsPhysicsPose = false;
            _physicsPose = null;

            if (grabbable.DisableHandCollision) Rigidbody.detectCollisions = true;
            if (ApplyHandLayer) HandModel.SetLayerRecursive(HVRLayers.Hand);
            if (TempGrabPoint) Destroy(TempGrabPoint.gameObject);

            IsClimbing = false;

            if (!grabbable.BeingDestroyed)
            {
                var timeout = grabbable.OverlapTimeout;

                if (grabbable.Rigidbody && !grabbable.Rigidbody.isKinematic)
                {
                    var throwVelocity = ComputeThrowVelocity(grabbable, out var angularVelocity, true);
                    grabbable.Rigidbody.velocity = throwVelocity;
                    grabbable.Rigidbody.angularVelocity = angularVelocity;
                    //prevent clipping on throw
                    if (timeout < .2f && grabbable.Rigidbody.velocity.magnitude > 2f) timeout = .2f;
                }

                if (grabbable.Rigidbody && !grabbable.Rigidbody.isKinematic && (grabbable.RequireOverlapClearance || timeout > 0f))
                {
                    var routine = StartCoroutine(CheckReleasedOverlap(grabbable, timeout));
                    OverlappingGrabbables[grabbable] = routine;
                }
                else
                {
                    EnableHandCollision(grabbable);
                }

                grabbable.HeldGrabPoints.Remove(GrabPoint);
            }

            GrabToggleActive = false;
            GrabPoint = null;
            Released.Invoke(this, grabbable);

            OnReleasedHaptics();
        }

        protected virtual void OnReleasedHaptics()
        {
            if (IsMine)
            {
                Controller.Vibrate(HVRInputManager.Instance.GrabHaptics.HandRelease);
            }
        }

        public Vector3 GetAverageVelocity(int frames, int start)
        {
            if (start + frames > TrackedVelocityCount)
                frames = TrackedVelocityCount - start;
            return GetAverageVelocity(frames, start, RecentVelocities, TakePeakVelocities, CountPeakVelocities);
        }

        public Vector3 GetAverageAngularVelocity(int frames, int start)
        {
            if (start + frames > TrackedVelocityCount)
                frames = TrackedVelocityCount - start;
            return GetAverageVelocity(frames, start, RecentAngularVelocities);
        }

        private static readonly List<Vector3> _peakVelocities = new List<Vector3>(10);
        private static readonly IComparer<Vector3> _velocityComparer = new VelocityComparer();


        internal static Vector3 GetAverageVelocity(int frames, int start, CircularBuffer<Vector3> recentVelocities, bool takePeak = false, int nPeak = 3)
        {
            var sum = Vector3.zero;
            for (var i = start; i < start + frames; i++)
            {
                sum += recentVelocities[i];
            }

            if (Mathf.Approximately(frames, 0f))
                return Vector3.zero;

            var average = sum / frames;

            sum = Vector3.zero;

            _peakVelocities.Clear();

            for (var i = start; i < start + frames; i++)
            {

                //removing any vectors not going in the direction of the average vector
                var dot = Vector3.Dot(average.normalized, recentVelocities[i].normalized);
                if (dot < .2)
                {
                    //Debug.Log($"Filtered {average},{recentVelocities[i]},{dot}");
                    continue;
                }

                if (takePeak)
                {
                    _peakVelocities.Add(recentVelocities[i]);
                }
                else
                {
                    sum += recentVelocities[i];
                }
            }

            if (!takePeak)
            {
                return sum / frames;
            }

            if (nPeak == 0)
                return Vector3.zero;

            sum = Vector3.zero;
            SortHelper.Sort(_peakVelocities, 0, _peakVelocities.Count, _velocityComparer);

            for (int i = _peakVelocities.Count - 1, j = 0; j < nPeak; j++, i--)
            {
                if (i < 0 || i >= _peakVelocities.Count)
                    break;
                sum += _peakVelocities[i];
            }

            return sum / nPeak;
        }



        public Vector3 ComputeThrowVelocity(HVRGrabbable grabbable, out Vector3 angularVelocity, bool isThrowing = false)
        {
            if (!grabbable.Rigidbody)
            {
                angularVelocity = Vector3.zero;
                return Vector3.zero;
            }

            var grabbableVelocity = grabbable.GetAverageVelocity(ThrowLookback, ThrowLookbackStart, TakePeakVelocities, CountPeakVelocities);
            var grabbableAngular = grabbable.GetAverageAngularVelocity(ThrowLookback, ThrowLookbackStart);

            var handVelocity = GetAverageVelocity(ThrowLookback, ThrowLookbackStart);
            var handAngularVelocity = GetAverageAngularVelocity(ThrowLookback, ThrowLookbackStart);

            var throwVelocity = ReleasedVelocityFactor * handVelocity + grabbableVelocity * grabbable.ReleasedVelocityFactor;

            //Debug.Log($"{handAngularVelocity.magnitude}");

            if (handAngularVelocity.magnitude > ReleasedAngularThreshold)
            {
                //compute linear velocity from wrist rotation
                var grabbableCom = GrabPoint != null ? GrabPoint.position : grabbable.Rigidbody.worldCenterOfMass;

                Vector3 centerOfMass;
                if (ThrowingCenterOfMass && ThrowingCenterOfMass.CenterOfMass)
                {
                    centerOfMass = ThrowingCenterOfMass.CenterOfMass.position;
                }
                else
                {
                    centerOfMass = Rigidbody.worldCenterOfMass;
                }

                var cross = Vector3.Cross(handAngularVelocity, grabbableCom - centerOfMass) * grabbable.ReleasedAngularConversionFactor * ReleasedAngularConversionFactor;
                throwVelocity += cross;
            }

            angularVelocity = grabbableAngular * grabbable.ReleasedAngularFactor;

            return throwVelocity;
        }


        private IEnumerator CheckReleasedOverlap(HVRGrabbable grabbable, float timeout)
        {
            if (!OverlapSizer || !_overlapCollider)
            {
                yield break;
            }

            yield return _wffu;

            var elapsed = 0f;

            while (OverlappingGrabbables.ContainsKey(grabbable))
            {
                var count = Physics.OverlapSphereNonAlloc(OverlapSizer.transform.position, _overlapCollider.radius, _overlapColliders, ~0, QueryTriggerInteraction.Ignore);
                if (count == 0)
                    break;

                var match = false;
                for (int i = 0; i < count; i++)
                {
                    if (grabbable.IsIgnoreCollider(_overlapColliders[i]))
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                    break;

                yield return _wffu;
                elapsed += Time.fixedDeltaTime;

                if (!grabbable.RequireOverlapClearance && elapsed > timeout)
                {
                    break;
                }
            }

            EnableHandCollision(grabbable);

            //if (!IsGrabbing && CollisionHandler)
            //{
            //    CollisionHandler.SweepHand(this);
            //}

            OverlappingGrabbables.Remove(grabbable);
        }

        public void UpdateCollision(HVRGrabbable grabbable, bool enable)
        {
            if (enable)
            {
                EnableHandCollision(grabbable);
            }
            else
            {
                DisableHandCollision(grabbable);
            }
        }

        public void EnableHandCollision(HVRGrabbable grabbable)
        {
            HandPhysics.IgnoreCollision(grabbable.Colliders, false);
            HandPhysics.IgnoreCollision(grabbable.AdditionalIgnoreColliders, false);
        }

        public void DisableHandCollision(HVRGrabbable grabbable)
        {
            HandPhysics.IgnoreCollision(grabbable.Colliders, true);
            HandPhysics.IgnoreCollision(grabbable.AdditionalIgnoreColliders, true);
        }

        private readonly Dictionary<Transform, int> _layerCache = new Dictionary<Transform, int>();
        private readonly List<Transform> _layerKeys = new List<Transform>(20);

        private void DynamicGrab()
        {
            var layerMask = LayerMask.GetMask(HVRLayers.DynamicPose.ToString());
            var layer = LayerMask.NameToLayer(HVRLayers.DynamicPose.ToString());

            for (var i = 0; i < GrabbedTarget.Colliders.Count; i++)
            {
                var col = GrabbedTarget.Colliders[i];
                if (_layerCache.ContainsKey(col.transform)) continue;
                _layerKeys.Add(col.transform);
                _layerCache[col.transform] = col.gameObject.layer;
                col.gameObject.layer = layer;
            }

            try
            {
                if (_savedPose == null) _savedPose = PhysicsPoser.Hand.CreateHandPose();
                else PhysicsPoser.Hand.CopyHandData(_savedPose);

                //todo cache and reuse
                TempGrabPoint = new GameObject(name + " GrabPoint");
                TempGrabPoint.transform.parent = GrabbedTarget.transform;
                TempGrabPoint.transform.ResetLocalProps(true);
                var point = FindClosestPoint(GrabbedTarget, out var inside);
                GrabPoint = TempGrabPoint.transform;

                var count = 0;

                while (inside && count < 5)
                {
                    PhysicsPoser.transform.position -= Palm.forward * .1f;
                    point = FindClosestPoint(GrabbedTarget, out inside);
                    count++;
                }

                if (!inside && DynamicGrabPalmAdjust || _isForceAutoGrab)
                {
                    var delta = point - PhysicsPoser.Palm.position;
                    var palmDelta = Quaternion.FromToRotation(PhysicsPoser.Palm.forward, delta.normalized);
                    PhysicsPoser.transform.rotation = palmDelta * PhysicsPoser.Hand.transform.rotation;
                }

                PhysicsPoser.OpenFingers();

                var offset = PhysicsPoser.transform.position - Palm.position;
                PhysicsPoser.transform.position = point + offset;

                PhysicsPoser.SimulateClose(layerMask);
                if (_physicsPose == null) _physicsPose = PhysicsPoser.Hand.CreateHandPose();
                else PhysicsPoser.Hand.CopyHandData(_physicsPose);

                PhysicsHandRotation = Quaternion.Inverse(GrabbedTarget.transform.rotation) * PhysicsPoser.transform.rotation;
                PhysicsHandPosition = GrabbedTarget.transform.InverseTransformPoint(PhysicsPoser.transform.position);

                _physicsPose.Position = PhysicsHandPosition;
                _physicsPose.Rotation = PhysicsHandRotation;

                //Debug.Log($"{PhysicsHandRotation.eulerAngles}");
                //Debug.Log($"{PhysicsHandPosition}");
                PhysicsPoser.Hand.Pose(_savedPose, true);

#if HVR_PUN
                PhysicsPoseBytes = _physicsPose.Serialize();
#endif
                IsPhysicsPose = true;
            }
            finally
            {
                for (var i = 0; i < _layerKeys.Count; i++)
                {
                    var key = _layerKeys[i];
                    key.gameObject.layer = _layerCache[key];
                }

                _layerCache.Clear();
                _layerKeys.Clear();
            }
        }

        public bool TryAutoGrab(HVRGrabbable grabbable, HVRPosableGrabPoint grabPoint)
        {
            if (GrabTrigger == HVRGrabTrigger.Active && !Inputs.GetHoldActive(HandSide))
            {
                return false;
            }

            grabbable.Rigidbody.velocity = Vector3.zero;
            grabbable.Rigidbody.angularVelocity = Vector3.zero;

            if (grabPoint) GrabPoint = grabPoint.transform;
            else GrabPoint = null;


            _isForceAutoGrab = true;
            _primaryGrabPointGrab = true;

            try
            {
                if (TryGrab(grabbable))
                {
                    _currentGrabControl = grabbable.GrabControl;
                    return true;
                }
            }
            finally
            {
                _isForceAutoGrab = false;
            }
            return false;
        }


        private void ResetHandModel()
        {
            _hasPosed = false;

            if (!HandModel)
                return;

            if (HandModel.parent)
            {
                var listener = HandModel.parent.GetComponent<HVRDestroyListener>();
                if (listener)
                {
                    listener.Destroyed.RemoveListener(OnGrabPointDestroyed);
                    Destroy(listener);
                }
            }

            ResetHand(HandModel, HandAnimator);
            if (_collisionTransform)
            {
                ResetHand(_collisionTransform, _collisionAnimator);
            }
        }



        private void ResetHand(Transform hand, HVRHandAnimator animator)
        {
            ResetHandTransform(hand);
            if (animator)
            {
                animator.ResetToDefault();
            }
        }

        private void ResetHandTransform(Transform hand)
        {
            hand.parent = HandModelParent;
            hand.localPosition = HandModelPosition;
            hand.localRotation = HandModelRotation;
            hand.localScale = HandModelScale;
        }

        private void ResetRigidBodyProperties()
        {
            this.ExecuteNextUpdate(() =>
            {
                Rigidbody.ResetCenterOfMass();
                Rigidbody.ResetInertiaTensor();

                if (RigidOverrides)
                {
                    RigidOverrides.ApplyOverrides();
                }
            });
        }

        internal byte[] GetPoseData()
        {
            return _posableHand.CreateHandPose().Serialize();
        }

        internal void PoseHand(byte[] data)
        {
            if (CloneHandModel && _collisionHand)
            {
                _collisionHand.Pose(HVRHandPoseData.FromByteArray(data, HandSide), GrabbedTarget.ParentHandModel);
            }
            _posableHand.Pose(HVRHandPoseData.FromByteArray(data, HandSide), GrabbedTarget.ParentHandModel);
        }

        public void ChangeGrabPoint(HVRPosableGrabPoint grabPoint, float time, HVRAxis axis)
        {
            if (!GrabbedTarget || _swappingGrabPoint || GrabbedTarget.IsStabbing)
                return;

            StartCoroutine(SwapGrabPoint(grabPoint, time, axis));
        }

        private bool _swappingGrabPoint;

        protected virtual IEnumerator SwapGrabPoint(HVRPosableGrabPoint grabPoint, float time, HVRAxis axis)
        {
            var grabbable = GrabbedTarget;
            _swappingGrabPoint = true;

            try
            {
                PoseLocalRotation = grabPoint.GetGrabbableRelativeRotation(HandSide);

                var startRot = Quaternion.Inverse(HandModel.rotation) * GrabbedTarget.transform.rotation;
                var targetRot = Quaternion.Inverse(grabPoint.GetPoseWorldRotation(HandSide)) * GrabbedTarget.transform.rotation;

                var startPos = HandModel.transform.InverseTransformPoint(GrabbedTarget.transform.position);
                var targetPos = Quaternion.Inverse(grabPoint.GetPoseWorldRotation(HandSide)) * (GrabbedTarget.transform.position - grabPoint.GetPoseWorldPosition(HandSide));

                GrabPoint = grabPoint.transform;
                GrabbedTarget.RemoveJoint(this);

                if (HandAnimator)
                {
                    ResetHandModel();
                    HandAnimator.IgnoreCurls = true;
                    HandAnimator.ZeroFingerCurls();
                }

                CanRelease = false;

                if (GrabbedTarget.Rigidbody)
                {
                    GrabbedTarget.Rigidbody.detectCollisions = false;
                }


                if (time > 0f)
                {
                    var elapsed = 0f;
                    var vAxis = axis.GetVector();
                    var va = vAxis.OrthogonalVector();
                    var v1 = GrabbedTarget.transform.rotation * va;
                    var v2 = (HandModel.rotation * targetRot) * va;

                    var angle = Vector3.Angle(v1, v2);
                    var sign = Mathf.Sign(Vector3.Dot(GrabbedTarget.transform.rotation * vAxis, Vector3.Cross(v1, v2)));
                    angle = (angle * sign + 360) % 360;

                    while (GrabbedTarget && elapsed < time)
                    {
                        GrabbedTarget.transform.rotation = HandModel.rotation * startRot * Quaternion.AngleAxis(angle * elapsed / time, axis.GetVector());
                        GrabbedTarget.transform.position = HandModel.transform.TransformPoint(Vector3.Lerp(startPos, targetPos, elapsed / time));
                        if (GrabbedTarget.Rigidbody)
                            GrabbedTarget.Rigidbody.velocity = GrabbedTarget.Rigidbody.angularVelocity = Vector3.zero;
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                }

                if (!GrabbedTarget || grabbable != GrabbedTarget)
                {
                    yield break;
                }

                PoseHand(GrabbedTarget.ParentHandModel);

                GrabbedTarget.transform.rotation = HandModel.rotation * targetRot;
                GrabbedTarget.transform.position = HandModel.transform.TransformPoint(targetPos);

                SetupConfigurableJoint(GrabbedTarget, true);
            }
            finally
            {
                _swappingGrabPoint = false;
                CanRelease = true;

                if (grabbable && grabbable.Rigidbody)
                {
                    grabbable.Rigidbody.detectCollisions = true;
                }

                if (HandAnimator)
                {
                    HandAnimator.IgnoreCurls = false;
                }
            }
        }

        private bool _forceFullyGrabbed;

        /// <summary>
        /// Will grab the provided object using the provided grab point, if the grab point isn't provided then the first valid one on the object will be used.
        /// If there are no grab points that are allowed to be grabbed by this hand you shouldn't use this method.
        /// If a grab point is found it will use the saved pose information to orient the object in the hand.
        /// If the CollisionHandler field on this hand is populated, it will do a post teleport sweep to try and prevent overlapping collisions due to the object being teleported.
        /// If grabTrigger is set to toggle or manual release, it will temporarily override the hand grabber / grabbables GrabTrigger while held.
        /// If you provide 'Active'then either the hand or the grabbable need their GrabTrigger set appropriately otherwise the object will just drop on the next frame.
        /// </summary>
        public virtual void Grab(HVRGrabbable grabbable, HVRGrabTrigger grabTrigger, HVRPosableGrabPoint grabPoint = null)
        {
            try
            {
                if (grabbable.IsBeingHeld)
                    grabbable.ForceRelease();

                if (!grabPoint)
                    grabPoint = grabbable.GetGrabPoint(this, GrabpointFilter.Normal);

                if (!grabPoint)
                    return;

                GrabPoint = grabPoint.transform;

                _forceFullyGrabbed = true;

                var deltaRot = CachedWorldRotation * Quaternion.Inverse(grabPoint.GetPoseWorldRotation(HandSide));
                grabbable.transform.rotation = deltaRot * grabbable.transform.rotation;
                grabbable.transform.position += (HandModel.position - grabPoint.GetPoseWorldPosition(HandSide));


                GrabGrabbable(this, grabbable);

                if (grabTrigger == HVRGrabTrigger.Toggle)
                    GrabToggleActive = true;
                else if (grabTrigger == HVRGrabTrigger.ManualRelease)
                    CanRelease = false;


                if (CollisionHandler)
                    this.ExecuteNextUpdate(() => CollisionHandler.SweepHand(this, grabbable));
            }
            finally
            {
                _forceFullyGrabbed = false;
            }
        }

        public override void ForceRelease()
        {
            base.ForceRelease();
            CanRelease = true;
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (DrawCenterOfMass && Rigidbody)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(Rigidbody.worldCenterOfMass, .03f);
            }

            //if (_configurableJoint)
            //{
            //    Gizmos.color = Color.cyan;
            //    Gizmos.DrawWireSphere(_configurableJoint.transform.TransformPoint(_configurableJoint.anchor), .02f);
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawCube(transform.TransformPoint(_configurableJoint.connectedAnchor), new Vector3(.02f, .02f, .02f));
            //}

            //if (PosableGrabPoint && (IsHovering || IsGrabbing))
            //{
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawCube(PoseWorldPosition, new Vector3(.02f, .02f, .02f));
            //    //Debug.DrawLine(PoseWorldPosition, GrabPoint.position, Color.green);

            //    Gizmos.color = Color.blue;

            //    var grabbable = HoverTarget ?? GrabbedTarget;

            //    var p = Quaternion.Inverse(PosableGrabPoint.GetPoseRotationOffset(HandSide) * Quaternion.Inverse(HandModelRotation)) * -(PosableGrabPoint.GetPosePositionOffset(HandSide));

            //    Gizmos.DrawCube(transform.TransformPoint(p), new Vector3(.02f, .02f, .02f));
            //}
        }

#endif
    }

    internal class VelocityComparer : IComparer<Vector3>
    {
        public int Compare(Vector3 x, Vector3 y)
        {
            return x.magnitude.CompareTo(y.magnitude);
        }
    }

    public enum DynamicPoseGrabIndicator
    {
        Transform, Palm, None
    }



}
