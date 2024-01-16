using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class CumulativeDoseTracker : RadSensor
    {
        #region Inspector Assigned Variables
        [Header("Additional References")]
        [SerializeField] private RadWatchGUI m_doseDisplayGUI;
        [Header("Additional Events")]
        [SerializeField] private UnityEvent<float> m_onCumulativeDoseUpdated;
        [Header("Misc. Configuration")]
        [SerializeField] private float m_hazardLevelHapticStrength = 0.75f;
        [SerializeField] private float m_hazardLevelHapticDuration = 0.25f;
        #endregion
        #region Private Variables
        private float m_cumulativeDose;
        private float m_maxCumulativeDose;

        private float m_unsafeHazardThreshold;
        private float m_dangerHazardThreshold;

        public RadHazardLevel m_hazardLevel;
        #endregion
        #region Public Properties
        public float CumulativeDose
        {
            get => m_cumulativeDose;
            private set
            {
                m_cumulativeDose = value;
                m_onCumulativeDoseUpdated?.Invoke(value);
            }
        }

        public RadHazardLevel HazardLevel
        {
            get => m_hazardLevel;
            set
            {
                m_hazardLevel = value;

            }
        }

        public float MaxCumulativeDose
        {
            get => m_maxCumulativeDose;
            set 
            {
                m_maxCumulativeDose = value;

                m_unsafeHazardThreshold = m_maxCumulativeDose * 0.5f;
                m_dangerHazardThreshold = m_maxCumulativeDose * 0.75f;
            } 
        }
        public UnityEvent<float> OnCumulativeDoseUpdated { get => m_onCumulativeDoseUpdated; }
        #endregion

        #region Initialization
        /*
        protected override void Start()
        {
            if (m_doseDisplayGUI) m_doseDisplayGUI.Init(this);
            MaxCumulativeDose = 5;
            HazardLevel = RadHazardLevel.Safe;
        }
        */
        public override void Init()
        {
            base.Init();
            if (m_doseDisplayGUI) m_doseDisplayGUI.Init(this);
            //m_maxCumulativeDose = 5;
            SetActive(true);
        }
        #endregion

        #region Update

        protected override void UpdateRadLevel(float _value)
        {
            base.UpdateRadLevel(_value);
            UpdateCumulativeDose(m_currentLevel);
        }
        public void UpdateCumulativeDose(float _currentRadPerHour)
        {
            // Convert mR/h to R/s
            //float deltaRad = _currentRadPerHour * (m_tickInterval / 3600) * Time.deltaTime;
            // Convert R/hr to R/s
            float deltaRad = (_currentRadPerHour / 3600.0f) * Time.deltaTime;
            CumulativeDose += deltaRad;

            // Check hazard level
            CheckHazardLevel();
        }


        public void CheckHazardLevel()
        {
            // Compare hazard level
            if (m_cumulativeDose < m_unsafeHazardThreshold)
            {
                UpdateHazardLevel(RadHazardLevel.Safe);
            }
            else if (m_cumulativeDose >= m_unsafeHazardThreshold && m_cumulativeDose < m_dangerHazardThreshold)
            {
                UpdateHazardLevel(RadHazardLevel.Unsafe);
            }
            else
            {
                UpdateHazardLevel(RadHazardLevel.Danger);
            }
        }

        void UpdateHazardLevel(RadHazardLevel _hazardLevel)
        {
            if (m_hazardLevel == _hazardLevel) return;
            HazardLevel = _hazardLevel;

            if (VRUserManager.Instance && VRUserManager.Instance.Avatar != null) VRUserManager.Instance.Avatar.SendHapticImpulse(true, m_hazardLevelHapticStrength, m_hazardLevelHapticDuration);
        }
        #endregion


        private void OnDestroy()
        {
            // Destroy the rad watch if necessary
            if (m_doseDisplayGUI) Destroy(m_doseDisplayGUI.gameObject);
        }
    }

    public enum RadHazardLevel {Safe, Unsafe, Danger}
}

