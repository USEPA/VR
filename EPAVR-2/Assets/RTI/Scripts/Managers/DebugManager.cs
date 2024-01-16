using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace L58.EPAVR
{
    public class DebugManager : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] bool m_vrSimulating = false;
        [SerializeField] bool m_showSitesOnMinimap = false;
        #endregion
        #region Private Variables
        private Action m_onSpacePressed;
        private Action<bool> m_onDebugSiteMinimapViewChange;
        private DebugConsole m_debugConsole;
        #endregion
        #region Public Properties
        public static DebugManager Instance { get; set; }
        public bool VRSimulating { get => m_vrSimulating; }
        public bool ShowSitesOnMinimap { get => m_showSitesOnMinimap; set => m_showSitesOnMinimap = value; }

        public DebugConsole Console
        {
            get
            {
                if (!m_debugConsole && VRUserManager.Instance && VRUserManager.Instance.Player != null && VRUserManager.Instance.Player.DebugConsole != null)
                {
                    m_debugConsole = VRUserManager.Instance.Player.DebugConsole;
                }
                return m_debugConsole;
            }
        }
        public Action OnSpacePressed { get => m_onSpacePressed; set => m_onSpacePressed = value; }
        public Action<bool> OnDebugSiteMinimapViewChange { get => m_onDebugSiteMinimapViewChange; set => m_onDebugSiteMinimapViewChange = value; }
        #endregion

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Debug Console Functionality
        public void ToggleDebugConsole()
        {
            if (Console == null) return;
            Console.ToggleActive();
        }
        #endregion

        #region Helper Methods
        public void ToggleMinimapDebugSiteVisibility()
        {
            SetMinimapDebugSiteVisibility(!m_showSitesOnMinimap);
        }
        public void SetMinimapDebugSiteVisibility(bool _value)
        {
            m_showSitesOnMinimap = _value;
            if (!MapManager.Instance) return;
            // Set visibility
            MapManager.Instance.SetSiteDebugView(m_showSitesOnMinimap);
        }
        #endregion
    }
}

