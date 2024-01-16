using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.Weapons
{
    public class HVRBolt : MonoBehaviour
    {
        public UnityEvent BoltForward = new UnityEvent();

        public Transform BackPosition;

        public float ForwardSpeed;

        private Vector3 _startingPosition;
        private Coroutine _forwardRoutine;
        private float _backDistance;

        public bool IsPushedBack { get; set; }


        private void Awake()
        {
            _startingPosition = transform.localPosition;
            _backDistance = Vector3.Distance(_startingPosition, BackPosition.localPosition);
        }

        public void Move(float percent)
        {
            transform.localPosition = Vector3.Lerp(_startingPosition, BackPosition.localPosition, percent);
        }

        public void PushBack()
        {
            if (BackPosition)
            {
                transform.localPosition = BackPosition.localPosition;
                if (_forwardRoutine != null)
                {
                    StopCoroutine(_forwardRoutine);
                }
            }

            IsPushedBack = true;
        }

        public void Close()
        {
            if (_forwardRoutine != null)
                return;
            StartCoroutine(ForwardRoutine());
        }

        private IEnumerator ForwardRoutine()
        {
            while (true)
            {
                var distance = Vector3.Distance(transform.localPosition, _startingPosition);
                var travel = ForwardSpeed * Time.deltaTime;
                if (distance < travel)
                {
                    transform.localPosition = _startingPosition;
                    break;
                }

                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _startingPosition, travel);
                yield return null;
            }

            _forwardRoutine = null;
            IsPushedBack = false;
        }
    }
}