using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class EndRadSurveyScenarioInfo : EndScenarioInfo
    {
        #region Protected Variables
        protected float m_maxRecordedRadLevel;
        protected float m_actualMaxRadLevel;

        protected float m_epicenterDistance;

        protected float m_cumulativeDoseReading;
        protected float m_maxCumulativeDoseReading;
        protected RadHazardLevel m_doseHazardLevel;

        protected bool m_epicenterEstimateMarked = false;
        protected RadSpreadMapMarkerUIObject m_radSpreadMarker;
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.RadiationSurvey;

        public bool EpicenterEstimateMarked { get => m_epicenterEstimateMarked; }

        public float MaxRecordedRadLevel { get => m_maxRecordedRadLevel; }
        public float ActualMaxRadLevel { get => m_actualMaxRadLevel; }

        public float EpicenterDistance { get => m_epicenterDistance; }

        public float CumulativeDoseReading { get => m_cumulativeDoseReading; }
        public float MaxCumulativeDoseReading { get => m_maxCumulativeDoseReading; }
        public RadHazardLevel DoseHazardLevel { get => m_doseHazardLevel; }

        public RadSpreadMapMarkerUIObject RadSpreadMarker { get => m_radSpreadMarker; }
        #endregion

        public override void LoadInstanceInfo(ScenarioInstance _instance)
        {
            base.LoadInstanceInfo(_instance);

            RadSurveyScenarioInstance radInstance = (RadSurveyScenarioInstance)_instance;
            // Load measurements info
            m_maxRecordedRadLevel = radInstance.MaxValue;
            m_actualMaxRadLevel = radInstance.ActualMaxValue;
            // Load epicenter info
            m_epicenterEstimateMarked = radInstance.IsEpicenterMarkerPlaced;
            m_epicenterDistance = radInstance.BlastEpicenterEstimateToActualDistance;
            // Load cumulative dose info
            m_cumulativeDoseReading = radInstance.DoseTracker.CumulativeDose;
            m_maxCumulativeDoseReading = radInstance.MaxCumulativeDoseReading;
            m_doseHazardLevel = radInstance.DoseTracker.HazardLevel;

            // Check for rad spread marker
            if (m_mapMarkerContainer != null)
            {
                RadSpreadMapMarkerUIObject spreadMarker = m_mapMarkerContainer.GetComponentInChildren<RadSpreadMapMarkerUIObject>(true);
                if (spreadMarker != null) m_radSpreadMarker = spreadMarker;
            }
        }
    }
}

