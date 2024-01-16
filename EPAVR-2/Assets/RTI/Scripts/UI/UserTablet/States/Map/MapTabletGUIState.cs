using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class MapTabletGUIState : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [SerializeField] private MapUI m_mapUI;
        [SerializeField] private Image m_mapImage;
        [SerializeField] private RawImage m_cameraMapImage;
        [SerializeField] private Button m_markSiteButton;
        #endregion
        #region Private Variables
        private bool m_isDynamicMapMode = false;
        #endregion

        #region Initialization
        public override void Init()
        {
            if (MapManager.Instance && ScenarioManager.Instance)
            {
                m_isDynamicMapMode = MapManager.Instance.MapCameraEnabled;

                //if (!m_isDynamicMapMode) m_mapImage.sprite = ScenarioManager.Instance.CurrentScenario.MapImage;
                if (!m_isDynamicMapMode) m_mapUI.Init(ScenarioManager.Instance.CurrentScenario.MapImage);
                SetMapImageMode(m_isDynamicMapMode);
            }
            // Check gamemode
            if (CoreGameManager.Instance && CoreGameManager.Instance.CurrentGamemode == Gamemode.RadiationSurvey)
            {
                // Do the thing
            }
            else
            {
                m_markSiteButton.gameObject.SetActive(false);
            }
            // Call base functionality
            base.Init();
        }
        #endregion


        #region Update Functionality
        public override void OnUpdate()
        {
            // Call base functionality
            base.OnUpdate();

            // Update map UI
            m_mapUI.OnUpdate();
        }
        #endregion

        #region Exit Functionality
        public override void OnExit()
        {
            // Check map UI type
            if (m_mapUI is RadMapUI radMap)
            {
                if (radMap.MarkerModeActive) radMap.SetMarkerModeActive(false);
            }
            base.OnExit();
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            // Do the thing
        }

        public void SetMapImageMode(bool _isDynamic)
        {
            if (_isDynamic)
            {
                m_cameraMapImage.gameObject.SetActive(true);
                m_mapImage.gameObject.SetActive(false);
            }
            else
            {
                m_cameraMapImage.gameObject.SetActive(false);
                m_mapImage.gameObject.SetActive(true);
            }
        }
    }
}

