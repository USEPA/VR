using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRParentOnStart : MonoBehaviour
    {
        public Transform Parent;
        public bool WorldPositionStays = true;

        private void Start()
        {
            if (Parent)
            {
                transform.SetParent(Parent, WorldPositionStays);
            }
        }
    }
}