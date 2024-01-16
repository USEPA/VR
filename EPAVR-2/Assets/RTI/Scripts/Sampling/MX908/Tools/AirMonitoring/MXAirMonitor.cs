using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MXAirMonitor : MX908Tool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected VaporSensor m_vaporSensor;
        [Header("Configuration")]
        [SerializeField] protected float m_initializationTime = 1.0f;
        #endregion
        #region Protected Variables
        protected AirMonitorState m_currentState = AirMonitorState.Idle;
        
        protected ChemicalAgent m_identifiedAgent;
        protected MXAirMonitorGUI m_airMonitorGUI;
        protected System.Action<AirMonitorState> m_onStateChange;
        #endregion
        #region Public Properties
        public override MXMode Mode => MXMode.AirMonitoring;

        public AirMonitorState CurrentState { get => m_currentState; }

        public VaporSensor VaporSensor { get => m_vaporSensor; }

        #endregion

        #region Initialization
        public override void Init(MX908 _parent)
        {
            // Call base functionality
            base.Init(_parent);
            // Cache references
            m_airMonitorGUI = _parent.GUI.AirMonitorGUI;
            m_airMonitorGUI.Init(this);
            // Initialize the vapor sensor
            m_vaporSensor.Init(this);
            // Hook up UI events
            m_onStateChange += i => m_airMonitorGUI.SetStateGUI(i);
            // Set state
            //SetState(AirMonitorState.Monitoring);
        }
        #endregion

        #region Tool State Functionality
        public override void OnApplied()
        {
            // Call base functionality
            base.OnApplied();
            // Set to initializing state
            SetState(AirMonitorState.Initializing);
            // Enable GUI
            //m_airMonitorGUI.gameObject.SetActive(true);
        }

        public override void OnUpdate()
        {
            // Call base functionality
            base.OnUpdate();
            // Check the state
            switch (m_currentState)
            {
                case AirMonitorState.Monitoring:
                    // Get the readings from the vapor sensor
                    m_vaporSensor.Tick();
                    break;
            }
        }

        public override void OnRemoved()
        {
            // Call base functionality
            base.OnRemoved();
        }
        #endregion

        #region Monitoring State Functionality
        public void SetState(AirMonitorState _state)
        {
            m_currentState = _state;
            switch (m_currentState)
            {
                case AirMonitorState.Initializing:
                    StartCoroutine(StartupAirMonitor());
                    break;
                case AirMonitorState.Monitoring:
                    m_identifiedAgent = null;
                    m_vaporSensor.Startup();
                    break;
            }
            m_onStateChange?.Invoke(m_currentState);
        }
        #endregion

        #region Contaminant Functionality
        public void DisplayContaminantResults(ChemicalAgent _agent)
        {
            if (m_currentState != AirMonitorState.Monitoring || !m_airMonitorGUI || m_airMonitorGUI.CurrentStateGUI == null) return;
            m_airMonitorGUI.CurrentStateGUI.SetAgentDisplay(_agent);
        }
        #endregion

        IEnumerator StartupAirMonitor()
        {
            yield return new WaitForSeconds(m_initializationTime);
            SetState(AirMonitorState.Monitoring);
        }
    }

    public enum AirMonitorState { Initializing, Monitoring, Idle }
}

