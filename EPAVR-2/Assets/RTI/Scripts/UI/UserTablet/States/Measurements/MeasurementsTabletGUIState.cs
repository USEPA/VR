using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    public class MeasurementsTabletGUIState : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private RectTransform m_reportContainer;
        [SerializeField] private TextMeshProUGUI m_maxValueDisplay;
        [SerializeField] private TextMeshProUGUI m_minValueDisplay;
        [SerializeField] private TextMeshProUGUI m_rangeDisplay;
        [SerializeField] private UserMeasurementReport m_recordingPrefab;
        [SerializeField] private SampleTool m_sampleTool;
        [SerializeField] private RadTestZone m_testZone;
        [SerializeField] private RadTestMarker m_testMarker;
        [Header("Default Configuration")]
        [SerializeField] private int m_maxRecordings = 50;
        [SerializeField] private Gradient m_gradient;
        #endregion
        #region Private Variables
        private List<UserMeasurementReport> m_recordings;

        private float m_maxValue = 0.0f;
        private float m_minValue = float.MaxValue;

        private float m_estimatedRange = 0.0f;
        private RadSensor m_currentRadSensor;

        private RadSurveyScenarioInstance m_radScenarioInstance;
        #endregion

        #region Public Properties
        public float MaxValue
        {
            get => m_maxValue;
            private set
            {
                m_maxValue = value;
                UpdateRange();
                m_maxValueDisplay.text = $"Max: {m_maxValue.ToString("0.00")} R/h";
            }
        }


        public float MinValue
        {
            get => m_minValue;
            private set
            {
                m_minValue = value;
                UpdateRange();
                m_minValueDisplay.text = $"Min: {m_minValue.ToString("0.00")} R/h";
            }
        }

        public float EstimatedRange
        {
            get => m_estimatedRange;
            private set
            {
                m_estimatedRange = value;
                m_rangeDisplay.text = $"Estimated Range: {m_estimatedRange.ToString("0.00")} R/h";
            }
        }
        #endregion

        #region Initialization
        public override void Init()
        {
            base.Init();

            // Get rad survey scenario instance
            if (ScenarioManager.Instance && ScenarioManager.Instance.CurrentScenarioInstance != null && ScenarioManager.Instance.CurrentScenarioInstance is RadSurveyScenarioInstance radScenarioInstance)
            {
                // Cache reference
                m_radScenarioInstance = radScenarioInstance;
                // Cache information
                m_maxRecordings = m_radScenarioInstance.MaxRecordings;
                // Hook up event
                m_radScenarioInstance.OnMeasurementRecorded.AddListener(AddRecordingReport);
                m_radScenarioInstance.OnMinValueChange.AddListener(UpdateMinDisplay);
                m_radScenarioInstance.OnMaxValueChange.AddListener(UpdateMaxDisplay);
            }
            // Initialize recording list
            m_recordings = new List<UserMeasurementReport>();
        }
        #endregion

        #region Recording-Related Functionality
        public void AddRecording(float _time, float _value)
        {
            if (m_recordings == null) m_recordings = new List<UserMeasurementReport>();
            if (m_radScenarioInstance != null)
            {
                m_radScenarioInstance.AddRecording(_value, _time);
            }
            /*
            if (m_recordings == null) m_recordings = new List<UserMeasurementReport>();
            // Create a new measurement recording
            UserMeasurement recording = new UserMeasurement(_value, _time);
            UserMeasurementReport recordingDisplay = Instantiate(m_recordingPrefab.gameObject, m_reportContainer.transform).GetComponent<UserMeasurementReport>();
            recordingDisplay.Init(_time, _value);

            if (_value < m_minValue)
            {
                MinValue = _value;
            }
            else if (_value > m_maxValue)
            {
                MaxValue = _value;
            }
            
            m_recordings.Add(recordingDisplay);
            */
        }

        public void AddRecording()
        {
            if (!ScenarioManager.Instance || !VRUserManager.Instance.Player || !VRUserManager.Instance.CurrentTool || ((m_recordings.Count+1) > m_maxRecordings)) return;

            SampleTool tool = VRUserManager.Instance.CurrentTool;
            float value = Mathf.Clamp(tool.CurrentReading, 0.0f, tool.MaxReading);
            Vector3 position = tool.transform.position;
            if(m_currentRadSensor == null)
            {
                if (tool is Fluke fluke) m_currentRadSensor = fluke.Sensor;
            }

            if (m_currentRadSensor != null)
            {
                // Debug log values
                if (m_currentRadSensor.CurrentEvaluation != null && m_currentRadSensor.CurrentRadCloud != null)
                {
                    RadCloud cloud = m_currentRadSensor.CurrentRadCloud;
                    RadLevelEvaluation evaluation = m_currentRadSensor.CurrentEvaluation;
                    UnityEngine.Debug.Log($"Recorded Measurement Added for {tool.gameObject.name}: {value} | Position: {position} | Target Cloud: {cloud.Parent.gameObject.name} | Epicenter: {cloud.Epicenter} | Wind Direction: {cloud.WindDirection}\n" +
                        $"Distance: {evaluation.Distance} | Angle: {evaluation.Angle} | Angle Difference: {evaluation.AngleDifference} | Angle Difference Factor: {evaluation.AngleDifferenceFactor}\n" +
                        $"Dampening Factor: {evaluation.DampeningFactor} | Multiplier: {evaluation.Multiplier} | Rad Level: {evaluation.RadLevel} || Time: {Time.time}");
                }
            }
            AddRecording(ScenarioManager.Instance.GlobalScenarioTimer, value);
            if (MapUI.Instance) MapUI.Instance.AddMeasurementMarker(position, (value / tool.MaxReading));
        }

        public void AddTestRecording()
        {
            /*
            m_testZone.CalculateScore(m_testMarker);
            float value = m_testMarker.RadLevel;
            */
            float value = m_sampleTool.CurrentReading;

            Vector3 position = m_sampleTool.transform.position;

            AddRecording(Time.time, value);

            if (MapUI.Instance) MapUI.Instance.AddMeasurementMarker(position, (value / m_sampleTool.MaxReading));

        }

        public void AddRecordingReport(UserMeasurement _measurement)
        {
            if (m_recordings == null) m_recordings = new List<UserMeasurementReport>();
            // Instantiate the report display
            UserMeasurementReport recordingDisplay = Instantiate(m_recordingPrefab.gameObject, m_reportContainer.transform).GetComponent<UserMeasurementReport>();
            recordingDisplay.Init(_measurement);

            // Add this to the list
            m_recordings.Add(recordingDisplay);
        }
        #endregion

        #region Helper Methods
        private void UpdateRange()
        {
            if (m_recordings.Count < 1) return;
            EstimatedRange = m_maxValue - m_minValue;
        }

        public void UpdateMinDisplay(float _value)
        {
            MinValue = _value;
        }


        public void UpdateMaxDisplay(float _value)
        {
            MaxValue = _value;
        }
        #endregion
    }
}

