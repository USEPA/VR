using System.Collections.Generic;
using HurricaneVR.Framework.Core.Utils;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.Framework.Core.Stabbing
{
    public class HVRStabbable : MonoBehaviour
    {
        public HVRStabbableSettings Settings;

        public HVRStabEvent Stabbed = new HVRStabEvent();
        public HVRStabEvents UnStabbed = new HVRStabEvents();
        public HVRStabEvents FullStabbed = new HVRStabEvents();

        public bool IsStabbed => Stabbers.Count > 0;
        public List<HVRStabber> Stabbers;

        public Vector3 Velocity { get; private set; }
        private Vector3 _previousPosition;

        void Awake()
        {
            if (!Settings)
            {
                Settings = ScriptableObject.CreateInstance<HVRStabbableSettings>();
            }

            Settings.CheckCurve();
            Stabbers = new List<HVRStabber>();
        }

        public void Update()
        {
            //DrawBounds();
        }

        private void DrawBounds()
        {
            var bounds = transform.GetColliderBounds();
            bounds.DrawBounds();
        }

      

        public void FixedUpdate()
        {
            Cleanup();
            Velocity = (transform.position - _previousPosition) / Time.deltaTime;
            _previousPosition = transform.position;
        }

        private void Cleanup()
        {
            var cleanup = false;
            foreach (var stabber in Stabbers)
            {
                if (!stabber || !stabber.gameObject.activeInHierarchy || !stabber.enabled)
                {
                    cleanup = true;
                    break;
                }
            }

            if (cleanup)
            {
                Stabbers.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy || !e.enabled);
            }
        }


        public virtual void OnStabberEnter(HVRStabber stabber, Collision collision, ContactPoint contactPoint)
        {
            Stabbers.Add(stabber);
        }

        public virtual void OnStabberExit(HVRStabber stabber)
        {
            Stabbers.Remove(stabber);
            UnStabbed.Invoke(stabber, this);
        }

        public virtual void OnFullStabReached(HVRStabber stabber)
        {
            FullStabbed.Invoke(stabber, this);
        }

    }
}