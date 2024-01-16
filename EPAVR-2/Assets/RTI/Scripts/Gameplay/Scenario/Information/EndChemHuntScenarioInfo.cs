using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace L58.EPAVR
{
    public class EndChemHuntScenarioInfo : EndScenarioInfo
    {
        #region Protected Variables
        private int m_contaminantSitesCleared = 0;
        private int m_totalContaminantSiteCount = 0;

        private List<ChemicalAgent> m_spawnedAgents;
        private List<ChemCloudMapMarkerUIObject> m_chemCloudMapMarkers;
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.ChemicalHunt;

        public int ContaminantSitesCleared { get => m_contaminantSitesCleared; }
        public int TotalContaminantSiteCount { get => m_totalContaminantSiteCount; }

        public List<ChemicalAgent> SpawnedAgents { get => m_spawnedAgents; }
        public List<ChemCloudMapMarkerUIObject> ChemCloudMapMarkers { get => m_chemCloudMapMarkers; }
        #endregion

        public override void LoadInstanceInfo(ScenarioInstance _instance)
        {
            base.LoadInstanceInfo(_instance);

            ChemHuntScenarioInstance chemInstance = (ChemHuntScenarioInstance)_instance;
            // Load completion info
            m_contaminantSitesCleared = chemInstance.CompletedObjectives.Count;
            m_totalContaminantSiteCount = chemInstance.TotalContaminantSiteCount;

            // Check spawned agents
            if (chemInstance.SpawnedAgents.Count > 0)
            {
                m_spawnedAgents = new List<ChemicalAgent>();
                foreach (ChemicalAgent agent in chemInstance.SpawnedAgents)
                    m_spawnedAgents.Add(agent);
            }

            // Check for chem cloud markers
            if (m_mapMarkerContainer != null)
            {
                List<ChemCloudMapMarkerUIObject> chemCloudMarkers = m_mapMarkerContainer.GetComponentsInChildren<ChemCloudMapMarkerUIObject>(true).ToList();
                if (chemCloudMarkers != null && chemCloudMarkers.Count > 0)
                {
                    m_chemCloudMapMarkers = new List<ChemCloudMapMarkerUIObject>();
                    foreach (ChemCloudMapMarkerUIObject cloudMarker in chemCloudMarkers) m_chemCloudMapMarkers.Add(cloudMarker);
                }
            }

        }
    }

}
