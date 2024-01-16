using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Utils
{
    public class PhysicsSimulation : MonoBehaviour
    {

        public int maxIterations = 1000;
        public List<SimulatedBody> SimulatedBodies;

        //[Button("Run Simulation")]
        public void RunSimulation()
        {

            SimulatedBodies = transform.GetComponentsInChildren<Rigidbody>().Select(rb => new SimulatedBody(rb, rb.transform.IsChildOf(transform))).ToList();


            // Run simulation for maxIteration frames, or until all child rigidbodies are sleeping
            Physics.autoSimulation = false;
            for (int i = 0; i < maxIterations; i++)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                if (SimulatedBodies.All(body => body.rigidbody.IsSleeping() || !body.isChild))
                {
                    break;
                }
            }

            Physics.autoSimulation = true;

            // Reset bodies which are not child objects of the transform to which this script is attached
            foreach (SimulatedBody body in SimulatedBodies)
            {
                if (!body.isChild)
                {
                    body.Reset();
                }
            }

        }


        [ContextMenu("Reset")]
        public void ResetAllBodies()
        {
            if (SimulatedBodies != null)
            {
                foreach (SimulatedBody body in SimulatedBodies)
                {
                    body.Reset();
                }
            }
        }

        public struct SimulatedBody
        {
            public readonly Rigidbody rigidbody;
            public readonly bool isChild;
            readonly Vector3 originalPosition;
            readonly Quaternion originalRotation;
            readonly Transform transform;

            public SimulatedBody(Rigidbody rigidbody, bool isChild)
            {
                this.rigidbody = rigidbody;
                this.isChild = isChild;
                transform = rigidbody.transform;
                originalPosition = rigidbody.position;
                originalRotation = rigidbody.rotation;
            }

            public void Reset()
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
