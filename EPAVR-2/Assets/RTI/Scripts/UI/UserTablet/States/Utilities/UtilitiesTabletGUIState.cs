using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class UtilitiesTabletGUIState : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private PlayerOptionsUtility m_optionsUtility;
        [SerializeField] private ConfigSlider m_heightConfig;
        [SerializeField] private Button m_calibrateHeightButton;
        #endregion
        #region Private Variables
        private VRUserManager m_userManager;
        private PlayerController m_player;
        #endregion
        #region Public Properties

        #endregion

        #region Initialization
        public override void Init()
        {
            // Initialize the options utility
            if (!m_optionsUtility.Initialized) 
            {
                m_optionsUtility.Init();
                m_optionsUtility.UpdateSitStandingModeDisplay();
            }
 
            // Call base functionality
            base.Init();
            /*
            // Make sure conditions are valid
            if (!VRUserManager.Instance || VRUserManager.Instance.Player == null) return;
            // Cache references
            m_userManager = VRUserManager.Instance;
            m_player = VRUserManager.Instance.Player;
            float priorYOffset = m_player.CameraYOffset;
            // Initialize height config
            m_heightConfig.Init(m_userManager.MinCameraYOffset, m_userManager.MaxCameraYOffset, m_userManager.DefaultCameraYOffset);
            m_heightConfig.OnValueChanged += i => m_player.SetHeight(i);
            m_player.OnHeightCalibrated += i => m_heightConfig.ForceSetValue(i);
            // Initialize auto-calibration button
            m_calibrateHeightButton.onClick.AddListener(m_player.CalibrateHeightFromFloorTrackMode);
            m_heightConfig.ForceSetValue(priorYOffset);
            */
        }
        #endregion
    }
}

