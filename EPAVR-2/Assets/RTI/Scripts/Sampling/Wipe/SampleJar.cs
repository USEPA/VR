using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class SampleJar : SampleBag
    {
        #region Inspector Assigned Variables
        [SerializeField] private GameObject m_lid;
        [Header("Default Configuration")]
        [SerializeField] private bool m_startOpen = false;
        #endregion
        #region Private Variables
        protected bool m_isOpen = false;
        #endregion
        #region Public Properties
        public bool IsOpen { get => m_isOpen; }
        #endregion

        #region Initialization
        private void Start()
        {
            SetOpen(m_startOpen);
        }
        #endregion
        #region Interaction-Related Functionality
        public void ToggleOpen()
        {
            SetOpen(!m_isOpen);
        }
        public void SetOpen(bool value)
        {
            // Set isOpen
            m_isOpen = value;
            // Enable/disable socket
            m_sampleSocket.Active = value;
            // Enable/disable lid gameobject
            m_lid.SetActive(!value);
        }
        #endregion
    }
}

