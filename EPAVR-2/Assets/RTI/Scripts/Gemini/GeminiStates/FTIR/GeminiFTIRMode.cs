using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class GeminiFTIRMode : GeminiState
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private BaseSampleContainer m_sampleContainer;
        [SerializeField] private FTIRLever m_lever;
        [SerializeField] private MeshRenderer m_sampleStatusDisplay;

        [Header("Configuration")]
        [SerializeField] float m_scanDelay = 15.0f;
        [SerializeField] float m_scanTime = 5.0f;
        [SerializeField] float m_analysisTime = 5.0f;

        [SerializeField] Gradient m_defaultSampleAreaGradient;
        [SerializeField] Gradient m_dirtySampleAreaGradient;
        [SerializeField] Gradient m_cleanSampleAreaGradient;
        #endregion
        #region Private Variables
        private DeviceGUIState m_guiState;

        private Sample m_currentSample;
        private FTIRState m_currentState = FTIRState.Idle;

        private int m_scanCount = 0;
        private float m_genericTimer;

        private Action<FTIRState> m_onStateChange;
        #endregion
        #region Public Properties
        public override GModeType Type => GModeType.FTIR;
        public override string Title => "FTIR Scan";

        public FTIRState CurrentState { get => m_currentState; }


        public Action<FTIRState> OnStateChange { get => m_onStateChange; set => m_onStateChange = value; }
        #endregion

        #region Initialization
        public override void Init(Gemini _parent)
        {
            // Call base functionality
            base.Init(_parent);

            // Cache GUI state reference
            m_guiState = _parent.GUI.Modes[Type];
            // Hook up events
            m_onStateChange += i => m_guiState.SetSubstate((int)i);
            //m_sampleContainer.OnClearSample += () => m_sampleStatusDisplay.colorGradient = m_cleanSampleAreaGradient;
            m_sampleContainer.OnClearSample += () => m_sampleStatusDisplay.material.SetColor("_EmissionColor", m_cleanSampleAreaGradient.Evaluate(0.0f));
            m_lever.OnSetLocked += i => m_guiState.Substates[1].Buttons[0].enabled = i;
            m_guiState.Substates[1].Buttons[0].onClick.AddListener(BeginScan);

            // Disable the status light by default
            m_sampleStatusDisplay.material.SetColor("_EmissionColor", m_defaultSampleAreaGradient.Evaluate(0.0f));
            //m_sampleStatusDisplay.enabled = false;

        }
        #endregion

        #region Mode-Related Functionality
        public override void OnEnter()
        {
            // Enable the status light
            //m_sampleStatusDisplay.enabled = true;
            // Set state depending on whether or not there is currently a sample
            if (m_sampleContainer.CurrentSample == null || (m_sampleContainer.CurrentSample.Analyzed == false))
            {
                // Set the default light
                //m_sampleStatusDisplay.colorGradient = m_cleanSampleAreaGradient;
                m_sampleStatusDisplay.material.SetColor("_EmissionColor", m_cleanSampleAreaGradient.Evaluate(0.0f));
                // Go to the scan preview screen
                SetState(FTIRState.Idle);
            }
            else
            {
                // Go to the clean prompt screen
                SetState(FTIRState.CleanPrompt);
            }
                
        }

        public override void OnUpdate()
        {
            // Check state
            switch (m_currentState)
            {
                case FTIRState.Countdown:
                    // Update countdown timer
                    m_guiState.CurrentSubstate.SetTextMesh(0, $"{Mathf.RoundToInt(m_genericTimer - Time.time)}");
                    if (Time.time > m_genericTimer) SetState(FTIRState.Scanning);
                    break;
                case FTIRState.Scanning:
                    // Wait for vial to finish initial scan
                    if (Time.time > m_genericTimer) StartAnalysis(m_sampleContainer.CurrentSample);
                    break;
                case FTIRState.Analyzing:
                    m_guiState.CurrentSubstate.SetSlider(0, (1.0f - ((m_genericTimer - Time.time) / m_analysisTime)));
                    if (Time.time > m_genericTimer) DisplayResults();
                    break;
                default:
                    break;
            }
        }

        public override void OnExit()
        {
            // Disable the status light
            //m_sampleStatusDisplay.enabled = false;
            m_sampleStatusDisplay.material.SetColor("_EmissionColor", m_defaultSampleAreaGradient.Evaluate(0.0f));
        }
        #endregion

        #region State-Related Functionality
        public void SetState(FTIRState _state)
        {
            // Check prior state
            switch (m_currentState)
            {
                case FTIRState.CleanPrompt:
                    m_sampleContainer.OnClearSample -= () => SetState(FTIRState.Idle);
                    break;
                case FTIRState.Countdown:
                    // Reset timer
                    m_genericTimer = -1.0f;
                    break;
                case FTIRState.Scanning:
                    // Disable laser pointer
                    //m_laserPointer.Active = false;
                    m_genericTimer = -1.0f;
                    break;
                case FTIRState.Analyzing:
                    // Reset timer
                    m_genericTimer = -1.0f;
                    break;
            }

            // Check new state
            switch (_state)
            {
                case FTIRState.CleanPrompt:
                    //m_sampleStatusDisplay.colorGradient = m_dirtySampleAreaGradient;
                    m_sampleStatusDisplay.material.SetColor("_EmissionColor", m_dirtySampleAreaGradient.Evaluate(0.0f));
                    m_sampleContainer.OnClearSample += () => SetState(FTIRState.Idle);
                    break;
                case FTIRState.Countdown:
                    // Set up timer
                    m_genericTimer = Time.time + m_scanDelay;
                    break;
                case FTIRState.Scanning:
                    // Iterate scan count
                    m_scanCount++;
                    // Start the timer
                    m_genericTimer = Time.time + m_scanTime;
                    break;
                case FTIRState.Analyzing:
                    // Set up timer
                    m_genericTimer = Time.time + m_analysisTime;
                    break;
            }

            // Set state
            m_currentState = _state;
            // Invoke state change events
            m_onStateChange?.Invoke(m_currentState);
        }
        #endregion

        #region Scan-Related Functionality
        public void BeginScan()
        {
            if (!m_lever.Locked || m_sampleContainer.CurrentSample == null) return;
            SetState(FTIRState.Countdown);
        }

        public void StartAnalysis(Sample _sample)
        {
            m_currentSample = _sample;
            SetState(FTIRState.Analyzing);
            //m_currentAgent = _agent;
        }

        public void DisplayResults()
        {
            SetState(FTIRState.Completed);
            m_guiState.CurrentSubstate.SetTextMesh(0, $"Scan0{m_scanCount}");
            if (m_currentSample.Chemical != null)
            {
                // Update device GUI
                m_guiState.CurrentSubstate.SetAgentDisplay(m_currentSample.Chemical);
                // Invoke chemical identify event
                m_parent.IdentifyChemical(m_currentSample);
                //m_sampleArea?.OnIdentifyAgent?.Invoke();
            }
            m_currentSample.Analyzed = true;
        }
        #endregion
    }

    public enum FTIRState { CleanPrompt, Idle, Countdown, Scanning, Analyzing, Completed}
}

