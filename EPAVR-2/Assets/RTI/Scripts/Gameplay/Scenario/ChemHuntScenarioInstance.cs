using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemHuntScenarioInstance : ScenarioInstance
    {
        #region Private Variables
        private List<ChemicalContaminantSite> m_chemContaminantSites;
        private List<ChemicalAgent> m_spawnedAgents;
        private int m_totalContaminantSites = 0;
        #endregion
        #region Public Properties
        public List<ChemicalContaminantSite> ChemContaminantSites { get => m_chemContaminantSites; }
        public List<ChemicalAgent> SpawnedAgents 
        {
            get
            {
                if (m_spawnedAgents == null) m_spawnedAgents = new List<ChemicalAgent>();
                return m_spawnedAgents;
            }
        }
        public int TotalContaminantSiteCount { get => m_totalContaminantSites; }
        #endregion

        #region Initialization
        public ChemHuntScenarioInstance(ScenarioAsset _scenario) : base(_scenario)
        {
            // Load the contaminant sites
            //m_contaminantSites = new List<ChemicalContaminantSite>(_sites);
        }

        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Initialize contaminant sites
            InitContaminantSites();
        }

        public override void InitContaminantSites()
        {
            if (m_activeSites == null || m_activeSites.Count < 1) return;
            // Pull the contaminant distribution from scenario data
            ChemicalDistribution possibleChemicalDistribution = new ChemicalDistribution(m_scenario.SpawnableChemicalDistribution);
            // Loop through each contaminant site and figure out which chemical to spawn
            foreach (ChemicalContaminantSite site in m_activeSites)
            {
                UnityEngine.Debug.Log($"ScenarioManager - InitContaminantSites | Arrived in initialization loop with site: {site.gameObject.name} || Time: {Time.time}");
                // Get a chemical based on the distribution 
                ChemicalAgent chemical = site.SelectChemicalFromDistribution(possibleChemicalDistribution);
                // Initialize the site
                InitContaminantSite(site, chemical);

                // Add this to the spawned agent list if necessary
                if (!SpawnedAgents.Contains(chemical)) SpawnedAgents.Add(chemical);
            }

            // Set total contaminant site count
            m_totalContaminantSites = m_chemContaminantSites.Count;
        }


        void InitContaminantSite(ChemicalContaminantSite _site, ChemicalAgent _chemical)
        {
            // Initialize the chemical
            _site.Init(_chemical);
            // Add this to the list
            if (m_chemContaminantSites == null) m_chemContaminantSites = new List<ChemicalContaminantSite>();
            m_chemContaminantSites.Add(_site);
            // Hook up events
            //_site.OnSiteCleared += (i, j) => ClearSite(_site);
        }
        #endregion


        #region Site-Related Functionality
        void ClearSite(ChemicalContaminantSite _site)
        {
            // Do the thing
        }
        #endregion

        #region Score-Related Functionality
        public override EndScenarioInfo GenerateEndInfo()
        {
            EndChemHuntScenarioInfo scenarioInfo = new EndChemHuntScenarioInfo();
            scenarioInfo.LoadInstanceInfo(this);
            return scenarioInfo;
        }
        #endregion

        public override void OnExit()
        {
            // Call base functionality
            base.OnExit();
            // Load end scenario info
            ScenarioManager.Instance.EndScenarioInfo = GenerateEndInfo();
        }
    }
}

