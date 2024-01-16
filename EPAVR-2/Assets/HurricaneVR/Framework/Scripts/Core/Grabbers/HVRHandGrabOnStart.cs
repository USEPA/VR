using Assets.HurricaneVR.Framework.Shared.Utilities;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Grabbers
{
    public class HVRHandGrabOnStart : MonoBehaviour
    {
        public HVRHandGrabber Grabber;
        public HVRGrabbable Grabbable;
        public void Start()
        {
            this.ExecuteNextUpdate(() =>
            {
                if (Grabbable && Grabber)
                {
                    if (Grabber.GrabTrigger == HVRGrabTrigger.Active ||
                        Grabbable.OverrideGrabTrigger && Grabbable.GrabTrigger == HVRGrabTrigger.Active)
                    {
                        Debug.LogWarning($"{Grabber.name} and {Grabbable.name} GrabTrigger is set to Active. The object will fall immediately if the user isn't holding the grab button.");
                    }

                    Grabbable.transform.position = Grabber.transform.position;
                    Grabber.TryGrab(Grabbable, true);
                }
            });
        }
    }
}