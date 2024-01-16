using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class EndScenarioMapUI : MapUI
    {
        #region Private Variables
        #endregion

        #region Initialization
        public void Init(Sprite _image, EndScenarioInfo _scenarioInfo)
        {
            // Call base initialization
            base.Init(_image);
            // Load scenario info
            LoadScenarioInfo(_scenarioInfo);
        }
        private void LoadScenarioInfo(EndScenarioInfo _scenarioInfo)
        {
            if (_scenarioInfo.MapMarkerContainer != null)
            {
                _scenarioInfo.MapMarkerContainer.parent = Image.GetComponent<RectTransform>();
                // Zero out any offsets and force the container to fit the map
                UIHelper.ForceExpandRectTransform(_scenarioInfo.MapMarkerContainer);
                // Cache static marker container
                m_staticMarkerContainer = _scenarioInfo.MapMarkerContainer;

                if (_scenarioInfo.Mode == Gamemode.ChemicalHunt)
                {
                    // Get chem hunt info
                    EndChemHuntScenarioInfo chemScenarioInfo = (EndChemHuntScenarioInfo)_scenarioInfo;
                    // Enable all chemical site markers
                    if (chemScenarioInfo.ChemCloudMapMarkers != null && chemScenarioInfo.ChemCloudMapMarkers.Count > 0)
                    {
                        foreach (ChemCloudMapMarkerUIObject siteMarker in chemScenarioInfo.ChemCloudMapMarkers)
                        {
                            siteMarker.gameObject.SetActive(true);
                            siteMarker.SetStatusViewActive(true);
                        }
                    }
                }
                else if (_scenarioInfo.Mode == Gamemode.RadiationSurvey)
                {
                    // Get rad survey info
                    EndRadSurveyScenarioInfo radScenarioInfo = (EndRadSurveyScenarioInfo)_scenarioInfo;
                    // Enable rad spread view
                    if (radScenarioInfo.RadSpreadMarker != null) radScenarioInfo.RadSpreadMarker.gameObject.SetActive(true);
                }
         
            }
        }

        protected override void CreateMarkers()
        {
            // Do nothing
        }
        #endregion
        public override void OnUpdate()
        {
            // Do nothing
        }
    }
}

