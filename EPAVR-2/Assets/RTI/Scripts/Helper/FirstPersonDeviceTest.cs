using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class FirstPersonDeviceTest : MonoBehaviour
    {
        #region Inspector Assigned Variables
        public Transform m_cameraRoot;
        #endregion
        #region Private Variables
        private Animator m_animator;
        private bool m_isZoomed = false;
        private bool m_isCrouched = false;
        Vector3 m_defaultPosition;
        public Vector3 m_crouchPosition;
        #endregion
        #region Public Properties
        public Animator Animator
        {
            get
            {
                if (!m_animator) m_animator = GetComponent<Animator>();
                return m_animator;
            }
        }

        public bool IsZoomed
        {
            get => m_isZoomed;
            set
            {
                m_isZoomed = value;
                Animator.SetBool("isZoomed", value);
            }
        }

        public bool IsCrouched
        {
            get => m_isCrouched;
            set
            {
                m_isCrouched = value;
                float height = (value == true) ? m_crouchPosition.y : m_defaultPosition.y;
                m_cameraRoot.transform.localPosition = new Vector3(0, height, 0);
                //Animator.SetBool("isCrouched", value);
            }
        }
        #endregion

        private void Start()
        {
            m_defaultPosition = m_cameraRoot.transform.localPosition;
        }
        #region Helper Methods
        public void ToggleZoom()
        {
            IsZoomed = !m_isZoomed;
        }

        public void ToggleCrouch()
        {
            IsCrouched = !m_isCrouched;
        }
        #endregion
    }
}

