using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class MX908GUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Main UI References")]
        [SerializeField] Text m_modeTitle = default;
        [SerializeField] GameObject m_defaultDisplay;
        [SerializeField] MXTraceSamplerGUI m_traceSamplerGUI = default;
        [SerializeField] MXAirMonitorGUI m_airMonitorGUI = default;
        #endregion
        #region Private Variables
        private GameObject m_currentGUI;
        private Dictionary<MXMode, GameObject> m_guiStates;
        #endregion
        #region Public Properties
        public MXTraceSamplerGUI TraceSamplerGUI { get => m_traceSamplerGUI; }
        public MXAirMonitorGUI AirMonitorGUI { get => m_airMonitorGUI; }
        #endregion
        #region Initiialization
        public void Init()
        {
            // Set up state dictionary
            m_guiStates = new Dictionary<MXMode, GameObject>();
            m_guiStates.Add(MXMode.Neutral, m_defaultDisplay);
            m_guiStates.Add(MXMode.TraceSampling, m_traceSamplerGUI.gameObject);
            m_guiStates.Add(MXMode.AirMonitoring, m_airMonitorGUI.gameObject);
            // Disable all the states by default
            foreach (GameObject state in m_guiStates.Values) state.SetActive(false);
            // Set initial display
            UpdateToolGUI(null);
        }
        #endregion

        #region Update GUI Functionality
        public void UpdateToolGUI(MX908Tool _tool)
        {
            MXMode mode = (_tool != null) ? _tool.Mode : MXMode.Neutral;
            if (m_currentGUI != null) m_currentGUI.SetActive(false);
            m_currentGUI = m_guiStates[mode];
            m_currentGUI.SetActive(true);
            //m_modeTitle.text = $"Mode: {mode}";
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

