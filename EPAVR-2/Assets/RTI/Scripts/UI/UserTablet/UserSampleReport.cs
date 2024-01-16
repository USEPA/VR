using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class UserSampleReport : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Inspector Assigned Variables")]
        [SerializeField] private TextMeshProUGUI m_reportName;
        [SerializeField] private TextMeshProUGUI m_sampleType;

        [SerializeField] private GameObject m_analysisProgressContainer;
        [SerializeField] private Slider m_analysisSlider;

        [SerializeField] private AgentIdentificationDisplay m_agentDisplay;
        #endregion
        #region Private Variables
        private SampleRequest m_request;
        private Sample m_sample;
        private bool m_analysisComplete = false;
        #endregion
        #region Public Properties
        public SampleRequest Request { get => m_request; }
        public Sample Sample { get => m_sample; }
        #endregion

        #region Initialization
        public void Init(SampleRequest _request, string name, bool _analyzed = true)
        {
            // Cache reference
            m_sample = _request.Sample;
            // Set report name
            m_reportName.text = name;

            // Set matter type
            //m_sampleType.text = _sample.Type.ToString();
            m_sampleType.text = MathHelper.FormatTime(m_sample.Time);

            // Set agent identification
            m_agentDisplay.ParseAgentInfo(m_sample.Chemical);

            // Set whether or not the result display should be active by default
            SetAnalyzed(_analyzed);
        }

        public void Init(Sample _sample, string name, bool _analyzed = true)
        {
            // Cache reference
            m_sample = _sample;

            // Set report name
            m_reportName.text = name;

            // Check if this is pre-analyzed
            if (_analyzed)
            {
                // Set matter type
                //m_sampleType.text = _sample.Type.ToString();
                m_sampleType.text = MathHelper.FormatTime(_sample.Time);

                // Set agent identification
                m_agentDisplay.ParseAgentInfo(_sample.Chemical);
            }
            
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

        #region Helper Methods
        public void SetAnalyzed(bool _value)
        {
            if (_value == true)
            {
                m_analysisProgressContainer.SetActive(false);
                m_agentDisplay.gameObject.SetActive(true);
            }
            else
            {
                m_agentDisplay.gameObject.SetActive(false);
                m_analysisProgressContainer.SetActive(true);
            }
        }
        public void TickProgress(float _value)
        {
            if (!m_analysisSlider) return;
            m_analysisSlider.value = _value;
        }
        #endregion
    }
}

