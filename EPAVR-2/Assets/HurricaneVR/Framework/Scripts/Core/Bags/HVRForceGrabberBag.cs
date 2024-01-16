using UnityEngine;

namespace HurricaneVR.Framework.Core.Bags
{
    public class HVRForceGrabberBag : HVRTriggerGrabbableBag
    {
        [Header("Line of Sight")]
        public Transform RayCastOrigin;
        public LayerMask LayerMask;
        
        [Tooltip("If true uses collider closest point as ray cast target, if not uses collider bounds center")]
        public bool UseClosestPoint = true;


        protected override void Start()
        {
            base.Start();
        }

        protected override void Calculate()
        {
            base.Calculate();
        }


        protected override bool IsValid(HVRGrabbable grabbable)
        {
            if (!base.IsValid(grabbable))
                return false;

            return Grabber.CheckForLineOfSight(RayCastOrigin.position, grabbable, LayerMask, MaxDistanceAllowed, UseClosestPoint);
        }
    }
}