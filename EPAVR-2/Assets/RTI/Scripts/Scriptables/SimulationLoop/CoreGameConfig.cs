using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Core Game Configuration")]
    public class CoreGameConfig : ScriptableObject
    {
        #region Inspector Assigned Variables
        [Header("Game Info")]
        [SerializeField] string m_version;
        [Header("Default Configuration")]
        [SerializeField] string m_menuScene;
        [SerializeField] List<GamemodeConfigAsset> m_availableGamemodes;
        [Header("Scenario Configuration")]
        [SerializeField] List<ScenarioAsset> m_availableScenarios;
        [Header("Asset References")]
        [SerializeField] List<SampleTool> m_availableTools;
        [SerializeField] List<ChemicalAgent> m_availableChemicals;
        #endregion
        #region Public Properties
        public string Version { get => m_version; }
        public string MenuScene { get => m_menuScene; }

        public List<GamemodeConfigAsset> AvailableGamemodes { get => m_availableGamemodes; }
        public List<ScenarioAsset> AvailableScenarios { get => m_availableScenarios; }
        public List<SampleTool> AvailableTools { get => m_availableTools; }
        public List<ChemicalAgent> AvailableChemicals { get => m_availableChemicals; }
        #endregion

        #region Helper Methods
        public SampleTool GetTool(ToolType _type)
        {
            // Make sure there are tools referenced
            if (m_availableTools == null || m_availableTools.Count < 1) return null;

            // Find a tool with a matching tool type
            foreach (SampleTool tool in m_availableTools)
            {
                if (tool.Type == _type) return tool;
            }
            return null;
        }

        public ChemicalAgent GetRandomChemical()
        {
            // Make sure there are chemicals referenced
            if (m_availableChemicals == null || m_availableChemicals.Count < 1) return null;

            return m_availableChemicals[Random.Range(0, m_availableChemicals.Count)];
        }

        public GamemodeConfigAsset GetGamemode(Gamemode _mode)
        {
            //int index = ((int)_mode) - 1;
            int index = (int)_mode;
            if (m_availableGamemodes == null || index < 0 || index > m_availableGamemodes.Count) return null;
            return m_availableGamemodes[index];
        }
        #endregion
    }
}

