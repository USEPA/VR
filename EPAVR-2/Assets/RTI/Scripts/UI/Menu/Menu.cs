using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class Menu : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Core References")]
        [SerializeField] protected List<MenuState> m_menuStates;
        [SerializeField] protected int m_defaultStateIndex = 0;
        #endregion
        #region Protected Variables
        protected int m_currentStateIndex = -1;
        protected MenuState m_currentState;
        #endregion
        #region Public Properties
        public MenuState CurrentState { get => m_currentState; }
        #endregion

        #region Initialization
        protected virtual void Awake()
        {
            // Make sure that there are menu states
            if (m_menuStates == null || m_menuStates.Count < 1) return;
            // Check for default state
            if (m_defaultStateIndex != 0 && m_defaultStateIndex > 0 && m_defaultStateIndex < m_menuStates.Count)
                SetState(m_defaultStateIndex);
            else
                SetState(0);
        }
        #endregion


        #region State-Related Functionality
        public virtual void SetState(int _index)
        {
            // Make sure the index is valid
            if (m_menuStates.Count < 1 || m_currentStateIndex == _index || _index < 0 || _index > m_menuStates.Count) return;
            // Call exit functionality for any existing state
            if (m_currentState != null) m_currentState.OnStateExit();

            // Set new state reference and call enter functionality
            m_currentStateIndex = _index;
            m_currentState = m_menuStates[m_currentStateIndex];
            m_currentState.OnStateEnter();
        }
        #endregion

        #region Update
        private void Update()
        {
            if (!m_currentState) return;
            m_currentState.OnStateUpdate(Time.deltaTime);
        }
        #endregion
    }
}

