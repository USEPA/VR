using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.Weapons
{
    public class HVRPump : MonoBehaviour
    {
        [Header("Pump Events")]

        public UnityEvent FullRelease = new UnityEvent();
        public UnityEvent EjectReached = new UnityEvent();
        public UnityEvent ChamberRound = new UnityEvent();

        [Header("Pump Settings")]
        public HVRGrabbable PumpGrabbable;

        [Tooltip("Forward speed of the charging handle when released")]
        public float ForwardSpeed = 10f;
        [Tooltip("Faux difficulty for pulling back the charging handle")]
        public float Difficulty = .05f;

        [Tooltip("Hand must move this fast to unlock the pump")]
        public float VelocityThreshold = 2f;

        [Tooltip("Bolt that moves with the charging handle")]
        public HVRBolt Bolt;

        public bool ResetOnRelease;

        [Header("Required Tracking Transforms")]
        [Tooltip("Maximum charging handle back position")]
        public Transform MaximumPosition;
        [Tooltip("Position to reach to eject the chambered round")]
        public Transform EjectPosition;
        [Tooltip("Position to reach that charging handle release will chamber a round.")]
        public Transform RequiredChamberedPosition;
        [Tooltip("Forward resting position of the charging handle")]
        public Transform Forward;
        [Tooltip("Transform to check hand distance when moving the pump")]
        public Transform HandCheckAnchor;
        [Tooltip("Transform to check when to lock the pump when moving forward")]
        public Transform PumpLockCheck;

        private float _chamberedDistance;
        private float _maximumDistance;
        private float _ejectDistance;
        private Vector3 _startPosition;
        private Coroutine _forwardRoutine;

        private bool _chamberDistanceReached;
        private bool _ejectDistanceReached;
        private HVRHandGrabber _handGrabber;
        private Vector3 _previousHandPosition;
        private Vector3 _previousVelocity;
        private Vector3 _handAcceleration;
        private bool _locked = true;
        private float _previousDistance;
        private float _lockDistance;

        protected void Awake()
        {
            _chamberedDistance = Vector3.Distance(RequiredChamberedPosition.localPosition, Forward.localPosition);
            _maximumDistance = Vector3.Distance(MaximumPosition.localPosition, Forward.localPosition);
            _ejectDistance = Vector3.Distance(EjectPosition.localPosition, Forward.localPosition);
            _lockDistance = Vector3.Distance(PumpLockCheck.localPosition, Forward.localPosition);
            _startPosition = Forward.localPosition;

            PumpGrabbable.Grabbed.AddListener(OnGrabbed);
            PumpGrabbable.Released.AddListener(OnReleased);
        }


        public void Lock()
        {
            _locked = true;
            //Debug.Log($"lock");
        }

        public void Unlock()
        {
            _locked = false;
            //Debug.Log($"unlock");
        }

        public void Update()
        {
            if (!_handGrabber)
                return;

            var velocity = _handGrabber.TrackedController.position - _previousHandPosition;
            _handAcceleration = (velocity - _previousVelocity) / Time.deltaTime;
            _previousVelocity = velocity;
            _previousHandPosition = _handGrabber.TrackedController.position;

            if (_handAcceleration.magnitude > VelocityThreshold)
            {
                Unlock();
            }

            if (_locked)
                return;

            var pullDirection = (_handGrabber.TrackedController.position - HandCheckAnchor.position);
            var backDirection = (MaximumPosition.position - Forward.position).normalized * 10;
            var amount = Vector3.Dot(pullDirection, backDirection);

            if (amount > 0)
            {
                transform.position = Forward.position + backDirection.normalized * (amount * Difficulty);

                var distance = Vector3.Distance(transform.position, Forward.position);

                CheckEject(distance);
                CheckChamberDistance(distance);
                ClampPullBack(distance, backDirection);
                CheckLock(distance);
                MoveBolt();

            }
            else
            {
                Close();
            }
        }

        private void CheckLock(float distance)
        {
            if (distance < _lockDistance && !_locked)
            {
                Close();
            }
        }

        private void CheckChamberDistance(float distance)
        {
            if (distance > _chamberedDistance && !_chamberDistanceReached)
            {
                _chamberDistanceReached = true;
            }
            else if (distance < _chamberedDistance && _chamberDistanceReached)
            {
                ChamberRound.Invoke();
                _chamberDistanceReached = false;
            }

            //if (distance < _lockDistance && _chamberDistanceReached)
            //{
            //    _chamberDistanceReached = false;
            //}
        }

        private void CheckEject(float distance)
        {
            if (distance > _ejectDistance && !_ejectDistanceReached)
            {
                EjectReached.Invoke();
                _ejectDistanceReached = true;
            }

            if (distance < _lockDistance && _ejectDistanceReached)
            {
                _ejectDistanceReached = false;
            }
        }

        private void ClampPullBack(float distance, Vector3 backDirection)
        {
            if (distance > _maximumDistance)
            {
                transform.position = Forward.position + backDirection.normalized * _maximumDistance;
            }
        }

        private void MoveBolt()
        {
            if (Bolt)
            {
                var percent = Vector3.Distance(transform.localPosition, _startPosition) / _ejectDistance;

                if (percent > .90)
                {
                    Bolt.IsPushedBack = false;
                }

                if (Bolt.IsPushedBack && percent < .90)
                    return;
                Bolt.Move(percent);
            }
        }

        private void OnReleased(HVRGrabberBase grabber, HVRGrabbable arg1)
        {
            if (!grabber.IsHandGrabber)
                return;

            _handGrabber = null;
            if (ResetOnRelease)
                Close();
        }

        private void OnGrabbed(HVRGrabberBase grabber, HVRGrabbable arg1)
        {
            if (!grabber.IsHandGrabber)
                return;

            _handGrabber = grabber as HVRHandGrabber;
            //HandCheckAnchor.transform.localPosition = PumpGrabbable.transform.InverseTransformPoint(_handGrabber.GrabPoint.position);
            HandCheckAnchor.transform.position = _handGrabber.GrabPoint.position;
            _previousHandPosition = _handGrabber.TrackedController.position;
            _previousVelocity = Vector3.zero;
        }

        public void Close()
        {
            if (_forwardRoutine != null)
                return;
            StartCoroutine(ForwardRoutine());
        }

        private IEnumerator ForwardRoutine()
        {
            PumpGrabbable.CanBeGrabbed = false;

            while (true)
            {
                var distance = Vector3.Distance(transform.localPosition, _startPosition);
                var travel = ForwardSpeed * Time.deltaTime;

                if (distance < travel)
                {
                    transform.localPosition = _startPosition;
                    MoveBolt();
                    break;
                }

                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _startPosition, travel);

                MoveBolt();

                yield return null;
            }

            _forwardRoutine = null;
            PumpGrabbable.CanBeGrabbed = true;

            if (_chamberDistanceReached)
            {
                FullRelease.Invoke();
            }
            _chamberDistanceReached = false;
            _ejectDistanceReached = false;
            Lock();
        }

    }
}