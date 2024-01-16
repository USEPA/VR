using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MXTraceSamplerGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("State UI Objects")]
        [SerializeField] List<DeviceGUISubstate> m_stateObjects;
        #endregion
        #region Private Variables
        MXTraceSampler m_traceSampler;
        TraceSampleState m_currentState;
        DeviceGUISubstate m_currentStateRef;
        #endregion
        #region Public Properties
        public DeviceGUISubstate CurrentStateGUI { get => m_currentStateRef; }
        #endregion

        #region Initialization
        public void Init(MXTraceSampler _traceSampler)
        {
            m_traceSampler = _traceSampler;
        }
        #endregion
        #region State Functionality
        public void SetStateGUI(TraceSampleState _state, float _time)
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
            m_currentStateRef.OnStateEnter(_time);
        }

        public void UpdateState(float _deltaTime)
        {
            // Make sure state is valid
            if (m_currentStateRef == null) return;
            // Call update functionality
            m_currentStateRef.OnStateUpdate(_deltaTime);
        }
        #endregion
        #region Exit Functionality
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

