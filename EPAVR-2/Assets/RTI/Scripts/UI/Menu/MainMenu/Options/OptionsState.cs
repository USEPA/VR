using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class OptionsState : MenuState
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private PlayerOptionsUtility m_optionsUtility;
        [Header("UI References")]
        [SerializeField] private ConfigSlider m_heightConfig;
        [SerializeField] private Button m_calibrateHeightButton;
        [SerializeField] private TextMeshProUGUI m_sitStandModeDisplay;
        [SerializeField] private TextMeshProUGUI m_trackModeDisplay;
        [SerializeField] private TextMeshProUGUI m_rigScaleDisplay;
        [SerializeField] private TextMeshProUGUI m_heightDisplay;
        [SerializeField] private TextMeshProUGUI m_cameraOffsetDisplay;
        #endregion
        #region Private Variables
        private bool m_initialized = false;

        private VRUserManager m_userManager;
        private PlayerController m_player;
        #endregion
        #region Public Properties
        public PlayerController Player 
        { 
            get 
            {
                if (!m_player && VRUserManager.Instance && VRUserManager.Instance.Player != null) m_player = VRUserManager.Instance.Player;
                return m_player;
            } 
        }
        #endregion

        #region Initialization
        void Init()
        {
            // Make sure conditions are valid
            if (!VRUserManager.Instance || VRUserManager.Instance.Player == null) return;
            // Cache references
            m_userManager = VRUserManager.Instance;
            m_player = VRUserManager.Instance.Player;
 
            /*
            UnityEngine.Debug.Log($"{gameObject.name} found current height before initialization: {m_player.GetHeightDisplay()} || Time: {Time.time}");
            //float priorYOffset = m_player.CameraYOffset;
            float priorYOffset = m_player.CalibratedHeight;
            // Initialize height config
            m_heightConfig.Init(m_userManager.MinCameraYOffset, m_userManager.MaxCameraYOffset, m_player.CalibratedHeight);
            m_heightConfig.OnValueChanged += i => m_player.CalibrateHeight(i);
            m_player.OnHeightCalibrated += i => m_heightConfig.ForceSetValue(i);
            // Initialize auto-calibration button
            //m_calibrateHeightButton.onClick.AddListener(m_player.CalibrateHeightFromFloorTrackMode);
            m_calibrateHeightButton.onClick.AddListener(m_player.CalibrateHeightFromOffset);
            m_heightConfig.ForceSetValue(priorYOffset);
            UnityEngine.Debug.Log($"{gameObject.name} found current height after initialization: {m_player.GetHeightDisplay()} || Time: {Time.time}");
            */
            m_initialized = true;
        }
        #endregion

        #region Enter/Exit-Related Functionality
        public override void OnStateEnter()
        {
            // Check if this has been initialized yet
            if (!m_optionsUtility.Initialized) m_optionsUtility.Init();
            // Update the sit/standing mode display
            //UpdateSitStandingModeDisplay();
            // Call base functionality
            base.OnStateEnter();
        }
        #endregion

        #region Update Functionality
        public override void OnStateUpdate(float _deltaTime)
        {
            // Call base functionality
            base.OnStateUpdate(_deltaTime);
            // Make sure player is cached
            if (!m_optionsUtility.Initialized || !Player) return;
            // Update camera offset display
            m_cameraOffsetDisplay.text = $"{Camera.main.transform.localPosition.y.ToString("0.00")} m";
            // Update rig scale display
            m_rigScaleDisplay.text = $"{Player.RigScale.ToString("0.000")}";
            // Update player height display
            m_heightDisplay.text = $"{Player.CalibratedHeight.ToString("0.00")}";
        }
        #endregion

        #region Helper Methods
        public void CalibrateHeight()
        {
            if (!Player) return;
            Player.CalibrateHeightFromOffset();
        }

        public void ToggleSitStandMode()
        {
            if (!Player) return;
            Player.ToggleSitStandMode();
            UpdateSitStandingModeDisplay();
        }

        public void ToggleTrackingMode()
        {
            if (!Player) return;
            Player.ToggleTrackingMode();
            UpdateTrackingModeDisplay();
        }

        public void ResetRigScale()
        {
            if (!Player) return;
            Player.ResetRigScale();
        }
        public void UpdateSitStandingModeDisplay()
        {
            if (!m_sitStandModeDisplay || !Player) return;
            m_sitStandModeDisplay.text = Player.SitStandMode.ToString();
        }

        public void UpdateTrackingModeDisplay()
        {
            if (!m_trackModeDisplay || !Player) return;
            m_trackModeDisplay.text = Player.TrackingMode.ToString();
        }
        #endregion
    }
}

