using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class DeviceGUIState : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected List<DeviceGUISubstate> m_substates;
        #endregion
        #region Protected Variables
        protected DeviceGUISubstate m_currentSubstate;
        #endregion
        #region Public Variables
        public DeviceGUISubstate CurrentSubstate { get => m_currentSubstate; }
        public List<DeviceGUISubstate> Substates { get => m_substates; }
        #endregion

        #region Initialization
        public virtual void Init()
        {
            // Make sure there are substates
            if (m_substates == null || m_substates.Count < 1)
            {
                // Initialize all substates
                for (int i = 0; i < m_substates.Count; i++)
                {
                    DeviceGUISubstate substate = m_substates[i];
                    substate.Init(this);
                    // Disable the game object by default
                    substate.gameObject.SetActive(false);
                }
                // Set the default state
                SetSubstate(0);
            }
        }
        #endregion

        #region State-Related Functionality
        public virtual void OnEnter()
        {
            // Activate the game object
            gameObject.SetActive(true);
        }

        public virtual void OnUpdate()
        {
            // Check if there is a substate
            if (m_currentSubstate) m_currentSubstate.OnStateUpdate(Time.deltaTime);
        }

        public virtual void OnExit()
        {
            // By default, return to default substate
            SetSubstate(0);
            // De-activate the game object
            gameObject.SetActive(false);
        }
        #endregion

        #region Substate-Related Functionality
        public void SetSubstate(int _stateIndex, float _time = 0.0f)
        {
            //UnityEngine.Debug.Log($"Arrived in SetSubstate[{_stateIndex}] || Time: {Time.time}");
            // Make sure substate is valid
            if (m_substates == null || m_substates.Count < 1 || _stateIndex < 0 || _stateIndex >= m_substates.Count) return;

            // Exit any prior substate
            if (m_currentSubstate != null) m_currentSubstate.OnStateExit();
            if (m_currentSubstate != null && m_currentSubstate.gameObject.activeInHierarchy) UnityEngine.Debug.Log($"Previous substate ({m_currentSubstate.gameObject.name}) || Time: {Time.time}");
            // Set substate reference
            m_currentSubstate = m_substates[_stateIndex];
            m_currentSubstate.OnStateEnter(_time);
        }
        #endregion
    }
}

