using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class BriefingTabletGUIState : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_scenarioTitle;
        [SerializeField] private TextMeshProUGUI m_scenarioOverview;
        [SerializeField] private Image m_scenarioImage;
        [SerializeField] private BulletedList m_chemicalList;
        #endregion
        #region Private Variables
        private ScenarioAsset m_currentScenario;
        #endregion
        #region Public Properties
        #endregion

        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Make sure there is a valid scenario loaded
            if (!ScenarioManager.Instance || ScenarioManager.Instance.CurrentScenario == null) return;
            // Load the scenario info
            SetScenarioDisplay(ScenarioManager.Instance.CurrentScenario);



        }
        #endregion

        #region Scenario-Related Functionality
        public void SetScenarioDisplay(ScenarioAsset _scenario)
        {
            // Cache scenario reference
            m_currentScenario = _scenario;
            // Load the scenario briefing text
            m_scenarioOverview.text = _scenario.Briefing;
            // Load the scenario image
            m_scenarioImage.sprite = m_currentScenario.Icon;

            // Initialize or disable the possible chemical display
            LoadPossibleChemicals(m_currentScenario);
        }

        void LoadPossibleChemicals(ScenarioAsset _scenario)
        {
            // Check game mode
            if (CoreGameManager.Instance && CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt && ScenarioManager.Instance.ContaminantSites != null)
            {

                ChemHuntScenarioInstance chemScenario = (ChemHuntScenarioInstance)ScenarioManager.Instance.CurrentScenarioInstance;
                if (chemScenario.SpawnedAgents.Count < 1) return;
                foreach (ChemicalAgent agent in chemScenario.SpawnedAgents)
                {
                    m_chemicalList.AddItem(agent.Name);
                }
                /*
                List<ChemicalContaminantSite> chemSites = chemScenario.ChemContaminantSites;
                if (chemSites == null) return;
                List<ChemicalAgent> chemicals = new List<ChemicalAgent>();
                for (int i = 0; i < _scenario.SpawnableChemicalDistribution.Items.Count; i++)
                {
                    ChemicalAgent potentialAgent = _scenario.SpawnableChemicalDistribution.Items[i].Value;
                    if (chemSites.Any(i => i.Agent == potentialAgent)) chemicals.Add(potentialAgent);
                }

                if (chemicals.Count < 1) return;


                foreach (ChemicalAgent agent in chemicals)
                {
                    m_chemicalList.AddItem(agent.Name);
                }
                */
            }
            else
            {
                // Disable the container
                m_chemicalList.transform.parent.gameObject.SetActive(false);
            }
            if (_scenario.SpawnableChemicalDistribution == null || _scenario.SpawnableChemicalDistribution.Items.Count < 1 || !ScenarioManager.Instance || ScenarioManager.Instance.ContaminantSites == null || ScenarioManager.Instance.ContaminantSites.Count < 1) return;
        }
        #endregion
    }
}

