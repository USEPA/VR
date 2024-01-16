using System;
using UnityEngine;
using UnityEngine.Events;

namespace HurricaneVR.Framework.Components
{
    public class HVRDestroyListener : MonoBehaviour
    {
        public HVRDestroyedEvent Destroyed = new HVRDestroyedEvent();

        private void OnDestroy()
        {
            Destroyed.Invoke(this);
            Destroyed.RemoveAllListeners();
        }
    }

    [Serializable]
    public class HVRDestroyedEvent : UnityEvent<HVRDestroyListener>
    {

    }
}
