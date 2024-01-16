using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Gamemodes/Radiation Survey")]
    public class RadiationSurveyGMConfigAsset : GamemodeConfigAsset
    {
        #region Inspector Assigned Variables
        [Header("Rad Survey Prefab References")]
        [SerializeField] private CumulativeDoseTracker m_cumulativeDoseTrackerPrefab;
        [SerializeField] private GameObject m_epicenterEstimateWorldPrefab;
        [SerializeField] private RadMeasurementMarker m_measurementMarkerWorldPrefab;
        [Header("Default Settings")]
        [SerializeField] private bool m_useWorldSpaceMeasurementMarkers = true;
        [SerializeField] private int m_maxRecordings = 50;
        [SerializeField] private float m_flukeMaxMeasurement = 50.0f;
        [SerializeField] private float m_maxCumulativeDose = 10.0f;
        [SerializeField] private float m_measurementMaxErrorMargin = 2.0f;
        [SerializeField] private float m_epicenterEstimateErrorMargin = 2.0f;
        [SerializeField] private Vector2 m_easyDifficultyRadiationRange = new Vector2(5.0f, 15.0f);
        [SerializeField] private Vector2 m_mediumDifficultyRadiationRange = new Vector2(15.0f, 30.0f);
        [SerializeField] private Vector2 m_hardDifficultyRadiationRange = new Vector2(40.0f, 70.0f);
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.RadiationSurvey;

        public CumulativeDoseTracker CumulativeDoseTrackerPrefab { get => m_cumulativeDoseTrackerPrefab; }
        public GameObject EpicenterEstimateWorldPrefab { get => m_epicenterEstimateWorldPrefab; }
        public RadMeasurementMarker MeasurementMarkerWorldPrefab { get => m_measurementMarkerWorldPrefab; }

        public bool UseWorldSpaceMeasurementMarkers { get => m_useWorldSpaceMeasurementMarkers; }
        
        public int MaxRecordings { get => m_maxRecordings; }

        public float FlukeMaxMeasurement { get => m_flukeMaxMeasurement; }
        public float MaxCumulativeDose { get => m_maxCumulativeDose; }

        public float MeasurementMaxErrorMargin { get => m_measurementMaxErrorMargin; }
        public float EpicenterEstimateErrorMargin { get => m_epicenterEstimateErrorMargin; }
        #endregion


        public Vector2 GetRadiationRangeFromDifficulty(Difficulty _difficulty)
        {
            switch (_difficulty)
            {
                case Difficulty.Hard:
                    return m_hardDifficultyRadiationRange;
                case Difficulty.Medium:
                    return m_mediumDifficultyRadiationRange;
                default:
                    return m_easyDifficultyRadiationRange;
            }
        }
    }
}

