using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class UserTablet : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private UserTabletGUI m_gui;
        [SerializeField] private GameObject m_model;
        [Header("Default Configuration")]
        [SerializeField] private bool m_startActive = true;
        #endregion
        #region Private Variables
        private bool m_active = false;
        #endregion
        #region Public Properties
        public UserTabletGUI GUI { get => m_gui; }

        public bool Active { 
            get => m_active; 
            set
            {
                m_active = value;
                SetModelVisibility(m_active);
            }
        }

        #endregion

        #region Initialization
        public void Init()
        {
            // Initiialize GUI
            m_gui.Init(this);

            // Enable/disable model on startup
            Active = m_startActive;
            //SetModelVisibility(m_startActive);
        }
        #endregion


        #region Helper Methods
        public void SetModelVisibility(bool value)
        {
            m_model.SetActive(value);
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!m_active || !m_gui) return;
            m_gui.OnUpdate();
        }
    }
}

