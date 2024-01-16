using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class GeminiRamanMode : GeminiState
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] RamanLaserPointer m_laserPointer;
        [SerializeField] ObjectAttachPoint m_vialSlot;
        [SerializeField] GeminiVial m_vialPrefab;
        [Header("Configuration")]
        [SerializeField] float m_scanDelay = 15.0f;
        [SerializeField] float m_vialScanTime = 5.0f;
        [SerializeField] float m_analysisTime = 5.0f;
        [SerializeField] float m_laserMaxDistance = 0.0762f;
        [SerializeField] float m_laserIdentificationTime = 1.0f;
        #endregion
        #region Private Variables
        private DeviceGUIState m_guiState;
        private RamanState m_currentState;

        private GeminiVial m_currentInsertedVial;

        private Sample m_currentSample;
        private SampleArea m_sampleArea;
        private ChemicalAgent m_currentAgent;

        private int m_scanCount = 0;
        private float m_genericTimer;
        private Action<RamanState> m_onStateChange;
        private Action<GeminiVial> m_onVialInserted;
        private Action<GeminiVial> m_onVialRemoved;
        #endregion
        #region Public Properties
        public override GModeType Type => GModeType.Raman;
        public RamanState CurrentState { get => m_currentState; }
        public override string Title => "Raman Scan";

        public ObjectAttachPoint VialSlot { get => m_vialSlot; }
        public float LaserMaxDistance { get => m_laserMaxDistance; }
        public float LaserIdentificationTime { get => m_laserIdentificationTime; }

        public GeminiVial CurrentInsertedVial { get => m_currentInsertedVial; }
        #endregion

        #region Initialization
        public override void Init(Gemini _parent)
        {
            // Call base functionality
            base.Init(_parent);
            if (_parent.GUI == null) UnityEngine.Debug.Log($"Gemini GUI reference was null! || Time: {Time.time}");
            if (_parent.GUI.Modes == null) UnityEngine.Debug.Log($"Gemini GUI mode dictionary was null! || Time: {Time.time}");
            // Cache GUI state reference
            m_guiState = _parent.GUI.Modes[Type];
            // Hook up events
            m_onStateChange += i => m_guiState.SetSubstate((int)i);
            m_guiState.Substates[0].Buttons[0].onClick.AddListener(BeginScan);
            m_onVialInserted += i => m_parent.GUI.SetButtonStatusActive(1, true);
            m_onVialRemoved += i => m_parent.GUI.SetButtonStatusActive(1, false);
            // Initialize components
            m_laserPointer?.Init(this);
            m_laserPointer.OnSetActive += i => m_parent.GUI.SetButtonStatusActive(0, i);
            m_laserPointer.OnConfirmAgent += i => StartAnalysis(i);
            // Spawn the gemini vial
            if (m_vialPrefab != null)
            {
                GeminiVial vial = Instantiate(m_vialPrefab, m_vialSlot.transform);
                vial.Init(this);
            }
            // Set default state
            SetState(RamanState.Idle);
        }
        #endregion

        #region Mode-Related Functionality
        public override void OnEnter()
        {
            // Activate laser pointer
            //m_laserPointer.Active = true;
        }

        public override void OnUpdate()
        {
            // Check state
            switch (m_currentState)
            {
                case RamanState.Countdown:
                    // Update countdown timer
                    m_guiState.CurrentSubstate.SetTextMesh(0, $"{Mathf.RoundToInt(m_genericTimer - Time.time)}");
                    if (Time.time > m_genericTimer) SetState(RamanState.Scanning);
                    break;
                case RamanState.Scanning:
                    if (m_currentInsertedVial == null || m_currentInsertedVial.CurrentSample == null)
                    {
                        // Tick laser pointer
                        m_laserPointer.Tick();
                    }
                    else
                    {
                        // Wait for vial to finish initial scan
                        if (Time.time > m_genericTimer) StartAnalysis(m_currentInsertedVial.CurrentSample);
                    }
                    break;
                case RamanState.Analyzing:
                    m_guiState.CurrentSubstate.SetSlider(0, (1.0f - ((m_genericTimer - Time.time) / m_analysisTime)));
                    if (Time.time > m_genericTimer) DisplayResults();
                    break;
                default:
                    break;
            }
        }

        public override void OnExit()
        {
            ResetMode();
        }
        #endregion

        #region State-Related Functionality
        public void SetState(RamanState _state)
        {
            // Check prior state
            switch (m_currentState)
            {
                case RamanState.Countdown:
                    // Reset timer
                    m_genericTimer = -1.0f;
                    break;
                case RamanState.Scanning:
                    // Disable laser pointer
                    //m_laserPointer.Active = false;
                    m_genericTimer = -1.0f;
                    break;
                case RamanState.Analyzing:
                    // Reset timer
                    m_genericTimer = -1.0f;
                    break;
            }
  
            // Check new state
            switch (_state)
            {
                case RamanState.Countdown:
                    // Set up timer
                    m_genericTimer = Time.time + m_scanDelay;
                    break;
                case RamanState.Scanning:
                    // Iterate scan count
                    m_scanCount++;
                    if (m_currentInsertedVial == null || m_currentInsertedVial.CurrentSample == null)
                    {
                        // Enable laser pointer
                        //m_laserPointer.Active = true;
                    }
                    else
                    {
                        // Start the timer
                        m_genericTimer = Time.time + m_vialScanTime;
                    }
                    break;
                case RamanState.Analyzing:
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
        public void InsertVial(GeminiVial _vial)
        {
            // Attach the vial to the insert slot
            m_vialSlot.AttachObject(_vial.gameObject);
            // Cache reference
            m_currentInsertedVial = _vial;
   
            // Activate any events
            if (m_currentInsertedVial.CurrentSample != null) m_onVialInserted?.Invoke(_vial);
        }

        public void RemoveVial()
        {
            // Activate any events
            if (m_currentInsertedVial.CurrentSample != null)
            {
                m_onVialRemoved?.Invoke(m_currentInsertedVial);
                if (m_currentState == RamanState.Analyzing && m_currentSample == m_currentInsertedVial.CurrentSample)
                {
                    ResetMode();
                }
            }
        }

        public void BeginScan()
        {
            SetState(RamanState.Countdown);
        }

        public void StartAnalysis(Sample _sample)
        {
            m_currentSample = _sample;
            SetState(RamanState.Analyzing);
            //m_currentAgent = _agent;
        }

        public void DisplayResults()
        {
            SetState(RamanState.Completed);
            m_guiState.CurrentSubstate.SetTextMesh(0, $"Scan0{m_scanCount}");
            if (m_currentSample.Chemical != null) 
            {
                // Update device GUI
                m_guiState.CurrentSubstate.SetAgentDisplay(m_currentSample.Chemical);
                // Invoke chemical identify event
                m_parent.IdentifyChemical(m_currentSample);
                m_sampleArea?.OnIdentifyAgent?.Invoke();

            }
            m_currentSample.Analyzed = true;
        }
        #endregion

        #region Helper Methods
        public void ResetMode()
        {
            m_genericTimer = -1.0f;
            m_currentAgent = null;
            m_currentSample = null;
            SetState(RamanState.Idle);
        }
        #endregion
    }

    public enum RamanState {Idle, Countdown, Scanning, Analyzing, Completed}
}

