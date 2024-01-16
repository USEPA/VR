using Assets.HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class HVRCenterOfMassOverride : MonoBehaviour
    {
        public Transform CenterOfMass;

        private void Start()
        {
            this.ExecuteNextUpdate(Apply);
        }

        public void Apply()
        {
            if (CenterOfMass)
                GetComponent<Rigidbody>().centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);
        }
    }
}
