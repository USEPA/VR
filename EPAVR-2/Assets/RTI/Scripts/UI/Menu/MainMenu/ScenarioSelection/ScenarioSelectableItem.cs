using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class ScenarioSelectableItem : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI m_title;
        [SerializeField] Image m_icon;
        [SerializeField] Image m_gamemodeIcon;
        [SerializeField] TextMeshProUGUI m_summary;
        [SerializeField] Button m_button;
        #endregion
        #region Private Variables
        private ScenarioAsset m_scenario;
        #endregion
        #region Public Properties
        public ScenarioAsset Scenario { get => m_scenario; }
        public Button Button { get => m_button; }
        #endregion

        #region Initialization
        public void Init(ScenarioAsset _scenario)
        {
            m_scenario = _scenario;
            m_title.text = m_scenario.Title;
            if (m_scenario.Icon) m_icon.sprite = m_scenario.Icon;
            if (CoreGameManager.Instance != null && CoreGameManager.Instance.CurrentGamemodeConfig != null) m_gamemodeIcon.sprite = CoreGameManager.GetGamemodeConfig(m_scenario.Mode).Icon;
            if (m_scenario.Summary != "") m_summary.text = m_scenario.Summary;
        }
        #endregion
    }
}

