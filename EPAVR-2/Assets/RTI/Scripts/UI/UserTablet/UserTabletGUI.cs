using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace L58.EPAVR
{
    public class UserTabletGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private List<DeviceGUIState> m_stateObjects;
        [SerializeField] private RectTransform m_sampleReportContainer;
        [SerializeField] private UserSampleReport m_sampleReportPrefab;
        [SerializeField] private TutorialVideoPlayer m_tutorialVideoPlayer;
        [SerializeField] private TextMeshProUGUI m_gamemodeDisplay;
        [Header("Default Configuration")]
        [SerializeField] private int m_defaultState = 0;
        [SerializeField] private VideoClip m_defaultTutorialVideo;
        #endregion
        #region Private Variables
        private UserTablet m_parent;
        public DeviceGUIState m_currentStateObject;

        private List<UserSampleReport> m_sampleReports;
        public UserSampleReport m_sampleReportInProgress;

        private int m_reportCount = 0;
        #endregion
        #region Public Properties
        public UserTablet Parent { get => m_parent; }
        #endregion

        #region Initialization
        public void Init(UserTablet _parent)
        {
            // Cache references
            m_parent = _parent;
            // Initialize lists
            m_sampleReports = new List<UserSampleReport>();
            // Initialize individual states
            //m_tutorialVideoPlayer.Init();
            // Hook up events
            //if (VRUserManager.Instance) VRUserManager.Instance.OnDeliverSample += i => CreateSampleReport(i);
            if (OffsiteLab.Instance)
            {
                OffsiteLab.Instance.OnAddRequest.AddListener(CreateSampleRequest);
                OffsiteLab.Instance.OnAnalysisStart.AddListener(SetSampleReportInProgress);
                OffsiteLab.Instance.OnAnalysisTimerTick.AddListener(TickReportAnalysisProgress);
                OffsiteLab.Instance.OnAnalysisComplete.AddListener(CompleteReport);
            }
            // Disable all states
            for(int i = 0; i < m_stateObjects.Count; i++)
            {
                m_stateObjects[i].Init();
                m_stateObjects[i].gameObject.SetActive(false);
            }
            // Set the default state
            SetState(m_defaultState);
            // Set gamemode display
            if (CoreGameManager.Instance != null)
            {
                m_gamemodeDisplay.text = (CoreGameManager.Instance.CurrentGamemode == Gamemode.RadiationSurvey) ? "Radiation Survey" : "Chemical Hunt";
                //m_gamemodeDisplay.text =  CoreGameManager.Instance.CurrentGamemode.ToString();
            }
            //m_tutorialVideoPlayer.SetVideoClip(m_defaultTutorialVideo);
        }
        #endregion

        #region Update
        public void OnUpdate()
        {
            if (m_currentStateObject) m_currentStateObject.OnUpdate();
        }
        #endregion

        #region State-Related Functionality
        public virtual void SetState(int _stateIndex)
        {
            if (m_currentStateObject != null) 
            {
                m_currentStateObject.OnExit();
            }

            if (_stateIndex < 0 || _stateIndex > m_stateObjects.Count)
            {
                m_currentStateObject = m_stateObjects[0];
            }
            else
            {
                m_currentStateObject = m_stateObjects[_stateIndex];
                //m_currentStateObject = m_stateObjects[((int)_mode.Type) + 1];
            }
            m_currentStateObject.OnEnter();
        }
        #endregion

        #region Sample-Related Functionality
        public void CreateSampleRequest(SampleRequest _request)
        {
            UserSampleReport report = Instantiate(m_sampleReportPrefab, m_sampleReportContainer);
            report.Init(_request, $"WipeSample_{m_reportCount}", false);
            m_sampleReports.Add(report);
            m_reportCount++;
        }

        public void CreateSampleReport(Sample _sample, bool _analyzed = true)
        {
            UserSampleReport report = Instantiate(m_sampleReportPrefab, m_sampleReportContainer);
            report.Init(_sample, $"WipeSample_{m_reportCount}", _analyzed);
            m_sampleReports.Add(report);
            m_reportCount++;
        }

        public void CompleteReport(SampleRequest _request)
        {
            // Find any sample report with a matching sample
            UserSampleReport report = GetReport(_request.Sample);
            if (report != null) report.SetAnalyzed(true);
            if (m_sampleReportInProgress != null && report == m_sampleReportInProgress) m_sampleReportInProgress = null;
        }

        public void SetSampleReportInProgress(SampleRequest _request)
        {
            // Try to get a sample report from this
            UserSampleReport report = GetReport(_request.Sample);
            if (report != null) m_sampleReportInProgress = report;
        }

        public UserSampleReport GetReport(Sample _sample)
        {
            if (m_sampleReports == null || m_sampleReports.Count < 1) return null;

            foreach(UserSampleReport report in m_sampleReports)
            {
                if (report.Sample == _sample) return report;
            }
            return null;
        }

        public void TickReportAnalysisProgress(float _value)
        {
            if (m_sampleReportInProgress == null) return;
            m_sampleReportInProgress.TickProgress(_value);
        }
        #endregion

        #region Additional Events
        public void ExitScenario()
        {
            if (!ScenarioManager.Instance) return;
            // Attempt to load the main menu scene
            ScenarioManager.Instance.EndScenario();
        }
        #endregion

        private void OnDestroy()
        {
            if (OffsiteLab.Instance)
            {
                OffsiteLab.Instance.OnAddRequest.RemoveListener(CreateSampleRequest);
                OffsiteLab.Instance.OnAnalysisStart.RemoveListener(SetSampleReportInProgress);
                OffsiteLab.Instance.OnAnalysisTimerTick.RemoveListener(TickReportAnalysisProgress);
                OffsiteLab.Instance.OnAnalysisComplete.RemoveListener(CompleteReport);
            }
        }
    }
}

