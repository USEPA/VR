using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ScenarioManagerInjector : MonoBehaviour, IManagerInjector
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] List<GamemodeSceneReferences> m_sceneReferences;
        [SerializeField] List<SampleArea> m_sampleAreas;
        [SerializeField] List<ContaminationSite> m_possibleContaminantSites;
        [SerializeField] List<ContaminationSite> m_presetContaminantSites;
        [SerializeField] SimulationStageGUI m_stageGUI = default;
        #endregion
        #region Private Variables
        private GamemodeSceneReferences m_selectedGamemodeReferences;
        #endregion
        #region Initialization
        public void Init()
        {
            // Make sure that there is a valid ScenarioManager first
            if (!ScenarioManager.Instance) return;
            // Get the scene references
            m_selectedGamemodeReferences = GetGamemodeReferences(CoreGameManager.Instance.CurrentGamemode);
            LoadPossibleSites(m_selectedGamemodeReferences);
            // Inject sample areas
            ScenarioManager.Instance.LoadSampleAreas(m_sampleAreas);
            // Inject contaminant sites
            //ScenarioManager.Instance.LoadContaminantSites(m_possibleContaminantSites);
            // Load random sites
            List<ContaminationSite> chosenSites = GetRandomContaminantSites(ScenarioManager.Instance.ContaminantSiteCount);
            ScenarioManager.Instance.LoadContaminantSites(chosenSites);
            // Inject scenario stage GUI reference
            ScenarioManager.Instance.LoadStageGUI(m_stageGUI);
        }
        #endregion

        #region Contaminant Site-Related Functionality
        private List<ContaminationSite> GetRandomContaminantSites(int _numSites)
        {
            UnityEngine.Debug.Log($"Arrived in GetRandomContaminantSites: {_numSites} || Time: {Time.time}");
            // Set up return list
            List<ContaminationSite> chosenSites = new List<ContaminationSite>();
            List<ContaminationSite> possibleSites = new List<ContaminationSite>(m_possibleContaminantSites);
            // Load any preset contaminant sites
            if (m_presetContaminantSites.Count > 0)
            {
                foreach (ContaminationSite presetSite in m_presetContaminantSites)
                {
                    // Add this to the chosen sites
                    chosenSites.Add(presetSite);
                    // Remove this as a possible site if the list contains it
                    if (possibleSites.Contains(presetSite))
                        possibleSites.Remove(presetSite);
                }
                UnityEngine.Debug.Log($"Loaded preset contaminant sites: {chosenSites.Count} || Time: {Time.time}");
            }

            // Add random contaminant sites until desired count is reached (or there are no more possible sites left)
            while (chosenSites.Count < _numSites || possibleSites.Count < 1) //possibleSites.Count < 1
            {
                // Select a random site
                ContaminationSite randomSite = possibleSites[Random.Range(0, possibleSites.Count)];
                // Add this to the chosen sites list and remove it from the possible sites
                chosenSites.Add(randomSite);
                possibleSites.Remove(randomSite);
            }

            // Return the chosen sites
            return chosenSites;
        }
        #endregion

        #region Helper Methods
        private GamemodeSceneReferences GetGamemodeReferences(Gamemode _mode)
        {
            if (m_sceneReferences == null || m_sceneReferences.Count < 1) return null;

            foreach(GamemodeSceneReferences sceneRefs in m_sceneReferences)
            {
                if (sceneRefs.Gamemode == _mode) return sceneRefs;
            }
            return null;
        }

        private void LoadPossibleSites(GamemodeSceneReferences _sceneRefs) 
        {
            // Load possible sites
            m_possibleContaminantSites = _sceneRefs.PossibleContaminantSites;
            // Load preset sites
            m_presetContaminantSites = _sceneRefs.PresetContaminantSites;
        }
        #endregion
    }
}

