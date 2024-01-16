using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class VaporSensor : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected float m_tickInterval = 0.05f;
        [SerializeField] protected float m_minContactDistance = 0.25f;
        [SerializeField] protected float m_minContactRatio = 0.25f;
        [SerializeField] protected float m_minContactLevel = 0.45f;
        [SerializeField] protected float m_minConfirmLevel = 0.9f;
        [SerializeField] protected float m_levelScaleFactor = 0.9f;
        [SerializeField] protected int m_minConfirmReadings = 4;

        #endregion
        #region Protected Variables
        protected MXAirMonitor m_airMonitor;
        protected bool m_isActive = false;

        public SampleArea m_currentSampleArea;
        public VaporCloud m_currentVaporCloud;

        public ChemicalAgent m_contactedAgent;
        protected float m_updateTimer = 0.0f;
        public float m_currentLevel;
        public bool m_isAgentConfirmed = false;
        public int m_successiveConfirmations = 0;
        protected System.Action<float> m_onUpdateLevel;
        protected System.Action<ChemicalAgent> m_onConfirmAgent;
        public float m_targetDistance;
        public float m_targetDistanceDifference;

        public float currentScore;
        #endregion
        #region Public Properties
        public bool Active { get => m_isActive; }
        public ChemicalAgent ContactedAgent { get => m_contactedAgent; }
        public bool IsAgentConfirmed { get => m_isAgentConfirmed; }
        public System.Action<float> OnUpdateLevel { get => m_onUpdateLevel; set => m_onUpdateLevel = value; }
        public System.Action<ChemicalAgent> OnConfirmAgent { get => m_onConfirmAgent; set => m_onConfirmAgent = value; }
        #endregion

        #region Initialization
        public void Init(MXAirMonitor _airMonitor)
        {
            // Cache referennces
            m_airMonitor = _airMonitor;
            // Hook up events
            m_onConfirmAgent += i => m_airMonitor.DisplayContaminantResults(i);
            if (ScenarioManager.Instance)
            {
                ScenarioManager.Instance.OnObjectiveCompleted.AddListener(TryClearVaporCloud);
                //ScenarioManager.Instance.OnSiteCleared += (i, j) => TryClearVaporCloud(i);
            }
            // Set default values
            m_currentLevel = 0.0f;
            m_contactedAgent = null;
        }

        public void Startup()
        {
            // Initialize default values
            m_contactedAgent = null;
            m_currentSampleArea = null;
            m_currentVaporCloud = null;
            m_isAgentConfirmed = false;
            m_updateTimer = 0.0f;
            ResetValues();
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        #region Update Logic
        public void Tick()
        {
            // Check to make sure the vapor sensor is active first
            m_updateTimer += Time.deltaTime;
            // Check if it is time to update again
            if (m_updateTimer >= m_tickInterval)
            {
                // Get the level of contamination
                UpdateContaminantLevel(CalculateContaminantLevel());
                m_updateTimer = 0.0f;
            }
        }
        #endregion

        #region Collision Logic
        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<VaporCloud>(out VaporCloud vaporCloud) && m_currentVaporCloud != vaporCloud)
            {
                // Set cloud reference
                SetVaporCloud(vaporCloud);
            }
            /*
            if (other.transform.CompareTag("SampleArea"))
            {
                //UnityEngine.Debug.Log($"Vapor sensor entered sample area: {other.gameObject.name} || Time: {Time.time}");
                // Set sample area reference
                if (other.TryGetComponent<SampleVaporArea>(out SampleVaporArea sampleArea) && m_currentSampleArea != sampleArea) SetSampleArea(sampleArea);
            }
            */
        }

        public void OnTriggerExit(Collider other)
        {
            if (m_currentVaporCloud != null && (other.TryGetComponent<VaporCloud>(out VaporCloud vaporCloud) && m_currentVaporCloud == vaporCloud))
            {
                // Reset references/values
                m_currentVaporCloud = null;
                ResetValues();
            }
            /*
            if (other.transform.CompareTag("SampleArea") && m_currentSampleArea != null && other.transform.GetComponent<SampleVaporArea>() == m_currentSampleArea)
            {
                m_currentSampleArea = null;
                m_contactedAgent = null;
                ResetValues();
            }
            */
        }

        #endregion

        #region Sampling Logic
        public void SetSampleArea(SampleArea _sampleArea)
        {
            // Set sample area reference and get its agent
            m_currentSampleArea = _sampleArea;
            if (m_currentSampleArea.Contaminant != null) m_contactedAgent = m_currentSampleArea.Contaminant;
            // Reset values
            ResetValues();
        }

        public void SetVaporCloud(VaporCloud _vaporCloud)
        {
            // Set vapor cloud reference and get its agent
            m_currentVaporCloud = _vaporCloud;
            if (m_currentVaporCloud.Contaminant != null) m_contactedAgent = m_currentVaporCloud.Contaminant;
        }

        public void TryClearVaporCloud(ScenarioObjective _objective)
        { 
            if (_objective is ClearSiteScenarioObjective siteObjective && siteObjective.Site != null)
            {
                TryClearVaporCloud((ChemicalContaminantSite)siteObjective.Site);
            }
        }
        public void TryClearVaporCloud(ChemicalContaminantSite site)
        {
            if (!m_currentVaporCloud || m_currentVaporCloud.Parent != site) return;
            // Clear vapor cloud reference and reset values
            m_currentVaporCloud = null;
            ResetValues();
        }

        public float CalculateContaminantLevel()
        {
            if (m_currentVaporCloud == null) return 0.0f;
            // First, get the closest position to the vapor sensor relative to the bounds of the sample area
            //Vector3 sensorBoundsPosition = m_currentSampleArea.Collider.ClosestPointOnBounds(transform.position);
            // Get the distance between the sample area's epicenter and the vapor sensor's origin
            //m_targetDistance = MathHelper.QuickDistance(transform.position, m_currentSampleArea.Epicenter);
            m_targetDistance = MathHelper.QuickDistance(transform.position, m_currentVaporCloud.Epicenter);
            UnityEngine.Debug.DrawLine(transform.position, m_currentVaporCloud.Epicenter, Color.cyan);
            m_targetDistanceDifference = m_targetDistance - m_minContactDistance;
            //float score = Mathf.Clamp(((-0.9f * Mathf.Abs(m_targetDistance - m_minContactDistance)) + 1.0f), 0.0f, 1.0f);
            //float score = m_minContactDistance / m_targetDistance;
            float score = ((m_currentVaporCloud.Radius *2) * m_minContactRatio) / m_targetDistance;
            // Scale the score based on type/concenteation
            if (m_currentVaporCloud.Parent.SpillType != MatterType.Gas)
            {
                float adjustedScore = score * m_levelScaleFactor;
                //UnityEngine.Debug.Log($"Original Score: {score} | Adjusted: {adjustedScore} || Time: {Time.time}");
                score = adjustedScore;
            }
            currentScore = score;
            // Score the current value based on the minimum detection distance
            return score;
            //return (m_targetDistance / m_minContactDistance);
        }

        /*
        public float CalculateContaminantLevel()
        {
            if (m_currentSampleArea == null) return 0.0f;
            // First, get the closest position to the vapor sensor relative to the bounds of the sample area
            //Vector3 sensorBoundsPosition = m_currentSampleArea.Collider.ClosestPointOnBounds(transform.position);
            // Get the distance between the sample area's epicenter and the vapor sensor's origin
            //m_targetDistance = MathHelper.QuickDistance(transform.position, m_currentSampleArea.Epicenter);
            m_targetDistance = MathHelper.QuickDistance(transform.position, m_currentSampleArea.Epicenter);
            UnityEngine.Debug.DrawLine(transform.position, m_currentSampleArea.Epicenter, Color.cyan);
            m_targetDistanceDifference = m_targetDistance - m_minContactDistance;
            //float score = Mathf.Clamp(((-0.9f * Mathf.Abs(m_targetDistance - m_minContactDistance)) + 1.0f), 0.0f, 1.0f);
            float score = m_minContactDistance / m_targetDistance;
            currentScore = score;
            // Score the current value based on the minimum detection distance
            return score;
            //return (m_targetDistance / m_minContactDistance);
        }
        */

        void UpdateContaminantLevel(float value)
        {
            m_currentLevel = value;
            m_onUpdateLevel?.Invoke(m_currentLevel);

            if (!m_isAgentConfirmed && m_currentLevel > m_minConfirmLevel)
            {
                // Check if the current level is greater than the minimum confirmation level
                if (m_currentLevel > m_minConfirmLevel)
                {
                    // Iterate successive confirmations
                    m_successiveConfirmations++;
                    // Check if there are enough successive confirmations to identify the agent
                    if (m_successiveConfirmations > m_minConfirmReadings)
                    {
                        m_isAgentConfirmed = true;
                        m_onConfirmAgent?.Invoke(m_contactedAgent);
                    }
                }
                else if (!m_currentVaporCloud.Parent.FirstContacted && m_currentLevel > m_minContactLevel)
                {
                    if (m_currentVaporCloud.Parent.Status == SiteStatus.Idle)
                    {
                        m_currentVaporCloud.Parent.FirstContact();
                    }
                }
     
            }
        }
        #endregion

        #region Reset Logic
        public void ResetValues()
        {
            m_currentLevel = 0.0f;
            m_successiveConfirmations = 0;
            m_isAgentConfirmed = false;
            m_onConfirmAgent?.Invoke(null);
        }
        #endregion
    }
}

