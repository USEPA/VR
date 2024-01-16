using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class RadSurveyScenarioInstance : ScenarioInstance
    {
        #region Protected Variables
        protected CumulativeDoseTracker m_doseTracker;
        protected GameObject m_epicenterWorldEstimatePrefab;
        protected RadMeasurementMarker m_measurementMarkerWorldPrefab;

        protected List<UserMeasurement> m_recordings;

        protected RadiationSurveyGMConfigAsset m_radConfig;
        private int m_maxRecordings = 50;
        protected float m_maxCumulativeDose = 10.0f;
        private float m_flukeMaxMeasurement = 50.0f;

        protected Vector2 m_radRange = new Vector2(10.0f, 50.0f);
        protected float m_maxSiteRadiation = 0.0f;

        private float m_maxValue = 0.0f;
        private float m_minValue = float.MaxValue;

        private float m_estimatedRange = 0.0f;
        private float m_measurementMaxErrorMargin = 2.0f;
        private float m_epicenterEstimateErrorMargin = 2.0f;
        private bool m_isEpicenterMarkerPlaced = false;

        private GameObject m_epicenterEstimateWorldMarker;
        private RadSite m_maxRadSite;
        private Vector3 m_maxSiteEpicenter;
        private Vector3 m_blastEpicenterEstimate;
        private float m_blastEpicenterEstimateDistance;
        private float m_blastEpicenterEstimateToActualDistance;

        protected UnityEvent<UserMeasurement> m_onMeasurementRecorded;
        protected UnityEvent<float> m_onMinValueChange;
        protected UnityEvent<float> m_onMaxValueChange;
        protected UnityEvent<float> m_onEpicenterEstimateDistanceChanged;
        #endregion

        #region Public Properties
        public List<UserMeasurement> Recordings
        {
            get
            {
                if (m_recordings == null) m_recordings = new List<UserMeasurement>();
                return m_recordings;
            }
        }

        public CumulativeDoseTracker DoseTracker { get => m_doseTracker; }

        public RadSite MaxRadSite { get => m_maxRadSite; }

        public int MaxRecordings { get => m_maxRecordings; }

        public float MinValue
        {
            get => m_minValue;
            private set
            {
                m_minValue = value;
                m_onMinValueChange?.Invoke(m_minValue);
            }
        }

        public float MaxValue
        {
            get => m_maxValue;
            private set
            {
                m_maxValue = value;
                m_onMaxValueChange?.Invoke(m_maxValue);
            }
        }

        public float ActualMaxValue { get => m_maxSiteRadiation; }

        public Vector3 BlastEpicenterEstimate
        {
            get => m_blastEpicenterEstimate;
            set
            {
                if (!m_isEpicenterMarkerPlaced) m_isEpicenterMarkerPlaced = true;
                m_blastEpicenterEstimate = value;
            }
        }

        public bool IsEpicenterMarkerPlaced
        {
            get => m_isEpicenterMarkerPlaced;
        }

        public float BlastEpicenterEstimateDistance
        {
            get => m_blastEpicenterEstimateDistance;
            set 
            {
                m_blastEpicenterEstimateDistance = value;
                m_onEpicenterEstimateDistanceChanged?.Invoke(m_blastEpicenterEstimateDistance);
            }
        }

        public float BlastEpicenterEstimateToActualDistance { get => m_blastEpicenterEstimateToActualDistance; }

        public float MaxCumulativeDoseReading { get => m_maxCumulativeDose; }

        public UnityEvent<UserMeasurement> OnMeasurementRecorded
        {
            get
            {
                if (m_onMeasurementRecorded == null) m_onMeasurementRecorded = new UnityEvent<UserMeasurement>();
                return m_onMeasurementRecorded;
            }
            set => m_onMeasurementRecorded = value;
        }

        public UnityEvent<float> OnMinValueChange
        {
            get
            {
                if (m_onMinValueChange == null) m_onMinValueChange = new UnityEvent<float>();
                return m_onMinValueChange;
            }
        }

        public UnityEvent<float> OnMaxValueChange
        {
            get
            {
                if (m_onMaxValueChange == null) m_onMaxValueChange = new UnityEvent<float>();
                return m_onMaxValueChange;
            }
        }

        public UnityEvent<float> OnEpicenterEstimateDistanceChanged
        {
            get
            {
                if (m_onEpicenterEstimateDistanceChanged == null) m_onEpicenterEstimateDistanceChanged = new UnityEvent<float>();
                return m_onEpicenterEstimateDistanceChanged;
            }
        }
        #endregion

        #region Initialization
        public RadSurveyScenarioInstance(ScenarioAsset _scenario) : base(_scenario)
        {
            // Cache source reference
            m_scenario = _scenario;
        }

        public override void Init()
        {
            // Cache gamemode config
            if (!m_radConfig)
            {
                m_radConfig = (RadiationSurveyGMConfigAsset)CoreGameManager.Instance.CurrentGamemodeConfig;
                m_maxCumulativeDose = m_radConfig.MaxCumulativeDose;
                m_radRange = m_radConfig.GetRadiationRangeFromDifficulty(ScenarioManager.Instance.Difficulty);
            }
            // Initialize contaminant sites
            InitContaminantSites();
            // Call base functionality
            base.Init();

            if (m_radConfig)
            {
                // Spawn the cumulative dose tracker
                if (m_radConfig.CumulativeDoseTrackerPrefab != null)
                {
                    m_doseTracker = Object.Instantiate(m_radConfig.CumulativeDoseTrackerPrefab, VRUserManager.Instance.Player.XRRig.transform);
                    // Position the dose tracker at roughly 3/4 the player's height
                    float playerHeight = VRUserManager.Instance.Player.CharacterController.height;
                    Vector3 targetPosition = m_doseTracker.transform.localPosition;
                    targetPosition.y = playerHeight * 0.75f;
                    m_doseTracker.transform.localPosition = targetPosition;
                    // Initialize the dose tracker
                    m_doseTracker.MaxCumulativeDose = m_maxCumulativeDose;
                    m_doseTracker.Init();
                }


                if (m_radConfig.EpicenterEstimateWorldPrefab)
                {
                    m_epicenterWorldEstimatePrefab = m_radConfig.EpicenterEstimateWorldPrefab;
                }

                if (m_radConfig.UseWorldSpaceMeasurementMarkers && m_radConfig.MeasurementMarkerWorldPrefab)
                {
                    m_measurementMarkerWorldPrefab = m_radConfig.MeasurementMarkerWorldPrefab;
                }

                // Get default settings
                m_maxRecordings = m_radConfig.MaxRecordings;
                m_flukeMaxMeasurement = m_radConfig.FlukeMaxMeasurement;
                m_measurementMaxErrorMargin = m_radConfig.MeasurementMaxErrorMargin;
                m_epicenterEstimateErrorMargin = m_radConfig.EpicenterEstimateErrorMargin;
            }
    

            BlastEpicenterEstimateDistance = float.MaxValue;

            // Initialize recording list
            m_recordings = new List<UserMeasurement>();
            
        }

        public override void InitContaminantSites()
        {
            if (m_activeSites == null || m_activeSites.Count < 1) return;
            // Get difficulty variance
            bool spawnSmokeEffect = false;
            switch (ScenarioManager.Instance.Difficulty)
            {
                case Difficulty.Hard:
                    spawnSmokeEffect = false;
                    break;
                default:
                    spawnSmokeEffect = true;
                    break;
            }
            // Loop through each contaminant site and figure out which chemical to spawn
            foreach (RadSite site in m_activeSites)
            {
                // Get a random rad amount
                float radLevel = Random.Range(m_radRange.x, m_radRange.y);
                // Initialize the rad site
                site.InitRadCloud(radLevel, spawnSmokeEffect);

                // Check if this is max radiation
                if (site.Cloud.BaseRadAmount > m_maxSiteRadiation) 
                {
                    m_maxRadSite = site;
                    m_maxSiteRadiation = site.Cloud.BaseRadAmount;
                    m_maxSiteEpicenter = site.Cloud.Epicenter;
                    m_maxSiteEpicenter.y = 0.0f;
                }
                
            }
        }
        #endregion

        #region Update
        public override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);

            CheckCumulativeDoseReading();
            CheckBlastEstimateDistance();
        }

        private void CheckCumulativeDoseReading()
        {
            // Make sure the dose tracker exists
            if (!m_doseTracker) return;
            // Compare current cumulative dose to maximum threshold
            if (m_doseTracker.CumulativeDose > m_maxCumulativeDose)
            {
                if (VRUserManager.Instance && VRUserManager.Instance.Avatar != null) VRUserManager.Instance.Avatar.SendHapticImpulse(true, 1.0f, 0.75f);
                m_isActive = false;
                UnityEngine.Debug.Log($"SCENARIO FAILURE: Cumulative Dose reached above threshold: {m_maxCumulativeDose} || Time: {Time.time}");
                ScenarioManager.Instance.EndScenario();
            }
        }

        private void CheckBlastEstimateDistance()
        {
            if (m_blastEpicenterEstimate == null) return;
            // Get distance from the player
            BlastEpicenterEstimateDistance = GetDistanceFromEpicenterEstimate(VRUserManager.Instance.Avatar.transform.position);
        }
        #endregion

        #region Recording-Related Functionality
        public void AddRecording(UserMeasurement _measurement)
        {
            // Add the recording
            Recordings.Add(_measurement);

            m_onMeasurementRecorded?.Invoke(_measurement);


            float value = _measurement.Value;
            if (value < m_minValue)
            {
                MinValue = value;
            }
            else if (value > m_maxValue)
            {
                MaxValue = value;
            }
        }

        public void AddRecording(float _value, float _timeStamp)
        {
            // Add the recording
            AddRecording(new UserMeasurement(_value, _timeStamp));
        }

        public void CreateWorldMeasurementMarker(Vector3 _position, float _score)
        {
            if (!m_measurementMarkerWorldPrefab) return;
            // Get the closest world space position on the ground
            Vector3 targetPosition = GameObjectHelper.GetPointOnGround(_position);
            RadMeasurementMarker measurementMarker = GameObject.Instantiate(m_measurementMarkerWorldPrefab, targetPosition, Quaternion.identity).GetComponent<RadMeasurementMarker>();
            measurementMarker.Init(_score);
        }
        #endregion

        #region Epicenter-Related Functionality
        public void MarkEpicenterEstimate(Vector3 _estimatedEpicenterPoint)
        {
            BlastEpicenterEstimate = new Vector3(_estimatedEpicenterPoint.x, 0.0f, _estimatedEpicenterPoint.z);
            // Get rotation to point
            Quaternion lookAtPlayerRotation = Quaternion.identity;
            if (m_epicenterEstimateWorldMarker == null)
            {
                // Create the epicenter marker
                if (m_epicenterWorldEstimatePrefab != null) m_epicenterEstimateWorldMarker = Object.Instantiate(m_epicenterWorldEstimatePrefab, BlastEpicenterEstimate, Quaternion.LookRotation(VRUserManager.Instance.Avatar.transform.position - BlastEpicenterEstimate));
            }
            else
            {
                m_epicenterEstimateWorldMarker.transform.position = BlastEpicenterEstimate;
                m_epicenterEstimateWorldMarker.transform.rotation = Quaternion.LookRotation(VRUserManager.Instance.Avatar.transform.position - BlastEpicenterEstimate);
            }
            float epicenterEstimateDistance = MathHelper.QuickDistance(m_blastEpicenterEstimate, m_maxSiteEpicenter);
            UnityEngine.Debug.Log($"Marked Epicenter Estimate | Actual Distance: {epicenterEstimateDistance} || Time: {Time.time}");
        }

        public float GetDistanceFromEpicenterEstimate(Vector3 _sourcePosition)
        {
            if (m_blastEpicenterEstimate == null) return 0.0f;
            // Zero out the Y axis
            _sourcePosition.y = 0.0f;
            return MathHelper.QuickDistance(_sourcePosition, m_blastEpicenterEstimate);
        }
        #endregion

        #region Score-Related Functionality
        public override void CalculateScore()
        {
            if (!ScoreManager.Instance) return;

            // Calculate score based on user's recordings
            int spreadEstimateScore = 0;
            int epicenterEstimateScore = 0;
            int dosePenalty = 0;
            float rawMeasurementScore = 0.0f;
            float rawEpicenterEstimateScore = 0.0f;
            float epicenterEstimateDistance = 0.0f;

            if (m_recordings != null && m_recordings.Count >= 2)
            {
                float maxRadiationComparisonValue = (m_maxSiteRadiation > m_flukeMaxMeasurement) ? m_flukeMaxMeasurement : m_maxSiteRadiation;
                // Get how close the user was to the actual radiation spread
                rawMeasurementScore = (m_maxValue / maxRadiationComparisonValue);
                spreadEstimateScore = (int) (500 * rawMeasurementScore);
            }

            // Figure out estimated blast site position
            if (m_isEpicenterMarkerPlaced)
            {
                // Get the distance between the estimated blast site position and the actual one
                Vector3 blastPointEstimate = m_blastEpicenterEstimate;
                Vector3 blastPointActual = m_maxSiteEpicenter;

                epicenterEstimateDistance = MathHelper.QuickDistance(m_blastEpicenterEstimate, m_maxSiteEpicenter);
                m_blastEpicenterEstimateToActualDistance = epicenterEstimateDistance;
                float errorMargin = m_epicenterEstimateErrorMargin;

                rawEpicenterEstimateScore = Mathf.Clamp(errorMargin / epicenterEstimateDistance, 0.0f, 1.0f);
                epicenterEstimateScore = (int)(200 * rawEpicenterEstimateScore);
            }

            // Figure out cumulative dose penalty
            if (m_doseTracker && m_doseTracker.HazardLevel != RadHazardLevel.Safe)
            {
                dosePenalty = CalculateCumulativeDosePenalty();
                if (m_doseTracker.CumulativeDose > m_maxCumulativeDose)
                {
                    dosePenalty = 500;
                }
            }

            UnityEngine.Debug.Log($"{m_scenario.Scene} - Estimated Spread Score: {spreadEstimateScore} | Raw Measurement Score: {rawMeasurementScore} | Max Value: {m_maxValue} | Max Site Radiation: {m_maxSiteRadiation} || Time: {Time.time}");
            UnityEngine.Debug.Log($"{m_scenario.Scene} - Estimated Epicenter Score: {epicenterEstimateScore} | Distance: {epicenterEstimateDistance} | Raw Difference Score: {rawEpicenterEstimateScore} || Time: {Time.time}");
            // Add the score
            ScoreManager.Instance.AddScore(ScoreType.EstimatedSpread, spreadEstimateScore);
            ScoreManager.Instance.AddScore(ScoreType.EstimatedEpicenter, epicenterEstimateScore);
            if (dosePenalty > 0) ScoreManager.Instance.AddPenalty(PenaltyType.UnsafeCumulativeDose, dosePenalty);
            UnityEngine.Debug.Log($"{m_scenario.Scene} - Cumulative Dose Penalty: {dosePenalty} | Cumulative Dose: {m_doseTracker.CumulativeDose} || Time: {Time.time}");
        }

        protected int CalculateCumulativeDosePenalty()
        {
            return (m_doseTracker.HazardLevel == RadHazardLevel.Danger) ? 200 : 100;
        }

        public override EndScenarioInfo GenerateEndInfo()
        {
            EndRadSurveyScenarioInfo scenarioInfo = new EndRadSurveyScenarioInfo();
            scenarioInfo.LoadInstanceInfo(this);
            return scenarioInfo;
        }
        #endregion

        public override void OnExit()
        {
            // Call base functionality
            base.OnExit();
            // Calculate score
            CalculateScore();
            // Load end scenario info
            ScenarioManager.Instance.EndScenarioInfo = GenerateEndInfo();
            // Destroy the dose tracker
            if (m_doseTracker) 
            {
                Object.Destroy(m_doseTracker.gameObject);
                m_doseTracker = null;
            }

            m_onMeasurementRecorded.RemoveAllListeners();
            m_onMinValueChange.RemoveAllListeners();
            m_onMaxValueChange.RemoveAllListeners();
            m_onEpicenterEstimateDistanceChanged.RemoveAllListeners();
        }
    }
}

