using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace L58.EPAVR
{
    public class XRLayerSwapGrabbable : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Configuration")]
        [SerializeField] protected bool m_enableTriggerOnSelect = true;
        [SerializeField] protected int m_defaultSwapLayer = 0;
        #endregion
        #region Protected Variables
        protected XRGrabInteractable m_grabbable;
        protected Collider m_collider;
        protected int m_defaultLayer = 0;
        #endregion

        #region Initialization
        private void Awake()
        {
            // Try to get the grabbable component
            if (TryGetComponent<XRGrabInteractable>(out m_grabbable))
            {
                // Cache collider reference
                m_collider = GetComponent<Collider>();
                // Cache default layer
                m_defaultLayer = gameObject.layer;
                // Hook up grab events
                m_grabbable.selectEntered.AddListener(i => SetActive(true));
                m_grabbable.selectExited.AddListener(i => SetActive(false));
                // By default, set this object as inactive
                SetActive(false);
            }

        }
        #endregion

        public void SetActive(bool value)
        {
            if (value)
            {
                gameObject.layer = m_defaultLayer;
                if (m_enableTriggerOnSelect) m_collider.isTrigger = true;
            }
            else
            {
                gameObject.layer = m_defaultSwapLayer;
                if (m_enableTriggerOnSelect) m_collider.isTrigger = false;
            }
        }
    }
}

