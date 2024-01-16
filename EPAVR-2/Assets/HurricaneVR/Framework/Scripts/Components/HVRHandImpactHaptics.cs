using HurricaneVR.Framework.Core.Grabbers;

namespace HurricaneVR.Framework.Components
{

    public class HVRHandImpactHaptics : HVRImpactHapticsBase
    {
        public HVRHandGrabber Hand;
        public bool HandGrabbingPrevents = true;

        protected override void Awake()
        {
            base.Awake();

            if (!Hand) TryGetComponent(out Hand);
        }

        protected override void Vibrate(float duration, float amplitude, float frequency)
        {
            if (HandGrabbingPrevents && Hand.IsGrabbing) return;
            Hand.Controller.Vibrate(amplitude, duration, frequency);
        }
    }
}