using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class HazardPropertyDisplay : AgentInfoDisplayItem
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] Image m_isHazardousImage;
        [SerializeField] Image m_isWashableImage;
        [SerializeField] Image m_isFlammableImage;
        [SerializeField] Image m_isExplosiveImage;
        [Header("Configuration")]
        [SerializeField] Color m_activeColor;
        [SerializeField] Color m_inactiveColor;
        #endregion

        #region Configuration
        public override void ParseAgentInfo(ChemicalAgent _agent)
        {
            m_isHazardousImage.color = GetStateColor(_agent.HazardProperties.IsHazardous);
            m_isWashableImage.color = GetStateColor(_agent.HazardProperties.IsWashable);
            m_isFlammableImage.color = GetStateColor(_agent.HazardProperties.IsFlammable);
            m_isExplosiveImage.color = GetStateColor(_agent.HazardProperties.IsExplosive);
        }

        public Color GetStateColor(bool _hasProperty)
        {
            return (_hasProperty) ? m_activeColor : m_inactiveColor;
        }
        #endregion
    }
}

