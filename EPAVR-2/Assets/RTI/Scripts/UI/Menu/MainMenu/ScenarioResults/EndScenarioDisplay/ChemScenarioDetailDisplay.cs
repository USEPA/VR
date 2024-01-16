using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class ChemScenarioDetailDisplay : EndScenarioDetailsDisplay
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private DoubleTextDisplay m_contaminantSiteCompletionScoreDisplay;
        [SerializeField] private DoubleTextDisplay m_contaminantSiteClearCountDisplay;
        [SerializeField] private DoubleTextDisplay m_spawnedChemicalDisplay;
        [SerializeField] private BulletedList m_spawnedChemicalAgentList;
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.ChemicalHunt;
        #endregion

        #region Initialization
        public override void Init(EndScenarioInfo _scenarioInfo)
        {
            base.Init(_scenarioInfo);
            EndChemHuntScenarioInfo chemScenarioInfo = (EndChemHuntScenarioInfo)_scenarioInfo;
            // Set the contaminant site display values
            m_contaminantSiteCompletionScoreDisplay.SetSecondaryValue($"{ScoreManager.Instance.GetScoreValue(ScoreType.SitesCleared)}/{ScoreManager.Instance.MaxSiteClearScoreValue}");
            m_contaminantSiteClearCountDisplay.SetSecondaryValue($"{chemScenarioInfo.ContaminantSitesCleared}/{chemScenarioInfo.TotalContaminantSiteCount}");

            if (chemScenarioInfo.SpawnedAgents != null && chemScenarioInfo.SpawnedAgents.Count > 0)
            {
                m_spawnedChemicalDisplay.SetSecondaryValue($"{chemScenarioInfo.SpawnedAgents.Count}");
                foreach(ChemicalAgent agent in chemScenarioInfo.SpawnedAgents)
                {
                    m_spawnedChemicalAgentList.AddItem(agent.Name);
                }
            }
        }
        #endregion
    }
}

