using Assets.HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class HVRRigidBodyOverrides : MonoBehaviour
    {

        public bool OverrideCOM;
        public bool OverrideRotation;
        public bool OverrideTensor;
        public bool OverrideAngularSpeed;
        public bool OverrideMaxDepenetration;

        public Vector3 CenterOfMass;
        public Vector3 InertiaTensorRotation;
        public Vector3 InertiaTensor;
        public float MaxAngularVelocity;
        public float MaxDepenetration;

        [Header("Debug")]
        public Vector3 COMGizmoSize = new Vector3(.02f, .02f, .02f);
        public bool LiveUpdate;

        public Rigidbody Rigidbody;

        void Awake()
        {
            if (!Rigidbody)
            {
                Rigidbody = GetComponent<Rigidbody>();
            }
            
            this.ExecuteNextUpdate(ApplyOverrides);
        }

        public void ApplyOverrides()
        {
            if (OverrideCOM)
            {
                Rigidbody.centerOfMass = CenterOfMass;
            }

            if (OverrideTensor)
            {
                Rigidbody.inertiaTensor = InertiaTensor;
            }

            if (OverrideRotation)
            {
                Rigidbody.inertiaTensorRotation = Quaternion.Euler(InertiaTensorRotation);
            }

            if (OverrideAngularSpeed)
            {
                Rigidbody.maxAngularVelocity = MaxAngularVelocity;
            }

            if (OverrideMaxDepenetration) Rigidbody.maxDepenetrationVelocity = MaxDepenetration;
        }

        void FixedUpdate()
        {
            if (LiveUpdate)
            {
                ApplyOverrides();
            }
        }

        void OnDrawGizmosSelected()
        {
            //if (OverrideCOM)
            {
                Gizmos.color = Color.yellow;
                if (OverrideCOM)
                {
                    Gizmos.DrawCube(transform.TransformPoint(CenterOfMass), COMGizmoSize);
                }
                else if(Rigidbody)
                {
                    Gizmos.DrawCube(Rigidbody.worldCenterOfMass, COMGizmoSize);
                }
                
            }
        }
    }
}