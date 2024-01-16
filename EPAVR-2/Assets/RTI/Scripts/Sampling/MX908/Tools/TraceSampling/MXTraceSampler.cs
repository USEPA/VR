using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MXTraceSampler : MX908Tool
    {
        #region Inspector Assigned Variables
        [Header("Tool Configuration")]
        [SerializeField] int m_maxSwabs = 8;
        [SerializeField] [Range(0.1f, 1.0f)] float m_maxSampleAmount = 0.25f;
        [SerializeField] float m_swabAnalyzeTime = 30.0f;
        [SerializeField] protected float m_initializationTime = 1.0f;
        [SerializeField] protected float m_deliveryBoxRespawnDelay = 5.0f;
        [Header("Important References")]
        [SerializeField] TraceSampleSwab m_sampleSwabPrefab;
        [SerializeField] List<Transform> m_sampleSwabSpawnPoints;
        [SerializeField] Collider m_sampleSwabInsertCollider;
        [Header("Prefab References")]
        [SerializeField] SampleSwabDeliveryBox m_deliveryBoxPrefab;
        #endregion
        #region Protected Variables
        protected TraceSampleState m_currentState = TraceSampleState.Idle;
        protected List<TraceSampleSwab> m_activeSwabs;

        protected TraceSampleSwab m_currentInsertedSwab;
        protected float m_analyzeTimer = -1.0f;
        protected bool m_analyzingSample = false;
        protected float m_thirdOfTime;
        protected float m_tenthOfTime;
        protected float m_deliveryBoxRespawnTimer = -1.0f;
        protected int m_spawnedSwabCount = 0;

        protected Action<TraceSampleSwab> m_onGrabSwab;
        protected Action<TraceSampleSwab> m_onInsertSwab;
        protected Action<TraceSampleSwab> m_onFinishAnalysis;
        protected Action<TraceSampleState, float> m_onStateChange;
        protected Action<float> m_onStateUpdate;

        protected ChemicalAgent m_identifiedAgent;
        protected MXTraceSamplerGUI m_traceSamplerGUI;

        protected SampleSwabDeliveryBox m_deliveryBox;

        protected IEnumerator m_analyzeCoroutine;
        #endregion
        #region Public Properties
        public override MXMode Mode => MXMode.TraceSampling;

        public TraceSampleState CurrentState { get => m_currentState; }
        public int MaxSwabs { get => m_maxSwabs; }

        public bool AnalyzingSample { get => m_analyzingSample; }

        public Collider SampleSwabInsertCollider { get => m_sampleSwabInsertCollider; }

        public List<TraceSampleSwab> ActiveSwabs { get => m_activeSwabs; }

        public SampleSwabDeliveryBox DeliveryBox { get => m_deliveryBox; }
        #endregion

        #region Initialization
        public override void Init(MX908 _parent)
        {
            // Call base functionality
            base.Init(_parent);
            // Cache references
            m_sampleSwabSpawnPoints = _parent.SampleSwabSpawnPoints;
            m_traceSamplerGUI = _parent.GUI.TraceSamplerGUI;
            m_traceSamplerGUI.Init(this);
            m_thirdOfTime = m_swabAnalyzeTime * 0.3333f;
            m_tenthOfTime = m_swabAnalyzeTime * 0.1f;
            // Hook up UI events
            m_onStateChange += (i,j) => m_traceSamplerGUI.SetStateGUI(i, j);
            // Set default values
            m_identifiedAgent = null;
            // Set default state
            //SetState(TraceSampleState.Idle);
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
   
        }

        #region Tool State Functionality
        public override void OnApplied()
        {
            // Call base functionality
            base.OnApplied();
            //m_traceSamplerGUI.gameObject.SetActive(true);
            // Initiialize swab list if necessary
            if (m_activeSwabs == null) m_activeSwabs = new List<TraceSampleSwab>();
            // Spawn the sample swabs
            for (int i = 0; i < m_sampleSwabSpawnPoints.Count; i++)
            { 
                SpawnSwab(i); 
            }
            // Spawn the delivery box
            //SpawnDeliveryBox();
            SetState(TraceSampleState.Initializing);
        }

        public override void OnUpdate()
        {
            // Call base functionality
            base.OnUpdate();
            // Invoke necessary state-based logic
            if (m_currentState == TraceSampleState.ProcessSample || m_currentState == TraceSampleState.Analyzing)
            {
                m_onStateUpdate?.Invoke(Time.deltaTime);
            }

            /*
            // Respawn the delivery box
            if (m_deliveryBox == null && (Time.time > m_deliveryBoxRespawnTimer))
                SpawnDeliveryBox();
            */
            /*
            // Check if sample is being analyzed
            if (m_analyzingSample && m_currentInsertedSwab != null)
            {
                // Check if timer is up
                if (Time.time >= m_analyzeTimer)
                {
                    // Analysis finished, display results

                }
            }
            */

        }

        public override void OnRemoved()
        {
            // Call base functionality
            base.OnRemoved();
            m_traceSamplerGUI.gameObject.SetActive(false);
            // Disable the sample swabs
            for (int i = 0; i < m_activeSwabs.Count; i++)
            {
                // Destroy the swab
                TraceSampleSwab swab = m_activeSwabs[i];
                Destroy(swab.gameObject);
            }
            // Re-initialize list
            m_activeSwabs = new List<TraceSampleSwab>();
            /*
            // Destroy the delivery box
            if (m_deliveryBox) Destroy(m_deliveryBox.gameObject);
            m_deliveryBox = null;
            */
        }
        #endregion

        #region Swab Functionality
        public TraceSampleSwab SpawnSwab(int _spawnIndex)
        {
            if ((m_activeSwabs.Count + 1) > m_maxSwabs || _spawnIndex < 0 || _spawnIndex > m_sampleSwabSpawnPoints.Count) return null;
            // Create the swab
            TraceSampleSwab swab = Instantiate(m_sampleSwabPrefab, m_sampleSwabSpawnPoints[_spawnIndex]);
            swab.Init(this, _spawnIndex);
            // Add this to the active swabs
            m_activeSwabs.Add(swab);
            m_spawnedSwabCount++;
            swab.gameObject.name = $"MXTraceSampleSwab_{m_spawnedSwabCount}";
            return swab;
        }

        public void GrabSwab(TraceSampleSwab _swab)
        {
            if (m_currentInsertedSwab != null && m_currentInsertedSwab == _swab && m_currentState == TraceSampleState.StartAnalysis) return;
            m_onGrabSwab?.Invoke(_swab);
            //UnityEngine.Debug.Log($"Grab Swab called: {_swab.gameObject.name} || Time: {Time.time}");
            // Check if this is the first time this object has been grabbed
            if (!_swab.Active)
            {
                // Unparent the object
                _swab.transform.parent = null;
                // Set active and attempt to spawn a new swab in its place
                _swab.Active = true;
                SpawnSwab(_swab.SpawnIndex);
            }
            else if (_swab == m_currentInsertedSwab)
            {
                // Unparent the object
                _swab.transform.parent = null;
                
                m_currentInsertedSwab.AtAttachPoint = false;
                m_currentInsertedSwab.AttachPointCollider = null;
                
                m_currentInsertedSwab = null;
                // Cancel coroutine
                StopCoroutine(PerformAnalysis());
                // Reset state
                SetState(TraceSampleState.Idle);
            }
        }

        public void InsertSwab(TraceSampleSwab _swab)
        {
            // Make sure there is not already a swab inserted
            if (m_currentInsertedSwab != null) return;
            // Snap the swab into the slot
            _swab.transform.parent = m_sampleSwabInsertCollider.transform;
            _swab.transform.localPosition = Vector3.zero;
            _swab.transform.localEulerAngles = Vector3.zero;
            // Disable interaction for this swab
            // Invoke events
            m_onInsertSwab?.Invoke(_swab);
            // Set reference
            m_currentInsertedSwab = _swab;
            // Begin analysis
            BeginSampleAnalysis();
        }

        public void SpawnDeliveryBox()
        {
            // Spawn and initialize the delivery box object
            m_deliveryBox = Instantiate(m_deliveryBoxPrefab, GetComponent<XRToolbeltItem>().AttachPoint);
            m_deliveryBox.Init(this);
            // Reset respawn timer
            m_deliveryBoxRespawnTimer = -1.0f;
        }

        public void DestroyDeliveryBox()
        {
            // Check if any of the current trace sample swabs are within the delivery box
            if (m_activeSwabs != null && m_activeSwabs.Count > 0 && m_deliveryBox.InsertedSwabs != null && m_deliveryBox.InsertedSwabs.Count > 0)
            {
                // Loop through each swab reference and check if it needs to be removed
                for (int i = 0; i < m_activeSwabs.Count; i++)
                {
                    // Get the swab reference
                    TraceSampleSwab swab = m_activeSwabs[i];
                    // Check if the inserted swabs list contains this reference
                    if (m_deliveryBox.InsertedSwabs.Contains(swab))
                        m_activeSwabs.Remove(swab);
                }
            }
            // Cache/destroy delivery box object and reference
            GameObject deliveryBoxObject = m_deliveryBox.gameObject;
            m_deliveryBox = null;
            Destroy(deliveryBoxObject);
            // Set respawn timer
            m_deliveryBoxRespawnTimer = Time.time + m_deliveryBoxRespawnDelay;
        }

        #endregion

        #region State Functionality
        public void BeginSampleAnalysis()
        {
            // Check base conditions
            if (m_currentInsertedSwab == null) return;
            // Set timer
            //m_analyzeTimer = Time.time + m_swabAnalyzeTime;
            // Set analyzing
            m_analyzingSample = true;
            StartCoroutine(PerformAnalysis());
        }

        public void FinishSampleAnalysis()
        {
            // Update agent display
            //m_traceSamplerGUI.CurrentStateGUI.SetAgentDisplay(m_currentInsertedSwab.Contaminant);
            // Re-enable swab interaction
            // Display analysis results
            DisplayAnalysisResults();
            // Reset values
            //m_analyzeTimer = -1.0f;
            if (m_currentInsertedSwab.CurrentSample != null) m_currentInsertedSwab.Analyzed = true;
            m_analyzingSample = false;
            m_onFinishAnalysis?.Invoke(m_currentInsertedSwab);
        }

        public void SetState(TraceSampleState _state, float _time = -1.0f)
        {
            // Check if there is anything we need to remove from the current state
            switch (m_currentState)
            {
                case TraceSampleState.ProcessSample:
                    m_onStateUpdate -= i => UpdateProcessSampleState(i);
                    break;
                case TraceSampleState.Analyzing:
                    m_onStateUpdate -= i => UpdateAnalyzingDisplay(i);
                    break;
            }
            // Set the state and invoke any necessary events
            m_currentState = _state;
            m_onStateChange.Invoke(m_currentState, _time);
            // Check if any specific events need to be hooked up
            switch (m_currentState)
            {
                case TraceSampleState.Initializing:
                    StartCoroutine(StartupTraceSampler());
                    break;
                case TraceSampleState.ProcessSample:
                    m_onStateUpdate += i => UpdateProcessSampleState(i);
                    break;
                case TraceSampleState.Analyzing:
                    m_onStateUpdate += i => UpdateAnalyzingDisplay(i);
                    break;
                case TraceSampleState.Completed:
                    //DisplayAnalysisResults();
                    FinishSampleAnalysis();
                    break;
            }
        }

        IEnumerator PerformAnalysis()
        {
            // Buffer sample
            SetState(TraceSampleState.StartAnalysis, m_tenthOfTime);
            yield return new WaitForSeconds(m_tenthOfTime);
            bool isOversaturated = ((m_currentInsertedSwab.CurrentSample != null) && ((m_currentInsertedSwab.Contamination / 100.0f) > m_maxSampleAmount));
            bool isCrossContaminated = ((m_currentInsertedSwab.CurrentSample != null) && m_currentInsertedSwab.CurrentSample.IsCrossContaminated);
            if (m_currentInsertedSwab.CurrentSample == null || isCrossContaminated || isOversaturated)
            {
                //UnityEngine.Debug.Log($"Swab: {m_currentInsertedSwab.gameObject.name} - too much contamination | Amount: {m_currentInsertedSwab.Contamination} || Time: {Time.time}");
                SetState(TraceSampleState.InvalidSample);
                m_analyzingSample = false;
            }
            else
            {
                // Process sample
                m_analyzeTimer = Time.time + m_swabAnalyzeTime;
                SetState(TraceSampleState.ProcessSample, m_swabAnalyzeTime);
                while (Time.time < m_analyzeTimer)
                {
                    yield return null;
                }

                // Analyze findings
                m_analyzeTimer = Time.time + m_tenthOfTime;
                SetState(TraceSampleState.Analyzing, m_tenthOfTime);
                yield return new WaitForSeconds(m_tenthOfTime);

                // Display results
                SetState(TraceSampleState.Completed);
                m_analyzingSample = false;
            }
        }


        public void UpdateProcessSampleState(float _deltaTime)
        {
            // Make sure base conditions are valid
            if (m_currentState != TraceSampleState.ProcessSample || !m_traceSamplerGUI || m_traceSamplerGUI.CurrentStateGUI == null) return;
            // Get current progress
            float time = m_analyzeTimer - Time.time;
            time = Mathf.Clamp(time, 0.0f, m_swabAnalyzeTime);
            string timeDisplay = (Mathf.FloorToInt(time) <= 9 && time.ToString("F0") != "10") ? $":0{time.ToString("F0")}" : $":{time.ToString("F0")}";
            // Update time display
            m_traceSamplerGUI.CurrentStateGUI.SetText(0, timeDisplay);
            // Get the time difference
            float timeDiff = m_swabAnalyzeTime - time;
            float progress = (time < m_thirdOfTime) ? (time / m_thirdOfTime) : (timeDiff / (m_swabAnalyzeTime-m_thirdOfTime));
            progress = Mathf.Clamp(progress, 0.0f, 1.0f);
            string statusText = (time < m_thirdOfTime) ? "Cooling" : "Heating";
            // Set text to heating
            m_traceSamplerGUI.CurrentStateGUI.SetText(1, statusText);
            // Simply increase the value of the slider
            m_traceSamplerGUI.CurrentStateGUI.SetSlider(0, progress);
            /*
            // Check progress
            if (progress < (0.66f))
            {
                // Set text to heating
                m_traceSamplerGUI.CurrentStateGUI.SetText(1, "Heating");
                // Simply increase the value of the slider
                m_traceSamplerGUI.CurrentStateGUI.SetSlider(0, progress);
            }
            */
        }

        public void UpdateAnalyzingDisplay(float _deltaTime)
        {
            // Make sure base conditions are valid
            if (m_currentState != TraceSampleState.Analyzing || !m_traceSamplerGUI || m_traceSamplerGUI.CurrentStateGUI == null) return;
            // Get current progress
            float time = m_analyzeTimer - Time.time;
            time = Mathf.Clamp(time, 0.0f, m_tenthOfTime);
            float progress = (m_tenthOfTime - time) / m_tenthOfTime;
            progress = Mathf.Clamp(progress, 0.0f, 1.0f);
            // Update slider
            m_traceSamplerGUI.CurrentStateGUI.SetSlider(0, progress);
        }

        public void DisplayAnalysisResults()
        {
            if (m_currentState != TraceSampleState.Completed || m_currentInsertedSwab == null || !m_traceSamplerGUI || m_traceSamplerGUI.CurrentStateGUI == null) return;
            UnityEngine.Debug.Log($"TraceSampler arrived in DisplayAnalysisResults | Contaminant: {m_currentInsertedSwab.Contaminant} || Time: {Time.time}");
            // Update the display info
            m_traceSamplerGUI.CurrentStateGUI.SetAgentDisplay(m_currentInsertedSwab.Contaminant);
            m_parent.IdentifyChemical(m_currentInsertedSwab.CurrentSample);
        }

        IEnumerator StartupTraceSampler()
        {
            yield return new WaitForSeconds(m_initializationTime);
            SetState(TraceSampleState.Idle);
        }
        #endregion

        public void OnDestroy()
        {
            
        }
    }

    public enum TraceSampleState { Initializing, Idle, StartAnalysis, InvalidSample, ProcessSample, Analyzing, Completed}
}

