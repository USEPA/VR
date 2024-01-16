using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class EndScenarioDetailsDisplayInjector : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected EndScenarioMapUI m_mapUI;
        [SerializeField] List<EndScenarioDetailsDisplay> m_scenarioDetailDisplays;
        #endregion
        #region Protected Variables
        protected EndScenarioInfo m_scenarioInfo;
        protected EndScenarioDetailsDisplay m_scenarioDetails;
        #endregion

        #region Initialization
        public void LoadScenarioInfo(EndScenarioInfo _scenarioInfo)
        {
            // Update the map
            m_mapUI.Init(_scenarioInfo.Scenario.MapImage, _scenarioInfo);
            //m_mapImageContainer.sprite = _scenarioInfo.Scenario.MapImage;
            // Get the proper scenario detail display
            foreach(EndScenarioDetailsDisplay detailDisplay in m_scenarioDetailDisplays)
            {
                // Disable the display by default
                detailDisplay.gameObject.SetActive(false);
                // Check if this is for the appropriate gamemode
                if (detailDisplay.Mode == _scenarioInfo.Mode)
                {
                    m_scenarioDetails = detailDisplay;
                    break;
                }
            }

            if (m_scenarioDetails != null)
            {
                // Enable and load scenario information
                m_scenarioDetails.gameObject.SetActive(true);
                m_scenarioDetails.Init(_scenarioInfo);
                /*
                if (_scenarioInfo.MapMarkerContainer != null)
                {
                    _scenarioInfo.MapMarkerContainer.parent = m_mapUI.Image.GetComponent<RectTransform>();
                    _scenarioInfo.MapMarkerContainer.anchorMin = new Vector2(0, 0);
                    _scenarioInfo.MapMarkerContainer.anchorMax = new Vector2(1, 1);
                    _scenarioInfo.MapMarkerContainer.localPosition = Vector3.zero;
                    _scenarioInfo.MapMarkerContainer.localEulerAngles = Vector3.zero;
                    _scenarioInfo.MapMarkerContainer.localScale = Vector3.one;
                }
                */
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

