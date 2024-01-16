using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class PlayerOptionsUtility : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_sitStandModeDisplay;
        [SerializeField] private TextMeshProUGUI m_debugMinimapSiteViewDisplay;
        [SerializeField] private Toggle m_paintSystemToggle;
        #endregion
        #region Private Variables
        private VRUserManager m_userManager;
        private PlayerController m_player;

        private bool m_initialized = false;
        #endregion
        #region Public Properties
        public bool Initialized { get => m_initialized; }
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
        public void Init()
        {
            // Make sure conditions are valid
            if (!VRUserManager.Instance || VRUserManager.Instance.Player == null) return;
            // Cache references
            m_userManager = VRUserManager.Instance;
            m_player = VRUserManager.Instance.Player;
            // Do default initialization stuff
            UpdateSitStandingModeDisplay();
            UpdateMinimapDebugSiteViewDisplay();
            if (m_paintSystemToggle && PaintManager.Instance) m_paintSystemToggle.isOn = PaintManager.Instance.Enabled;
            UnityEngine.Debug.Log($"{gameObject.name} finished initialization || Time: {Time.time}");
            // Set initialized
            m_initialized = true;
        }
        // Start is called before the first frame update
        void Start()
        {

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

        public void UpdateSitStandingModeDisplay()
        {
            if (!m_sitStandModeDisplay || !Player) return;
            m_sitStandModeDisplay.text = Player.SitStandMode.ToString();
        }

        public void RespawnTools()
        {
            if (!Player) return;
            Player.Inventory.RespawnEquipment();
        }

        public void UpdateMinimapDebugSiteViewDisplay()
        {
            if (!m_debugMinimapSiteViewDisplay || !DebugManager.Instance) return;
            string text = (DebugManager.Instance.ShowSitesOnMinimap) ? "Hide" : "Show";
            m_debugMinimapSiteViewDisplay.text = $"{text} Sites on Minimap";
        }

        public void ToggleMinimapDebugSiteView()
        {
            if (!DebugManager.Instance) return;
            DebugManager.Instance.ToggleMinimapDebugSiteVisibility();
            UpdateMinimapDebugSiteViewDisplay();
        }

        public void SetPaintSystemActive(bool _value)
        {
            if (!PaintManager.Instance) return;
            PaintManager.Instance.Enabled = _value;
        }
        #endregion
    }
}

