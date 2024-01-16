using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class RadScenarioDetailDisplay : EndScenarioDetailsDisplay
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private DoubleTextDisplay m_recordingAccuracyScoreDisplay;
        [SerializeField] private DoubleTextDisplay m_recordedMaxReadingDisplay;
        [SerializeField] private DoubleTextDisplay m_actualMaxReadingDisplay;

        [SerializeField] private DoubleTextDisplay m_epicenterMarkAccuracyScoreDisplay;
        [SerializeField] private DoubleTextDisplay m_epicenterEstimateDistanceDisplay;

        [SerializeField] private DoubleTextDisplay m_cumulativeDoseRatingDisplay;
        [SerializeField] private DoubleTextDisplay m_cumulativeDoseReadingDisplay;
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.RadiationSurvey;
        #endregion

        #region Initialization
        public override void Init(EndScenarioInfo _scenarioInfo)
        {
            base.Init(_scenarioInfo);
            EndRadSurveyScenarioInfo radScenarioInfo = (EndRadSurveyScenarioInfo)_scenarioInfo;
            // Set the reading display values
            m_recordingAccuracyScoreDisplay.SetSecondaryValue($"{ScoreManager.Instance.GetScoreValue(ScoreType.EstimatedSpread)}/{ScoreManager.Instance.MaxSiteClearScoreValue}");
            m_recordedMaxReadingDisplay.SetSecondaryValue($"{radScenarioInfo.MaxRecordedRadLevel.ToString("0.00")} R/hr");
            m_actualMaxReadingDisplay.SetSecondaryValue($"{radScenarioInfo.ActualMaxRadLevel.ToString("0.00")} R/hr");
            // Set the epicenter distance display values
            m_epicenterMarkAccuracyScoreDisplay.SetSecondaryValue($"{ScoreManager.Instance.GetScoreValue(ScoreType.EstimatedEpicenter)}/200");
            string epicenterDistanceText = (radScenarioInfo.EpicenterEstimateMarked) ? $"{radScenarioInfo.EpicenterDistance.ToString($"0.000")} m" : "N/A";
            m_epicenterEstimateDistanceDisplay.SetSecondaryValue(epicenterDistanceText);
            // Set the cumulative dose reading display values
            m_cumulativeDoseRatingDisplay.SetSecondaryValue($"{radScenarioInfo.DoseHazardLevel}");
            m_cumulativeDoseReadingDisplay.SetSecondaryValue($"{radScenarioInfo.CumulativeDoseReading.ToString("0.00")}/{radScenarioInfo.MaxCumulativeDoseReading.ToString("0.00")} R");
        }
        #endregion
    }
}

