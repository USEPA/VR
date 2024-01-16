using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class RadWatchGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private TextMeshProUGUI m_doseDisplay;
        [SerializeField] private Image m_watchBackground;
        [Header("Default Configuration")]
        [SerializeField] private Gradient m_hazardGradient;
        #endregion
        #region Private Variables
        private CumulativeDoseTracker m_doseTracker;
        #endregion

        #region Initiialization
        public void Init(CumulativeDoseTracker _doseTracker)
        {
            // Cache reference
            m_doseTracker = _doseTracker;
            // Set up event
            //m_doseTracker.OnCumulativeDoseUpdated.AddListener(UpdateDisplay);
        }

        private void Start()
        {
            // Check if there is currently a player
            if (!VRUserManager.Instance || !VRUserManager.Instance.Player || !VRUserManager.Instance.Player.Avatar.WatchAttachPoint) return;

            transform.parent = VRUserManager.Instance.Player.Avatar.WatchAttachPoint;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
        #endregion
        #region Update-Related Functionality
        public void UpdateDisplay(float _value)
        {
            if (!m_doseDisplay) return;
            m_doseDisplay.text = $"{_value.ToString("0.00")}\nR";

            // Update the background gradient
            float threatLevel = (_value) / m_doseTracker.MaxCumulativeDose;
            UpdateBackgroundGradient(threatLevel);
        }

        public void UpdateRadHazardDisplay(RadHazardLevel _hazardLevel)
        {

        }

        public void UpdateBackgroundGradient(float _value)
        {
            if (!m_watchBackground) return;
            m_watchBackground.color = m_hazardGradient.Evaluate(_value);
        }
        #endregion

        private void OnDestroy()
        {
            //if (m_doseTracker) m_doseTracker.OnCumulativeDoseUpdated.RemoveListener(UpdateDisplay);
        }
    }
}

