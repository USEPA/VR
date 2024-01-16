using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class TriggerEventListener : MonoBehaviour
    {
        #region Protected Variables
        protected Collider m_collider;
        protected Rigidbody m_rigidbody;

        protected bool m_active = true;

        protected Action<Collider> m_onTriggerEnter;
        protected Action<Collider> m_onTriggerStay;
        protected Action<Collider> m_onTriggerExit;
        #endregion
        #region Public Properties
        public Collider Collider { get => m_collider; }
        public Rigidbody Rigidbody { get => m_rigidbody; }

        public bool Active { get => m_active; set => m_active = value; }
        public Action<Collider> OnTriggerEntered { get => m_onTriggerEnter; set => m_onTriggerEnter = value; }
        public Action<Collider> OnTriggerStayed { get => m_onTriggerStay; set => m_onTriggerStay = value; }
        public Action<Collider> OnTriggerExited { get => m_onTriggerExit; set => m_onTriggerExit = value; }
        #endregion

        #region Initialization
        public void Init(bool _startActive = true, bool _addRigidbody = false)
        {
            // Cache collider component
            if (m_collider == null) m_collider = GetComponent<Collider>();
            // Check if a rigidbody needs to be added
            if (_addRigidbody && !TryGetComponent<Rigidbody>(out m_rigidbody)) m_rigidbody = gameObject.AddComponent<Rigidbody>();
            // If there is a rigidbody, disable gravity and make it kinematic
            if (m_rigidbody != null)
            {
                m_rigidbody.useGravity = false;
                m_rigidbody.isKinematic = true;
            }
            // Set the collider to be a trigger and enable event listeners
            m_collider.isTrigger = true;
            m_active = _startActive;
        }

        private void Awake()
        {
            if (m_collider == null) Init();
        }
        #endregion

        #region Trigger-Related Functionality
        private void OnTriggerEnter(Collider other)
        {
            if (!m_active) return;
            m_onTriggerEnter?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!m_active) return;
            m_onTriggerStay?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!m_active) return;
            m_onTriggerExit?.Invoke(other);
        }
        #endregion
    }
}

