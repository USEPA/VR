using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class GamemodeConfigAsset : ScriptableObject
    {
        #region Inspector Assigned Variables
        [Header("Scenario Configuration")]
        [SerializeField] protected List<ScenarioAsset> m_availableScenarios;
        [Header("Asset References")]
        [SerializeField] protected List<SampleTool> m_availableTools;
        [SerializeField] protected UserTablet m_userTabletPrefab;
        [SerializeField] protected UserToolbox m_userToolboxPrefab;
        [Header("Misc. Configuration")]
        [SerializeField] List<ScoreType> m_scoreTypes;
        [SerializeField] protected Sprite m_icon;
        #endregion
        #region Public Properties
        public abstract Gamemode Mode { get; }

        public List<ScenarioAsset> AvailableScenarios { get => m_availableScenarios; }

        public List<SampleTool> AvailableTools { get => m_availableTools; }
        public UserTablet UserTabletPrefab { get => m_userTabletPrefab; }
        public UserToolbox UserToolboxPrefab { get => m_userToolboxPrefab; }
        public List<ScoreType> ScoreTypes { get => m_scoreTypes; }
        public Sprite Icon { get => m_icon; }
        #endregion
    }
}

