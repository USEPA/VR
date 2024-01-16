using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class AgentInfoDisplay : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected GameObject m_defaultContainer;
        [SerializeField] protected GameObject m_validAgentContainer;
        [Header("Individual Display Items")]
        [SerializeField] protected List<AgentInfoDisplayItem> m_agentInfoElements;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Initialize default display
            //SetAgentDisplay(null);
        }

        #region Display Configuration
        public void SetAgentDisplay(ChemicalAgent _agent)
        {

            if (_agent == null || m_agentInfoElements == null || m_agentInfoElements.Count < 1) 
            {
                //UnityEngine.Debug.Log($"Returning to default agent display screen || Time: {Time.time}");
                if (_agent == null) UnityEngine.Debug.Log($"ERROR: Failed SetAgentDisplay - Agent was null! || Time: {Time.time}");
                if (m_agentInfoElements == null) UnityEngine.Debug.Log($"ERROR: Failed SetAgentDisplay - Agent Info Elements list was null! || Time: {Time.time}");
                if (m_agentInfoElements.Count < 1) UnityEngine.Debug.Log($"ERROR: Failed SetAgentDisplay - Agent Info Elements list had less than 1 element! || Time: {Time.time}");
                // Set Agent Display back to default
                SetContainerDisplay(false);
                return;
            }
            UnityEngine.Debug.Log($"Registered Agent: {_agent.name} || Time: {Time.time}");
            // Set Agent Display to be populated
            SetContainerDisplay(true);
            // Relay agent information to each individual agent info display item
            for (int i = 0; i < m_agentInfoElements.Count; i++) m_agentInfoElements[i].ParseAgentInfo(_agent);
        }


        protected void SetContainerDisplay(bool value) 
        {
            if (!m_defaultContainer || !m_validAgentContainer) return;
            if (value == true)
            {
                m_defaultContainer.SetActive(false);
                m_validAgentContainer.SetActive(true);
            }
            else
            {
                m_defaultContainer.SetActive(true);
                m_validAgentContainer.SetActive(false);
            }
        }
        #endregion
    }
}

