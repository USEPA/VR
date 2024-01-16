using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.HurricaneVR.Framework.Shared.Utilities;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.Core.Player
{
    [RequireComponent(typeof(HVRTeleporter))]
    public class HVRTeleportCollisonHandler : MonoBehaviour
    {
        public AfterTeleportOptions AfterTeleportOption = AfterTeleportOptions.DisableCollision;
        public LayerMask LayerMask;

        [Tooltip("Time required after teleporting to invoke stuck / unstuck events")]
        public float StuckTime = .1f;

        [Header("Required Objects")]

        [Tooltip("After teleporting, the hand will start at this position and sweep towards the final hand destination")]
        public Transform ResetTarget;

        public HVRHandGrabber LeftHand;
        public HVRJointHand LeftJointHand;

        public HVRHandGrabber RightHand;
        public HVRJointHand RightJointHand;

        [Header("Events")]
        public GrabbableStuck GrabbableStuck = new GrabbableStuck();
        public GrabbableStuck GrabbableUnstuck = new GrabbableStuck();

        public List<GrabbableCollisionTracker> LeftTrackers = new List<GrabbableCollisionTracker>();
        public List<GrabbableCollisionTracker> RightTrackers = new List<GrabbableCollisionTracker>();

        private readonly List<GrabbableCollisionTracker> _cleanup = new List<GrabbableCollisionTracker>();
        private readonly Collider[] _colliders = new Collider[100];

        private HVRGrabbable leftGrabbable;
        private HVRGrabbable rightGrabbable;

        private Vector3 _teleportStart;
        private Vector3 _teleportEnd;

        protected virtual void Start()
        {
            if (!LeftHand)
            {
                LeftHand = transform.root.GetComponentsInChildren<HVRHandGrabber>().FirstOrDefault(e => e.HandSide == HVRHandSide.Left);
            }

            if (LeftHand && !LeftJointHand)
            {
                LeftJointHand = LeftHand.gameObject.GetComponent<HVRJointHand>();
            }

            if (!RightHand)
            {
                RightHand = transform.root.GetComponentsInChildren<HVRHandGrabber>().FirstOrDefault(e => e.HandSide == HVRHandSide.Right);
            }

            if (RightHand && !RightJointHand)
            {
                RightJointHand = RightHand.gameObject.GetComponent<HVRJointHand>();
            }

            if (!ResetTarget)
            {
                var o = new GameObject("TeleportHandReset");
                o.transform.parent = transform;
                ResetTarget = o.transform;
                ResetTarget.ResetLocalProps();
                ResetTarget.localPosition = new Vector3(0f, 1.5f, 0f);
            }

            var teleporter = GetComponent<HVRTeleporter>();
            teleporter.AfterTeleport.AddListener(AfterTeleport);
            teleporter.PositionUpdate.AddListener(TeleportUpdate);
            teleporter.BeforeTeleport.AddListener(BeforeTeleport);
        }

        protected virtual void FixedUpdate()
        {
            //Debug.DrawLine(s, e, Color.red);
            //Debug.DrawLine(s, s + Vector3.up * .3f, Color.blue);

            CheckTrackers(RightTrackers, RightHand);
            CheckTrackers(LeftTrackers, LeftHand);
        }

        private void CheckTrackers(List<GrabbableCollisionTracker> trackers, HVRHandGrabber hand)
        {
            var count = trackers.Count;

            for (var i = 0; i < trackers.Count; i++)
            {
                var tracker = trackers[i];
                tracker.Frame++;

                if (tracker.Frame == 1)
                {
                    tracker.Rb.detectCollisions = false;
                }
                else if (tracker.Frame < 3)
                {
                    continue;
                }

                if (Time.time - tracker.StartTime > StuckTime && !tracker.Stuck)
                {
                    //Debug.Log($"{tracker.Rb.gameObject.name} is stuck.");
                    GrabbableStuck.Invoke(tracker.Grabbable);
                    tracker.Stuck = true;
                }

                if (CheckOverlap(tracker))
                {
                    continue;
                }


                _cleanup.Add(tracker);

                if (tracker.Stuck)
                {
                    //Debug.Log($"{tracker.Rb.gameObject.name} is free.");
                    GrabbableUnstuck.Invoke(tracker.Grabbable);
                    tracker.Stuck = false;
                }

                tracker.Rb.detectCollisions = true;
                tracker.Grabbable = null;
            }

            for (var i = 0; i < _cleanup.Count; i++)
            {
                var tracker = _cleanup[i];
                trackers.Remove(tracker);
            }

            if (count > 0 && trackers.Count == 0 && hand)
            {
                hand.CanRelease = true;
            }

            _cleanup.Clear();
        }

        private bool CheckOverlap(GrabbableCollisionTracker tracker)
        {
            //var bounds = tracker.Rb.GetColliders().ToArray().GetColliderBounds();
            //var overlaps = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _colliders, Quaternion.identity, LayerMask);
            var overlaps = Physics.OverlapBoxNonAlloc(tracker.Rb.transform.TransformPoint(tracker.Center), tracker.Bounds.extents, _colliders, Quaternion.identity, LayerMask);
            //bounds.DrawBounds();
            for (var j = 0; j < overlaps; j++)
            {
                var c = _colliders[j];

                if (tracker.Colliders.Contains(c))
                    continue;

                //concave colliders don't work correctly with compute penetration
                if (c is MeshCollider mc && !mc.convex)
                    return true;

                foreach (var tc in tracker.Colliders)
                {
                    if (Physics.ComputePenetration(tc, tc.transform.position, tc.transform.rotation, c, c.transform.position, c.transform.rotation, out var dir, out var dis))
                        return true;
                }
            }

            return false;
        }


        public virtual void BeforeTeleport(Vector3 position)
        {
            _teleportStart = position;

            leftGrabbable = null;
            rightGrabbable = null;

            if (LeftHand && LeftHand.GrabbedTarget)
            {
                leftGrabbable = LeftHand.GrabbedTarget;
                if (leftGrabbable.MasterGrabbable) leftGrabbable = leftGrabbable.MasterGrabbable;
            }

            if (RightHand && RightHand.GrabbedTarget)
            {
                rightGrabbable = RightHand.GrabbedTarget;
                if (rightGrabbable.MasterGrabbable) rightGrabbable = rightGrabbable.MasterGrabbable;
            }

            if (leftGrabbable && leftGrabbable.TryGetComponent<HVRTeleportOptions>(out var o) && o.BeforeTeleportOption == BeforeTeleportOptions.DropsGrabbable)
            {
                leftGrabbable.ForceRelease();
                leftGrabbable = null;
            }

            if (rightGrabbable && rightGrabbable != leftGrabbable && rightGrabbable.TryGetComponent(out o) && o.BeforeTeleportOption == BeforeTeleportOptions.DropsGrabbable)
            {
                rightGrabbable.ForceRelease();
                rightGrabbable = null;
            }

            if (leftGrabbable && leftGrabbable.Rigidbody)
            {
                leftGrabbable.Rigidbody.detectCollisions = false;
            }

            if (rightGrabbable && rightGrabbable.Rigidbody)
            {
                rightGrabbable.Rigidbody.detectCollisions = false;
            }

            if (LeftHand)
            {
                LeftHand.CanRelease = false;
            }

            if (RightHand)
            {
                RightHand.CanRelease = false;
            }

            if (LeftJointHand)
            {
                LeftJointHand.Disable();
                LeftJointHand.RigidBody.detectCollisions = false;
            }

            if (RightJointHand)
            {
                RightJointHand.Disable();
                RightJointHand.RigidBody.detectCollisions = false;
            }

            LeftHand.Rigidbody.velocity = Vector3.zero;
            RightHand.Rigidbody.velocity = Vector3.zero;

            _previousPosition = position;
        }

        private Vector3 _previousPosition;
        private RaycastHit[] _hits = new RaycastHit[50];

        public virtual void TeleportUpdate(Vector3 position)
        {
            _teleportEnd = position;
            var delta = position - _previousPosition;

            if (leftGrabbable && leftGrabbable.Rigidbody)
            {
                leftGrabbable.transform.position += delta;
                leftGrabbable.Rigidbody.position = leftGrabbable.transform.position;
            }

            if (rightGrabbable && rightGrabbable.Rigidbody && rightGrabbable != leftGrabbable)
            {
                rightGrabbable.transform.position += delta;
                rightGrabbable.Rigidbody.position = rightGrabbable.transform.position;
            }

            if (LeftJointHand)
            {
                LeftJointHand.transform.position += delta;
                LeftJointHand.RigidBody.position = LeftJointHand.RigidBody.position;
            }

            if (RightJointHand)
            {
                RightJointHand.transform.position += delta;
                RightJointHand.RigidBody.position = RightJointHand.RigidBody.position;
            }

            //SweepHand(LeftHand, leftGrabbable);
            //SweepHand(RightHand, rightGrabbable);

            _previousPosition = position;
        }

        public virtual void AfterTeleport()
        {
            if (LeftHand)
            {
                LeftHand.CanRelease = true;
            }

            if (RightHand)
            {
                RightHand.CanRelease = true;
            }

            if (leftGrabbable && leftGrabbable.Rigidbody && LeftTrackers.Count == 0)
            {
                leftGrabbable.Rigidbody.detectCollisions = true;
            }

            if (LeftHand && leftGrabbable && leftGrabbable.Rigidbody)
            {
                var option = GetAfterOption(leftGrabbable, out var o);

                if (option == AfterTeleportOptions.DisableCollision && LeftTrackers.Count == 0)
                {
                    var tracker = new GrabbableCollisionTracker(leftGrabbable);
                    LeftTrackers.Add(tracker);
                    LeftHand.CanRelease = false;
                }
            }

            if (rightGrabbable && rightGrabbable.Rigidbody && RightTrackers.Count == 0)
            {
                rightGrabbable.Rigidbody.detectCollisions = true;
            }

            if (RightHand && rightGrabbable && rightGrabbable.Rigidbody)
            {
                var option = GetAfterOption(rightGrabbable, out var o);

                if (option == AfterTeleportOptions.DisableCollision && RightTrackers.Count == 0)
                {
                    var tracker = new GrabbableCollisionTracker(rightGrabbable);
                    RightTrackers.Add(tracker);
                    RightHand.CanRelease = false;
                }
            }

            if (LeftHand)
            {
                LeftJointHand.RigidBody.detectCollisions = true;
            }

            if (RightJointHand)
            {
                RightJointHand.RigidBody.detectCollisions = true;
            }



            StartCoroutine(AfterFixedUpdate());

        }

        protected virtual IEnumerator AfterFixedUpdate()
        {
            yield return new WaitForFixedUpdate();

            LeftJointHand.RigidBody.position = LeftJointHand.Target.position;
            RightJointHand.RigidBody.position = RightJointHand.Target.position;

            LeftHand.Rigidbody.velocity = Vector3.zero;
            RightHand.Rigidbody.velocity = Vector3.zero;

            var direction = (_teleportStart - _teleportEnd).normalized;
            direction.y = 0f;

            SweepHand(LeftHand, leftGrabbable, direction);
            if (leftGrabbable && leftGrabbable == rightGrabbable)
            {
                SweepHand(RightHand, null, direction);
            }
            else
            {
                SweepHand(RightHand, rightGrabbable, direction);
            }

            LeftJointHand.Enable();
            RightJointHand.Enable();

            leftGrabbable = null;
            rightGrabbable = null;

        }

        public virtual bool SweepHand(HVRHandGrabber hand, HVRGrabbable g)
        {
            return SweepHand(hand, g, (ResetTarget.position - hand.transform.position).normalized);
        }

        public virtual bool SweepHand(HVRHandGrabber hand, HVRGrabbable g, Vector3 direction)
        {
            if (hand)
            {
                if (g && g.Rigidbody)
                {
                    var option = GetAfterOption(g, out var options);

                    if (option == AfterTeleportOptions.BoundingBoxSweep)
                    {
                        var grabbableOffset = g.Rigidbody.position - hand.Rigidbody.position;

                        var hit = SweepHand(hand);
                        if (g.MasterGrabbable)
                            g = g.MasterGrabbable;
                        SweepHandAndGrabbable(hand, g, options, grabbableOffset, direction);
                        return hit;
                    }
                }

                return SweepHand(hand);
            }

            return false;
        }

        public virtual void SweepHandAndGrabbable(HVRHandGrabber hand, HVRGrabbable grabbable, HVRTeleportOptions options, Vector3 grabbableOffset, Vector3 direction)
        {
            grabbable.Rigidbody.position = grabbable.transform.position = hand.Rigidbody.position + grabbableOffset;

            var bounds = hand.Rigidbody.GetColliderBounds();

            if (options && options.CustomBoundingBox)
            {
                bounds.Encapsulate(options.CustomBoundingBox.bounds);
            }
            else
            {
                if (grabbable.Colliders != null && grabbable.Colliders.Count > 0)
                    bounds.Encapsulate(grabbable.Colliders.GetColliderBounds());
            }

            var maxSide = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            var colliders = grabbable.Colliders;

            var offset = Vector3.zero;
            for (var d = 0f; d < maxSide * 1.5f; d += .01f)
            {
                offset = direction * d;
                var overlaps = Physics.OverlapBoxNonAlloc(bounds.center + offset, bounds.extents, _colliders, Quaternion.identity, LayerMask, QueryTriggerInteraction.Ignore);
                if (overlaps == 0)
                {
                    break;
                }

                var invalid = false;

                for (var i = 0; i < overlaps; i++)
                {
                    var overlappedCollider = _colliders[i];
                    var contains = false;

                    foreach (var grabbableCollider in colliders)
                    {
                        if (overlappedCollider == grabbableCollider)
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (contains)
                    {
                        continue;
                    }

                    invalid = true;

                    break;
                }

                if (!invalid)
                    break;
            }

            offset *= 1.2f;

            //if (hand.HandSide == HVRHandSide.Right) Debug.Log($"offset {offset.magnitude}");

            hand.transform.position = hand.Rigidbody.position = hand.Rigidbody.position + offset;
            grabbable.transform.position = grabbable.Rigidbody.position = grabbable.Rigidbody.position + offset;
        }


        public virtual bool SweepHand(HVRHandGrabber hand)
        {
            var target = hand.Rigidbody.position;
            var origin = ResetTarget;
            if (!origin)
            {
                origin = this.transform;
            }
            var direction = (target - origin.position).normalized;
            var length = Vector3.Distance(target, origin.position);

            hand.Rigidbody.position = origin.position;

            var bounds = hand.Rigidbody.GetColliders().ToArray().GetColliderBounds();
            var maxSide = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            var start = bounds.center - direction * maxSide * 1.05f;
            var rbDelta = bounds.center - hand.Rigidbody.position;

            //sweep test seems to collide with concave environment colliders where box cast doesn't?
            //if (hand.Rigidbody.SweepTest(direction, out var hit, length, QueryTriggerInteraction.Ignore))

            var count = Physics.BoxCastNonAlloc(start, bounds.extents, direction, _hits, Quaternion.identity, length, LayerMask, QueryTriggerInteraction.Ignore);

            if (count > 0)
            {
                var hit = _hits[0];

                if (hand.GrabbedTarget)
                {
                    var invalid = false;

                    for (int i = 0; i < count; i++)
                    {
                        var hitCollider = _hits[i].collider;
                        var contains = false;
                        for (var k = 0; k < hand.GrabbedTarget.Colliders.Count; k++)
                        {
                            var c = hand.GrabbedTarget.Colliders[k];
                            if (c == hitCollider)
                            {
                                contains = true;
                            }
                        }

                        foreach (var c in hand.GrabbedTarget.AdditionalIgnoreColliders)
                        {
                            if (c == hitCollider)
                            {
                                contains = true;
                            }
                        }

                        if (contains)
                            continue;

                        hit = _hits[i];
                        invalid = true;
                        break;
                    }

                    if (!invalid)
                    {
                        hand.transform.position = hand.Rigidbody.position = target;
                        return false;
                    }
                }

                //if (hand.HandSide == HVRHandSide.Right) Debug.Log($"hit {hit.distance} {hit.collider.name}");
                hand.Rigidbody.position += direction * (hit.distance - maxSide) + rbDelta;
                return true;
            }

            //if (hand.HandSide == HVRHandSide.Right) Debug.Log($"no hit {length}");
            hand.transform.position = hand.Rigidbody.position = target;

            return false;
        }

        private AfterTeleportOptions GetAfterOption(HVRGrabbable g, out HVRTeleportOptions options)
        {
            var option = AfterTeleportOption;

            if (g.gameObject.TryGetComponent<HVRTeleportOptions>(out options))
            {
                if (options.AfterTeleportOption != AfterTeleportOptions.TeleporterDefault)
                {
                    option = options.AfterTeleportOption;
                }
            }

            return option;
        }

    }


    [Serializable]
    public class GrabbableStuck : UnityEvent<HVRGrabbable>
    {

    }

    public class GrabbableCollisionTracker
    {
        public HVRGrabbable Grabbable;
        public Rigidbody Rb => Grabbable.Rigidbody;
        public float StartTime;
        public int Frame;
        public Bounds Bounds;
        public Collider[] Colliders;
        public Vector3 Center;
        public bool Stuck;

        public GrabbableCollisionTracker(HVRGrabbable g)
        {
            Grabbable = g;
            var rb = g.Rigidbody;
            Colliders = rb.GetColliders().ToArray();
            Bounds = Colliders.GetColliderBounds();
            Center = rb.transform.InverseTransformPoint(Bounds.center);
            StartTime = Time.time;
        }
    }
}