using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MXAirMonitorGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("State UI Objects")]
        [SerializeField] List<DeviceGUISubstate> m_stateObjects;
        [SerializeField] VaporGraphDisplay m_vaporGraph;
        #endregion
        #region Private Variables
        MXAirMonitor m_airMonitor;
        AirMonitorState m_currentState;
        public DeviceGUISubstate m_currentStateRef;
        #endregion
        #region Public Properties
        public DeviceGUISubstate CurrentStateGUI { get => m_currentStateRef; }
        #endregion

        #region Initialization
        public void Init(MXAirMonitor _airMonitor)
        {
            // Cache Air Monitor reference
            m_airMonitor = _airMonitor;
            // Hook up the GUI events
            m_airMonitor.VaporSensor.OnUpdateLevel += i => m_vaporGraph.UpdateGraph(i);
        }
        #endregion
        #region State Functionality
        public void SetStateGUI(AirMonitorState _state)
        {
            // Check if there is already a current state
            if (m_currentStateRef != null)
            {
                // Call exit functionality
                m_currentStateRef.OnStateExit();
                // Disable this object
                m_stateObjects[(int)m_currentState].gameObject.SetActive(false);
            }
            // Set new state and enable the relevant game object
            m_currentState = _state;
            m_currentStateRef = m_stateObjects[(int)m_currentState];
            m_currentStateRef.gameObject.SetActive(true);
            // Call enter functionality
            m_currentStateRef.OnStateEnter(0.0f); // _time
            UnityEngine.Debug.Log($"Arrived in SetStateGUI for Air Monitor || Time: {Time.time}");
        }

        public void UpdateState(float _deltaTime)
        {
            // Make sure state is valid
            if (m_currentStateRef == null) return;
            // Call update functionality
            m_currentStateRef.OnStateUpdate(_deltaTime);
        }
        #endregion
    }
}

