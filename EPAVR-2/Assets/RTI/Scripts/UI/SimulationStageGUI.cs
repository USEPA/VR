using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace L58.EPAVR
{
    public class SimulationStageGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Main UI References")]
        [SerializeField] TextMeshProUGUI m_stageTitle = default;
        [SerializeField] TextMeshProUGUI m_stepDisplay = default;
        [SerializeField] Button m_completeStepButton = default;
        [Header("Other UI References")]
        [SerializeField] TutorialVideoPlayer m_videoPlayer = default;
        [SerializeField] SampleReportGUI m_reportGUI;
        [SerializeField] Text m_buttonPrompt = default;
        [SerializeField] List<CanvasGroupFader> m_canvasFaders;
        #endregion
        #region Private Variables
        private int m_stepIndex = 0;
        #endregion
        #region Public Properties
        public TutorialVideoPlayer VideoPlayer { get => m_videoPlayer; }
        public SampleReportGUI ReportGUI { get => m_reportGUI; }
        public Button CompleteStepButton { get => m_completeStepButton; }
        public Text ButtonPrompt { get => m_buttonPrompt; }
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize the video player
            m_videoPlayer.Init();
        }
        #endregion
        #region Update GUI Functionality
        public void UpdateStageGUI(ScenarioAsset _stage)
        {
            // Initialize values
            m_stepIndex = 0;
            // Set stage title display
            m_stageTitle.text = _stage.Title;
        }
        public void UpdateStepGUI(ScenarioStep _step)
        {
            // Update step index
            m_stepIndex++;
            // Set stage step display
            m_stepDisplay.text = $"{m_stepIndex}. {_step.Title}";
            if (_step.Video != null) m_videoPlayer.SetVideoClip(_step.Video);
            m_buttonPrompt.text = (_step.EnableSampleArea) ? "Submit" : "Continue";
        }

        public void UpdateReport(SampleReportOld _report)
        {
            m_reportGUI.gameObject.SetActive(true);
            m_reportGUI.UpdateReport(_report);
        }

        public void ClearVideo(ScenarioAsset _stage)
        {
            m_videoPlayer.ClearRenderTexture();
        }
        #endregion
        #region Effect Functionality
        public void Fade(bool value)
        {
            foreach(CanvasGroupFader fader in m_canvasFaders)
            {
                if (fader.gameObject.activeInHierarchy)
                    fader.Fade(value, 0.15f);
            }
        }
        #endregion
    }
}

