using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.HurricaneVR.Framework.Shared.Utilities;
using HurricaneVR.Framework.Core.Bags;
using HurricaneVR.Framework.Core.Sockets;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HurricaneVR.Framework.Core.Grabbers
{
    public class HVRSocket : HVRGrabberBase
    {

        [Header("Grab Settings")]
        public HVRGrabControls GrabControl = HVRGrabControls.GripOrTrigger;

        public HVRGrabDetection GrabDetectionType = HVRGrabDetection.Socket;

        [Tooltip("If true the hand socket detector must have detected this socket to be placed as well.")]
        public bool CheckHandOverlap;

        [Tooltip("Releases the current grabbable if another valid one is in range")]
        public bool ReleasesOnHover;

        public SocketHoldType HoldType = SocketHoldType.Kinematic;

        [Tooltip("If supplied, this object will be cloned when one is removed.")]
        public GameObject AutoSpawnPrefab;

        [Tooltip("If > 0 the last object released cannot be grabbed again until the timeout is reached")]
        public float GrabTimeout;

        [Tooltip("If true item's must be placed with a hand grabber.")]
        public bool GrabbableMustBeHeld = true;

        [Tooltip("If true will snatch from a hand on hover.")]
        public bool GrabsFromHand;

        [Tooltip("Actions to apply when the socket is being hovered by a grabbable. Auto populates if empty")]
        public HVRSocketHoverAction[] HoverActions;

        [Tooltip("Actions to apply when the socket is being hovered by a hand.")]
        public HVRSocketHoverAction[] HandGrabActions;

        [Tooltip("If parent grabbable is socketed, disable grabbing.")]
        public bool ParentDisablesGrab;

        [Tooltip("Parent grabbable used with ParentDisablesGrab.")]
        public HVRGrabbable ParentGrabbable;

        [Tooltip("If false then you can't remove the grabbable via hand grab.")]
        public bool CanRemoveGrabbable = true;

        [Tooltip("Scales the grabbable down to fit based on Size and the model bounds.")]
        public bool ScaleGrabbable = true;

        [Tooltip("Grabbable scales down to this size along its longest extent.")]
        public float Size;

        [Tooltip("If the grabbable stabber is stabbing something, can this socket grab it ?")]
        public bool CanGrabStabbingGrabbable;

        [Header("SFX")]
        [Tooltip("Prioritized SFX to play for anything socketed")]
        public AudioClip AudioGrabbedOverride;

        [Tooltip("Prioritized SFX to play for anything released")]
        public AudioClip AudioReleasedOverride;

        [Tooltip("Fallback grabbed sfx to play if the socketable doesn't have one.")]
        public AudioClip AudioGrabbedFallback;
        [Tooltip("Fallback released sfx to play if the socketable doesn't have one.")]
        public AudioClip AudioReleasedFallback;


        [Header("Socketable Filtering")]
        [Tooltip("Filters to filter out socketables.")]
        public HVRSocketFilter[] SocketFilters;

        [Tooltip("If multiple filters are in use, must all be valid or just one?")]
        public SocketCondition FilterCondition = SocketCondition.AND;

        [Header("Misc")]
        [Tooltip("If supplied the hand will use this point when sorting distance to the closest socket instead of the socket position")]
        public Transform DistanceSource;

        [Tooltip("Fires when an AutoSpawnedPrefab is instantiated.")]
        public SocketSpawnEvent SpawnedPrefab = new SocketSpawnEvent();


        [Header("Debugging")]
        public bool DebugScale;

        protected Transform _previousParent;
        protected Vector3 _previousScale;
        protected Bounds _modelBounds;
        protected bool _appQuitting;
        protected HVRGrabbable _timeoutGrabbable;
        protected float _mass;
        protected bool _hadRigidBody;
        protected bool _ignoreGrabSFX;
        protected Coroutine _fixPositionRoutine;


        public HVRGrabbable LinkedGrabbable { get; internal set; }

        public override bool IsGrabActivated => !IsGrabbing;
        public override bool IsHoldActive => IsGrabbing;

        public override bool AllowSwap => true;

        public virtual bool CanInteract { get; set; } = true;

        public override bool IsSocket => true;

        public bool CanAddGrabbable
        {
            get
            {
                if (!CanInteract)
                    return false;
                if (ParentDisablesGrab && ParentGrabbable && ParentGrabbable.IsSocketed)
                    return false;
                return true;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (HoldType == SocketHoldType.RemoveRigidbody)
            {
                if (!Rigidbody)
                {
                    Rigidbody = GetComponentInParent<Rigidbody>();
                }
            }

            //if (!Rigidbody && HoldType == SocketHoldType.RemoveRigidbody)
            //{
            //    HoldType = SocketHoldType.Kinematic;
            //    Debug.LogWarning($"Socket set to Kinematic, no rigidbody was found or assigned.");
            //}

            if (GrabBags.Count == 0)
            {
                var bag = gameObject.AddComponent<HVRTriggerGrabbableBag>();
                GrabBags.Add(bag);
                bag.Grabber = this;
            }

            SetupParentDisablesGrab();

            SocketFilters = GetComponents<HVRSocketFilter>();
            if (HoverActions == null || HoverActions.Length == 0)
                HoverActions = GetComponents<HVRSocketHoverAction>();

            StartCoroutine(WaitForUpdate(CheckAutoSpawn));
        }

        private void SetupParentDisablesGrab()
        {
            if (ParentDisablesGrab && !ParentGrabbable)
            {
                ParentGrabbable = GetComponentInParent<HVRGrabbable>();
            }

            if (ParentDisablesGrab && !ParentGrabbable)
            {
                Debug.LogWarning($"{gameObject.name}'s socket has ParentDisablesGrab without a ParentGrabbable");
                ParentDisablesGrab = false;
            }
        }

        private IEnumerator WaitForUpdate(Action action)
        {
            yield return null;
            action();
        }

        protected virtual void CheckAutoSpawn()
        {
            if (AutoSpawnPrefab)
            {
                var clone = Instantiate(AutoSpawnPrefab);
                var cloneGrabbable = clone.GetComponent<HVRGrabbable>();
                if (cloneGrabbable)
                {
                    TryGrab(cloneGrabbable, true, true);
                    SpawnedPrefab.Invoke(this, clone);
                }
                else
                    Debug.Log($"Socket {name} has a AutoSpawnPrefab without an HVRGrabbable component");
            }
        }

        private void OnApplicationQuit()
        {
            _appQuitting = true;
        }

        protected override void Update()
        {
            base.Update();

            if (DebugScale)
            {
                DebugScale = false;
                if (GrabbedTarget)
                    UpdateScale(GrabbedTarget);
            }
        }

        protected override bool CheckHover()
        {
            if (base.CheckHover()) return true;

            //take over another invalid socket if we are valid
            for (var g = 0; g < GrabBags.Count; g++)
            {
                var grabBag = GrabBags[g];
                for (var i = 0; i < grabBag.ValidGrabbables.Count; i++)
                {
                    var grabbable = grabBag.ValidGrabbables[i];

                    if (!grabbable.SocketHoverer)
                        continue;

                    if (!grabbable.SocketHoverer.IsValid(grabbable) && IsValid(grabbable))
                    {
                        UnhoverGrabbable(grabbable.SocketHoverer, grabbable);
                        HoverGrabbable(this, grabbable);
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool CanHover(HVRGrabbable grabbable)
        {
            if (!grabbable.Socketable)
                return false;

            if (grabbable.Socketable.AnyLinkedGrabbablesHeld)
                return false;

            if (!CanInteract)
                return false;
            if (IsGrabbing && !ReleasesOnHover)
                return false;
            if (grabbable.SocketHoverer && grabbable.SocketHoverer != this)
                return false;
            if (GrabbableMustBeHeld && grabbable.GrabberCount != 1)
                return false;
            var handGrabber = grabbable.PrimaryGrabber as HVRHandGrabber;
            if (handGrabber == null)
                return false;
            if (_timeoutGrabbable && _timeoutGrabbable == grabbable)
                return false;

            if (CheckHandOverlap)
            {
                if (!handGrabber.SocketBag.AllSockets.Contains(this))
                {
                    return false;
                }
            }
            return base.CanHover(grabbable);
        }

        protected override void OnHoverEnter(HVRGrabbable grabbable)
        {
            if (ReleasesOnHover && IsGrabbing)
            {
                ForceRelease();
            }


            if (GrabsFromHand)
            {
                if (CanAddGrabbable && grabbable.IsBeingHeld && grabbable.GrabberCount == 1 &&
                    grabbable.PrimaryGrabber.IsHandGrabber && CanGrabEx(grabbable))
                {
                    grabbable.PrimaryGrabber.ForceRelease();
                    //need to let the joint get destroyed so it doesn't bring the hand into the socket area forcefully
                    this.ExecuteNextUpdate(() => TryGrab(grabbable, true));
                }

                return;
            }

            grabbable.Released.AddListener(OnHoverGrabbableReleased);



            base.OnHoverEnter(grabbable);
            if (HoverActions != null)
            {
                foreach (var action in HoverActions)
                {
                    if (action)
                        action.OnHoverEnter(this, grabbable, IsValid(grabbable));
                }
            }
        }

        public void OnHandGrabberEntered()
        {
            if (HandGrabActions != null)
            {
                foreach (var action in HandGrabActions)
                {
                    if (action)
                        action.OnHoverEnter(this, GrabbedTarget, true);
                }
            }
        }

        public void OnHandGrabberExited()
        {
            if (HandGrabActions != null)
            {
                foreach (var action in HandGrabActions)
                {
                    if (action)
                        action.OnHoverExit(this, GrabbedTarget, true);
                }
            }
        }

        protected virtual void OnHoverGrabbableReleased(HVRGrabberBase grabber, HVRGrabbable grabbable)
        {
            UnhoverGrabbable(grabber, grabbable);
            //drop could have been a hand swap or some other swapping action
            //so we wait til next frame and see if it's not grabbed by something else this frame
            StartCoroutine(TryGrabGrabbable(grabbable));
        }

        private IEnumerator TryGrabGrabbable(HVRGrabbable grabbable)
        {
            yield return new WaitForFixedUpdate();

            if (CanAddGrabbable && TryGrab(grabbable))
            {

            }
        }

        protected override void OnHoverExit(HVRGrabbable grabbable)
        {
            grabbable.Released.RemoveListener(OnHoverGrabbableReleased);
            base.OnHoverExit(grabbable);
            if (HoverActions != null)
            {
                foreach (var action in HoverActions)
                {
                    if(action)
                        action.OnHoverExit(this, grabbable, IsValid(grabbable));
                }
            }
        }

        protected override void CheckGrab()
        {
            if (GrabbableMustBeHeld) return;
            base.CheckGrab();
        }

        //public bool DebugCanGrab;

        public override bool CanGrab(HVRGrabbable grabbable)
        {
            //if (DebugCanGrab && !GrabbedTarget)
            //{

            //}

            if (grabbable.IsBeingHeld && grabbable != GrabbedTarget)
                return false;

            return CanGrabEx(grabbable);
        }

        /// <summary>
        /// Bypass the held check for GrabsFromHand
        /// </summary>
        protected virtual bool CanGrabEx(HVRGrabbable grabbable)
        {
            if (grabbable.IsStabbing && !CanGrabStabbingGrabbable || grabbable.IsStabbed)
                return false;

            if (!grabbable.Socketable)
                return false;

            if (grabbable.Socketable.AnyLinkedGrabbablesHeld)
                return false;

            if (LinkedGrabbable && LinkedGrabbable != grabbable)
                return false;

            if (grabbable.IsBeingForcedGrabbed)
                return false;

            if (!IsValid(grabbable))
                return false;

            if (_timeoutGrabbable && _timeoutGrabbable == grabbable)
                return false;

            return base.CanGrab(grabbable) && (!GrabbedTarget || GrabbedTarget == grabbable);
        }

        public virtual bool IsValid(HVRGrabbable grabbable)
        {
            if (grabbable.IsStabbing && !CanGrabStabbingGrabbable || grabbable.IsStabbed || !grabbable.Socketable)
                return false;

            if (grabbable.Socketable.AnyLinkedGrabbablesHeld)
                return false;

            if (SocketFilters != null)
            {
                var anyValid = false;
                foreach (var filter in SocketFilters)
                {
                    if (filter.IsValid(grabbable.Socketable) && FilterCondition == SocketCondition.OR)
                    {
                        anyValid = true;
                        break;
                    }

                    if (!filter.IsValid(grabbable.Socketable) && FilterCondition == SocketCondition.AND)
                        return false;
                }

                return anyValid || FilterCondition == SocketCondition.AND;
            }

            return true;
        }

        protected internal override void OnBeforeHover(HVRGrabbable grabbable)
        {
            base.OnBeforeHover(grabbable);
            grabbable.SocketHoverer = this;
        }

        protected internal override void OnAfterHover(HVRGrabbable grabbable)
        {
            base.OnAfterHover(grabbable);
            grabbable.SocketHoverer = null;
        }

        protected override void OnGrabbed(HVRGrabArgs args)
        {
            base.OnGrabbed(args);

            var grabbable = args.Grabbable;
            _previousParent = grabbable.transform.parent;
            _previousScale = grabbable.transform.localScale;

            if (grabbable.Socketable && grabbable.Socketable.ScaleOverride)
            {
                var scaleBox = grabbable.Socketable.ScaleOverride;

                var temp = scaleBox.enabled;

                if (!scaleBox.enabled)
                {
                    scaleBox.enabled = true;
                }

                _modelBounds = scaleBox.bounds;

                if (!temp)
                {
                    scaleBox.enabled = false;
                }
            }
            else
            {
                _modelBounds = grabbable.ModelBounds;
            }

            grabbable.transform.parent = transform;
            OnGrabbableParented(grabbable);
            HandleRigidBodyGrab(grabbable);
            PlaySocketedSFX(grabbable.Socketable);

            if (args.RaiseEvents)
            {
                Grabbed.Invoke(this, grabbable);
            }
        }

        protected virtual Vector3 GetPositionOffset(HVRGrabbable grabbable)
        {
            if (!grabbable || !grabbable.Socketable || !grabbable.Socketable.SocketOrientation)
                return Vector3.zero;
            return grabbable.Socketable.SocketOrientation.localPosition;
        }

        protected virtual Quaternion GetRotationOffset(HVRGrabbable grabbable)
        {
            if (!grabbable || !grabbable.Socketable || !grabbable.Socketable.SocketOrientation)
                return Quaternion.identity;
            return grabbable.Socketable.SocketOrientation.localRotation;
        }

        protected virtual Vector3 GetTargetPosition(HVRGrabbable grabbable)
        {
            var offSet = -GetPositionOffset(grabbable);
            var delta = Quaternion.Inverse(GetRotationOffset(grabbable));
            offSet = delta * offSet;

            offSet.x *= grabbable.transform.localScale.x;
            offSet.y *= grabbable.transform.localScale.y;
            offSet.z *= grabbable.transform.localScale.z;

            return offSet;
        }

        protected virtual Quaternion GetTargetRotation(HVRGrabbable grabbable)
        {
            var socketable = grabbable.Socketable;

            if (socketable.SocketOrientation)
            {
                var rotationOffset = GetRotationOffset(grabbable);
                return Quaternion.Inverse(rotationOffset);
            }

            return Quaternion.identity;
        }

        protected virtual void OnGrabbableParented(HVRGrabbable grabbable)
        {
            UpdateScale(grabbable);
            PositionGrabbable(grabbable);
            RotateGrabbable(grabbable);
        }

        protected virtual void PositionGrabbable(HVRGrabbable grabbable)
        {
            grabbable.transform.localPosition = GetTargetPosition(grabbable);
        }

        protected virtual void RotateGrabbable(HVRGrabbable grabbable)
        {
            grabbable.transform.localRotation = GetTargetRotation(grabbable);
        }

        protected virtual void HandleRigidBodyGrab(HVRGrabbable grabbable)
        {
            if (!grabbable.Rigidbody)
                return;

            

            switch (HoldType)
            {
                case SocketHoldType.Kinematic:
                    {
                        grabbable.Rigidbody.useGravity = false;
                        grabbable.Rigidbody.velocity = Vector3.zero;
                        grabbable.Rigidbody.angularVelocity = Vector3.zero;
                        grabbable.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        grabbable.Rigidbody.isKinematic = true;
                        grabbable.Rigidbody.interpolation = RigidbodyInterpolation.None;
                        grabbable.SetAllToTrigger();
                    }
                    break;
                case SocketHoldType.RemoveRigidbody:
                    {
                        _hadRigidBody = true;

                        if (Rigidbody)
                        {
                            _mass = Rigidbody.mass;
                            Rigidbody.mass += grabbable.Rigidbody.mass;
                        }

                        Destroy(grabbable.Rigidbody);
                        if (_fixPositionRoutine != null)
                        {
                            StopCoroutine(_fixPositionRoutine);
                        }
                        _fixPositionRoutine = StartCoroutine(SetPositionNextFrame(grabbable));
                    }
                    break;
            }

        }

        protected virtual void CleanupRigidBody(HVRGrabbable grabbable)
        {
            if (HoldType == SocketHoldType.RemoveRigidbody && _hadRigidBody)
            {
                grabbable.Rigidbody = grabbable.gameObject.AddComponent<Rigidbody>();
                if (Rigidbody)
                {
                    Rigidbody.mass = _mass;
                }
            }

            grabbable.ResetRigidBody();
        }

        private IEnumerator SetPositionNextFrame(HVRGrabbable grabbable)
        {
            var position = grabbable.transform.localPosition;
            var rotation = grabbable.transform.localRotation;
            yield return null;
            if (grabbable.PrimaryGrabber == this)
            {
                grabbable.transform.localPosition = position;
                grabbable.transform.localRotation = rotation;
            }

            _fixPositionRoutine = null;
        }

        protected virtual void PlaySocketedSFX(HVRSocketable socketable)
        {
            if (_ignoreGrabSFX)
                return;

            if (AudioGrabbedOverride)
            {
                PlaySFX(AudioGrabbedOverride);
            }
            else if (socketable.SocketedClip)
            {
                PlaySFX(socketable.SocketedClip);
            }
            else if (AudioGrabbedFallback)
            {
                PlaySFX(AudioGrabbedFallback);
            }
        }

        protected virtual void PlayUnsocketedSFX(HVRGrabbable grabbable)
        {
            if (AudioReleasedOverride)
            {
                PlaySFX(AudioReleasedOverride);
            }
            else if (grabbable.Socketable.UnsocketedClip)
            {
                PlaySFX(grabbable.Socketable.UnsocketedClip);
            }
            else if (AudioReleasedFallback)
            {
                PlaySFX(AudioReleasedFallback);
            }
        }

        protected virtual void PlaySFX(AudioClip clip)
        {
            if(SFXPlayer.Instance) SFXPlayer.Instance.PlaySFX(clip, transform.position);
        }



        protected virtual void UpdateScale(HVRGrabbable grabbable)
        {
            if (!grabbable || !ScaleGrabbable)
                return;
            var extents = _modelBounds.extents;
            var axis = extents.x;
            if (extents.y > axis) axis = extents.y;
            if (extents.z > axis) axis = extents.z;
            axis *= 2;
            var ratio = Size / axis;
            ratio *= grabbable.Socketable.SocketScale;
            var counterScale = grabbable.Socketable.CounterScale;
            grabbable.transform.localScale = new Vector3(ratio * counterScale.x, ratio * counterScale.y, ratio * counterScale.z);
        }

        protected override void OnReleased(HVRGrabbable grabbable)
        {
            if (_appQuitting)
            {
                return;
            }

            base.OnReleased(grabbable);

            Released.Invoke(this, grabbable);

            if (grabbable.BeingDestroyed)
            {
                return;
            }

            CleanupRigidBody(grabbable);

            grabbable.ResetToNonTrigger();
            grabbable.transform.parent = _previousParent;

            if (ScaleGrabbable)
            {
                grabbable.transform.localScale = _previousScale;
            }

            _previousParent = null;

            PlayUnsocketedSFX(grabbable);

            CheckAutoSpawn();

            if (GrabTimeout > .00001f)
            {
                StartCoroutine(GrabTimeoutRoutine(grabbable));
            }
        }

        public virtual bool CanGrabbableBeRemoved(HVRHandGrabber hand)
        {
            if (!CanRemoveGrabbable)
                return false;
            if (!CanInteract)
                return false;
            if (ParentDisablesGrab && ParentGrabbable && ParentGrabbable.IsSocketed)
                return false;
            return true;
        }


      

        protected virtual IEnumerator GrabTimeoutRoutine(HVRGrabbable grabbable)
        {
            _timeoutGrabbable = grabbable;
            yield return new WaitForSeconds(GrabTimeout);
            _timeoutGrabbable = null;
        }

        public virtual bool TryGrab(HVRGrabbable grabbable, bool force = false, bool ignoreGrabSound = false)
        {
            try
            {
                _ignoreGrabSFX = ignoreGrabSound;
                return base.TryGrab(grabbable, force);
            }
            finally
            {
                _ignoreGrabSFX = false;
            }
        }

        /// <summary>
        /// Gets the distance between this grabbable and the provided grabber
        /// </summary>
        public virtual float GetDistanceToGrabber(Vector3 point)
        {
            var ourPoint = DistanceSource ? DistanceSource.position : transform.position;

            return Vector3.Distance(point, ourPoint);
        }

        /// <summary>
        /// Gets the Squared Distance between this grabbable and the provided grabber
        /// </summary>
        public virtual float GetSquareDistanceToGrabber(Vector3 point)
        {
            var ourPoint = DistanceSource ? DistanceSource.position : transform.position;

            return (point - ourPoint).sqrMagnitude;
        }
    }

    [Serializable]
    public class SocketSpawnEvent : UnityEvent<HVRSocket, GameObject>
    {

    }

    public enum SocketCondition
    {
        AND,
        OR
    }

    public enum SocketHoldType
    {
        Kinematic,
        RemoveRigidbody
    }
}