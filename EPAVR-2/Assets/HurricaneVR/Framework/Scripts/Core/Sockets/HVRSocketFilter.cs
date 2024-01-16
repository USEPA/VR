using UnityEngine;

namespace HurricaneVR.Framework.Core.Sockets
{
    public abstract class HVRSocketFilter : MonoBehaviour
    {
        public abstract bool IsValid(HVRSocketable socketable);
    }
}