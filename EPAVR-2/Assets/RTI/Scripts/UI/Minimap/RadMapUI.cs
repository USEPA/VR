using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class RadMapUI : MapUI
    {
        #region Inspector Assigned Variables
        [SerializeField] private Button m_markSiteButton;
        [SerializeField] private MarkRadSiteHandler m_radSiteMarkerUtility;
        [SerializeField] private MapMarkerUIObject m_radSiteMarkerPrefab;
        [SerializeField] private RadSpreadMapMarkerUIObject m_radSpreadMarkerPrefab;
        [SerializeField] private GameObject m_markSiteInstructionsContainer;
        #endregion
        #region Private Variables
        private bool m_markerModeActive = false;
        private RadSpreadMapMarkerUIObject m_radSpreadMarker;
        private MapMarkerUIObject m_radSiteMarker;
        private RectTransform m_measurementMarkerContainer;

        private RadSurveyScenarioInstance m_radScenarioInstance;
        #endregion
        #region Public Properties
        public bool MarkerModeActive
        {
            get => m_markerModeActive;
            set
            {
                m_markerModeActive = value;
                m_radSiteMarkerUtility.enabled = value;
                m_markSiteButton.gameObject.SetActive(!m_markerModeActive);
                m_markSiteInstructionsContainer.SetActive(m_markerModeActive);
                //m_markSiteButton.interactable = !m_markerModeActive;
            }
        }

        public RectTransform MeasurementMarkerContainer
        {
            get
            {
                if (!m_measurementMarkerContainer)
                {
                    m_measurementMarkerContainer = new GameObject("RadMeasurements", typeof(RectTransform)).GetComponent<RectTransform>();
                    m_measurementMarkerContainer.parent = m_staticMarkerContainer;
                    UIHelper.ForceExpandRectTransform(m_measurementMarkerContainer);
                }
                return m_measurementMarkerContainer;
            }
        }
        #endregion

        #region Initialization
        public override void Init(Sprite _image)
        {
            // Call base functionality
            base.Init(_image);
            // Get rad survey scenario instance
            if (ScenarioManager.Instance && ScenarioManager.Instance.CurrentScenarioInstance != null && ScenarioManager.Instance.CurrentScenarioInstance is RadSurveyScenarioInstance radScenarioInstance)
            {
                // Cache reference
                m_radScenarioInstance = radScenarioInstance;


                // Try to get the rad cloud
                if (m_radScenarioInstance.MaxRadSite.Cloud != null)
                {
                    m_radSpreadMarker = (RadSpreadMapMarkerUIObject) CreateMarker(m_radSpreadMarkerPrefab, m_radScenarioInstance.MaxRadSite.Cloud.transform.position);
                    m_radSpreadMarker.LoadRadCloudInfo(m_radScenarioInstance.MaxRadSite.Cloud);

                    m_radSpreadMarker.gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Measurement-Related Functionality
        public override MapMarkerUIObject AddMeasurementMarker(Vector3 _position, float _score)
        {
            MapMarkerUIObject measurementMarker = base.AddMeasurementMarker(_position, _score);
            measurementMarker.transform.parent = MeasurementMarkerContainer;
            // Tell the scenario instance to instantiate a world position marker for this measurement
            if (m_radScenarioInstance != null) m_radScenarioInstance.CreateWorldMeasurementMarker(_position, _score);
            return measurementMarker;
        }
        #endregion

        #region Site Marker-Related Functionality
        public void SetMarkerModeActive(bool _value)
        {
            MarkerModeActive = _value;
        }

        public void MarkSitePosition(Vector2 _imagePosition)
        {
            // Convert image position to world position
            Vector3 siteWorldPosition = MapManager.Instance.GetImageToWorldPosition(_imagePosition, RectTransform);

            // Create the rad site marker
            if (!m_radSiteMarker)
            {
                m_radSiteMarker = CreateMarker(m_radSiteMarkerPrefab, siteWorldPosition, true);
                //m_radSiteMarker = Instantiate(m_radSiteMarkerPrefab, m_staticMarkerContainer);
                //m_radSiteMarker.Init(this, siteWorldPosition);
            }
            else
            {
                m_radSiteMarker.UpdatePosition(siteWorldPosition);
            }

            // Update epicenter estimate on scenario instance
            m_radScenarioInstance.MarkEpicenterEstimate(siteWorldPosition);

            if (MarkerModeActive) SetMarkerModeActive(false);
        }
        #endregion
    }
}

