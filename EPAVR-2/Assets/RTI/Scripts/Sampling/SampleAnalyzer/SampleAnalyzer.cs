using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class SampleAnalyzer : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected float m_analysisTime = 15.0f;
        [Header("Events")]
        [SerializeField] protected UnityEvent<SampleRequest> m_onAddRequest;
        [SerializeField] protected UnityEvent<float> m_onAnalysisTimerTick;
        [SerializeField] protected UnityEvent<SampleRequest> m_onAnalysisStart;
        [SerializeField] protected UnityEvent<SampleRequest> m_onAnalysisComplete;
        #endregion
        #region Protected Variables
        protected List<SampleRequest> m_sampleQueue;
        protected List<Sample> m_analyzedSamples;

        protected SampleRequest m_currentRequest;
        protected float m_analysisTimer = 0.0f;
        #endregion
        #region Public Properties
        public float AnalysisTime { get => m_analysisTime; set => m_analysisTime = value; }

        public SampleRequest CurrentRequest { get => m_currentRequest; }

        public UnityEvent<float> OnAnalysisTimerTick { get => m_onAnalysisTimerTick; set => m_onAnalysisTimerTick = value; }
        public UnityEvent<SampleRequest> OnAddRequest { get => m_onAddRequest; set => m_onAddRequest = value; }
        public UnityEvent<SampleRequest> OnAnalysisStart { get => m_onAnalysisStart; set => m_onAnalysisStart = value; }
        public UnityEvent<SampleRequest> OnAnalysisComplete { get => m_onAnalysisComplete; set => m_onAnalysisComplete = value; }
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize lists
            m_sampleQueue = new List<SampleRequest>();
            m_analyzedSamples = new List<Sample>();
            // Initialize timer
            m_analysisTimer = 0.0f;
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion

        #region Update
        // Update is called once per frame
        void Update()
        {
            // First, check if there is a sample to process
            if (m_currentRequest != null)
            {
                m_analysisTimer += Time.deltaTime;
                m_onAnalysisTimerTick?.Invoke(m_analysisTimer / m_analysisTime);
                //m_currentRequest.OnTickProgress?.Invoke(m_analysisTimer / m_analysisTime);
                if (m_analysisTimer > m_analysisTime)
                {
                    FinishSampleAnalysis();
                }
                return;
            }
            // First, check if there are any samples to be processed
            //if (m_sampleQueue == null || m_sampleQueue.Count < 1) return;
        }
        #endregion

        #region Sample Analysis-Related Functionality
        public void BeginProcessSample(SampleRequest _request)
        {
            // Remove this request from the queue
            m_sampleQueue.Remove(_request);
            // Cache reference
            m_currentRequest = _request;
            // Invoke any necessary events
            m_onAnalysisStart?.Invoke(_request);
            // Start timer
            m_analysisTimer = 0.0f;

            UnityEngine.Debug.Log($"{gameObject.name} began processing sample request || Time: {Time.time}");
        }

        public void FinishSampleAnalysis()
        {
            // Make sure current request exists
            if (m_currentRequest == null) return;
            // Get the sample from this request
            Sample sample = m_currentRequest.Sample;
            // Add this to the analyzed samples list
            m_analyzedSamples.Add(sample);
            // Invoke any necessary events
            m_onAnalysisComplete?.Invoke(m_currentRequest);
            sample.UsedTool?.IdentifyChemical(sample);
            UnityEngine.Debug.Log($"{gameObject.name} finished sample request analysis: {sample.Chemical} || Time: {Time.time}");
            // Clear reference
            m_currentRequest = null;
            // Reset timer
            m_analysisTimer = 0.0f;
            // Check if there are more samples to be analyzed
            if (m_sampleQueue.Count > 0) BeginProcessSample(m_sampleQueue[0]);
        }
        #endregion

        #region Sample Delivery-Related Functionality
        public void AddRequest(SampleRequest _request)
        {
            UnityEngine.Debug.Log($"{gameObject.name} received sample request || Time: {Time.time}");
            // Add this to the request queue
            m_sampleQueue.Add(_request);
            // Invoke necessary events
            m_onAddRequest?.Invoke(_request);
            // Check if there is no sample currently being analyzed
            if (m_currentRequest == null) BeginProcessSample(_request);
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Clear sample queue
            m_sampleQueue.Clear();
            m_analyzedSamples.Clear();
            // Reset references
            m_currentRequest = null;
            m_analysisTimer = 0.0f;
        }
        #endregion
    }
}

