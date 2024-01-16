using System.Collections;
using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Sockets
{
    public class HVRSocketHoverScale : HVRSocketHoverAction
    {
        [Tooltip("Target scale when hovered.")]
        public Vector3 Scale = Vector3.one;
        [Tooltip("How long it takes to reach the target scale.")]
        public float ScaleTime = .25f;
        [Tooltip("If the hovered item is invalid, do we scale?")]
        public bool ScaleIfInvalid;

        private Vector3 _originalHoverTargetScale;
        private Coroutine _hoverRoutine;

        protected override void Start()
        {
            base.Start();
            if (Target)
                _originalHoverTargetScale = Target.localScale;
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnHoverEnter(HVRSocket socket, HVRGrabbable grabbable, bool isValid)
        {
            if (!isValid && !ScaleIfInvalid)
                return;
            if (Target)
                _hoverRoutine = StartCoroutine(ScaleHoverTarget(Scale));
        }

        public override void OnHoverExit(HVRSocket socket, HVRGrabbable grabbable, bool isValid)
        {
            if (_hoverRoutine != null)
                StopCoroutine(_hoverRoutine);
            if (Target)
                _hoverRoutine = StartCoroutine(ScaleHoverTarget(_originalHoverTargetScale));
        }

        private IEnumerator ScaleHoverTarget(Vector3 targetScale)
        {
            var start = Target.transform.localScale;
            var elapsed = 0f;
            while (elapsed < ScaleTime)
            {
                Target.transform.localScale = Vector3.Lerp(start, targetScale , elapsed / ScaleTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}