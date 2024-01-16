using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SitesClearedDisplay : MonoBehaviour
    {
        #region Protected Variables
        protected TextMeshProUGUI m_sitesClearedDisplay;
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            // Cache reference
            m_sitesClearedDisplay = GetComponent<TextMeshProUGUI>();
            if (ScenarioManager.Instance)
            {
                // Hook up event
                //ScenarioManager.Instance.OnSiteCleared += (i, j) => UpdateSitesCleared();
                ScenarioManager.Instance.OnObjectiveCompleted.AddListener(UpdateObjectivesCleared);
                // Set default value
                UpdateSitesCleared();
            }
            else if (RadScenarioTest.Instance)
            {
                RadScenarioTest.Instance.OnSiteCleared += i => UpdateRadSitesCleared();
                UpdateRadSitesCleared();
            }
        }

        #region Display-Related Functionality
        public void UpdateObjectivesCleared(ScenarioObjective _objective)
        {
            m_sitesClearedDisplay.text = $"{ScenarioManager.Instance.CompletedObjectiveCount}/{ScenarioManager.Instance.ObjectiveCount}";
        }

        public void UpdateSitesCleared()
        {
            m_sitesClearedDisplay.text = $"{ScenarioManager.Instance.ClearedSiteCount}/{ScenarioManager.Instance.ContaminantSiteCount}";
        }

        public void UpdateRadSitesCleared()
        {
            m_sitesClearedDisplay.text = $"{RadScenarioTest.Instance.ClearedSiteCount}/{RadScenarioTest.Instance.RadSiteCount}";
        }
        #endregion

        private void OnDestroy()
        {
            if (ScenarioManager.Instance && ScenarioManager.Instance.CurrentScenarioInstance != null)
            {
                // Unhook event
                ScenarioManager.Instance.OnObjectiveCompleted.RemoveListener(UpdateObjectivesCleared);
                //ScenarioManager.Instance.OnSiteCleared -= (i, j) => UpdateSitesCleared();
            }
            else if (RadScenarioTest.Instance)
            {
                // Unhook event
                RadScenarioTest.Instance.OnSiteCleared -= i => UpdateRadSitesCleared();
            }
        }
    }
}

