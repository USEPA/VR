using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.Weapons
{
    public class HVRSlide : HVRGrabbable
    {
        [Header("Slide Events")]

        public UnityEvent FullRelease = new UnityEvent();
        public UnityEvent EjectReached = new UnityEvent();

        [Header("Slide Settings")]
        [Tooltip("Forward speed of the slide when released")]
        public float ForwardSpeed = 10f;
        [Tooltip("Faux difficulty for pulling back the charging handle")]
        public float Difficulty = .05f;

        [Header("Required Tracking Transforms")]
        [Tooltip("Maximum charging handle back position")]
        public Transform MaximumPosition;
        [Tooltip("Position to reach to eject the chambered round")]
        public Transform EjectPosition;
        [Tooltip("Position to reach that charging handle release will chamber a round.")]
        public Transform RequiredChamberedPosition;
        [Tooltip("Forward resting position of the charging handle")]
        public Transform Forward;
        [Tooltip("Dummy transform on the gun to track where the grabber started grabbing")]
        public Transform GrabbedPositionTracker;


        private float _chamberedRequiredDistance;
        private float _maximumDistance;
        private float _ejectDistance;
        private Vector3 _startPosition;
        private Coroutine _forwardRoutine;

        private bool _chamberDistanceReached;
        private bool _ejectDistanceReached;
        private bool _emptyPushedBack;

        protected override void Awake()
        {
            base.Awake();

            _chamberedRequiredDistance = Vector3.Distance(RequiredChamberedPosition.localPosition, Forward.localPosition);
            _maximumDistance = Vector3.Distance(MaximumPosition.localPosition, Forward.localPosition);
            _ejectDistance = Vector3.Distance(EjectPosition.localPosition, Forward.localPosition);
            _startPosition = Forward.localPosition;
        }

        protected override void ProcessUpdate()
        {
            base.ProcessUpdate();
          
            if (PrimaryGrabber && !_emptyPushedBack)
            {
                var pullDirection = (PrimaryGrabber.transform.position - GrabbedPositionTracker.transform.position);
                var backDirection = (MaximumPosition.position - Forward.position).normalized * 10;
                var amount = Vector3.Dot(pullDirection, backDirection);

                if (amount > 0)
                {
                    transform.position = Forward.position + backDirection.normalized * amount * Difficulty;

                    var distance = Vector3.Distance(transform.position, Forward.position);

                    if (distance > _ejectDistance && !_ejectDistanceReached)
                    {
                        EjectReached.Invoke();
                        _ejectDistanceReached = true;
                    }

                    if (distance > _chamberedRequiredDistance)
                    {
                        if (!_chamberDistanceReached)
                        {
                            _chamberDistanceReached = true;
                        }

                        if (distance > _maximumDistance)
                        {
                            transform.position = Forward.position + backDirection.normalized * _maximumDistance;
                        }
                    }
                }
            }
        }

        protected override void OnGrabbed(HVRGrabberBase grabber)
        {
            _emptyPushedBack = false;
            base.OnGrabbed(grabber);
            GrabbedPositionTracker.transform.localPosition = transform.InverseTransformPoint(grabber.transform.position);
        }

        protected override void OnReleased(HVRGrabberBase grabber)
        {
            base.OnReleased(grabber);
            if (_emptyPushedBack)
                return;
            Close();
        }

        public void Close()
        {
            if (_forwardRoutine != null)
                return;
            StartCoroutine(ForwardRoutine());
        }

        private IEnumerator ForwardRoutine()
        {
            CanBeGrabbed = false;

            while (true)
            {
                var distance = Vector3.Distance(transform.localPosition, _startPosition);
                var travel = ForwardSpeed * Time.deltaTime;

                if (distance < travel)
                {
                    transform.localPosition = _startPosition;
                    break;
                }

                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _startPosition, travel);

                yield return null;
            }

            _forwardRoutine = null;
            CanBeGrabbed = true;

            if (_chamberDistanceReached)
            {
                FullRelease.Invoke();
            }
            _chamberDistanceReached = false;
            _ejectDistanceReached = false;
            _emptyPushedBack = false;
        }

        public virtual void PushBack()
        {
            transform.position = EjectPosition.position;
            _emptyPushedBack = true;
        }
    }
}