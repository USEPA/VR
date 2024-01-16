using HurricaneVR.Framework.Core.Grabbers;
using UnityEngine;

namespace HurricaneVR.Framework.Weapons.Bow
{
    public class HVRNockingPoint : HVRSocket
    {
        protected override void Start()
        {
            base.Start();

            ScaleGrabbable = false;
            GrabbableMustBeHeld = true;
            GrabsFromHand = true;
            CanRemoveGrabbable = false;
            ParentDisablesGrab = true;
        }

        protected override void OnGrabbed(HVRGrabArgs args)
        {
            Debug.Log($"nocked");
            args.Cancel = true;
            Grabbed.Invoke(this, args.Grabbable);
            ForceRelease();
        }
    }
}