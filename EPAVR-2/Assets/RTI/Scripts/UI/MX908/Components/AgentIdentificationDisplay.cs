using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class AgentIdentificationDisplay : AgentInfoDisplayItem
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] Text m_agentFullName;
        [SerializeField] Text m_agentAbbreviation;
        [SerializeField] Text m_agentCAS;
        [SerializeField] TextMeshProUGUI m_agentFullNameMesh;
        [SerializeField] TextMeshProUGUI m_agentAbbreviationMesh;
        [SerializeField] TextMeshProUGUI m_agentCASMesh;
        [Header("Configuration")]
        [SerializeField] bool m_showCASLabel = false;
        #endregion
        #region Configuration
        public override void ParseAgentInfo(ChemicalAgent _agent)
        {
            UnityEngine.Debug.Log($"Arrived in ParseAgentInfo: {_agent.Name} || Time: {Time.time}");
            if (m_agentFullName) m_agentFullName.text = _agent.Name;
            if (m_agentFullNameMesh) m_agentFullNameMesh.text = _agent.Name;
            if (m_agentAbbreviation) m_agentAbbreviation.text = _agent.Abbreviation;
            if (m_agentAbbreviationMesh) m_agentAbbreviationMesh.text = _agent.Abbreviation;
            if (m_agentCAS) m_agentCAS.text = (m_showCASLabel) ? $"CAS: {_agent.CAS_RN}" : _agent.CAS_RN;
            if (m_agentCASMesh) m_agentCASMesh.text = (m_showCASLabel) ? $"CAS: {_agent.CAS_RN}" : _agent.CAS_RN;
        }
        #endregion
    }
}

