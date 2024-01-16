using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRCollisionMonitor : MonoBehaviour
    {
        public bool Collided;
        public Collider Collider;

        private void OnCollisionEnter(Collision other)
        {
            Collided = true;
            Collider = other.collider;
        }
    }
}
