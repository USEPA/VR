using UnityEngine;

namespace HurricaneVR.Framework.Core.Sockets
{
    public class HVRSocketable : MonoBehaviour
    {
        public HVRGrabbable Grabbable { get; private set; }
        public Transform SocketOrientation;
        public float SocketScale = 1f;

        [Tooltip("If your grabbable model is not at 1,1,1 scale. ")]
        public Vector3 CounterScale = Vector3.one;

        [Tooltip("Override renderer bounds when socket is scaling")]
        public BoxCollider ScaleOverride;
        
        public AudioClip SocketedClip;
        public AudioClip UnsocketedClip;

        [Tooltip("If populated this object cannot be socketed if any of these objects are held.")]
        public HVRGrabbable[] LinkedGrabbables;

        public bool AnyLinkedGrabbablesHeld
        {
            get
            {
                if (LinkedGrabbables == null || LinkedGrabbables.Length == 0)
                    return false;

                if (LinkedGrabbables[0].IsBeingHeld)
                    return true;

                for (int i = 1; i < LinkedGrabbables.Length; i++)
                {
                    if (LinkedGrabbables[i].IsBeingHeld)
                        return true;
                }

                return false;
            }
        }

        private void Start()
        {
            Grabbable = GetComponent<HVRGrabbable>();
        }
    }
}