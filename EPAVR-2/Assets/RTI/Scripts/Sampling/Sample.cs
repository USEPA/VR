using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Sample
    {
        #region Protected Variables
        protected SampleArea m_sourceArea;
        protected Contamination m_source;

        protected ChemicalAgent m_chemicalAgent;
        protected SampleTool m_usedTool;
        protected MatterType m_type;
        protected float m_amount;
        protected float m_time;

        protected bool m_isCrossContaminated = false;
        protected bool m_analyzed = false;
        #endregion
        #region Public Properties
        public SampleArea SourceArea { get => m_sourceArea; }

        public Contamination Source { get => m_source; }
        public ChemicalAgent Chemical { get => m_chemicalAgent; }

        public MatterType Type { get => m_type; }
        public float Amount { get => m_amount; set => m_amount = value; }
        public float Time { get => m_time; }
        public SampleTool UsedTool { get => m_usedTool; set => m_usedTool = value; }

        public bool IsCrossContaminated { get => m_isCrossContaminated; set => m_isCrossContaminated = true; }
        public bool Analyzed { get => m_analyzed; set => m_analyzed = value; }
        #endregion

        #region Constructors
        public Sample(SampleArea _sourceArea, ChemicalAgent _chemicalAgent = null, float _amount = 100.0f)
        {
            m_sourceArea = _sourceArea;
            m_chemicalAgent = _chemicalAgent;
            m_amount = _amount;
        }

        public Sample(ChemicalAgent _chemicalAgent, float _amount = 100.0f)
        {
            m_sourceArea = null;
            m_chemicalAgent = _chemicalAgent;
            m_amount = _amount;
        }

        public Sample(Contamination _source, float _amount = 100.0f)
        {
            m_source = _source;
            m_type = _source.Type;
            if (_source.Agent != null) m_chemicalAgent = _source.Agent;
            m_amount = _amount;
            m_time = UnityEngine.Time.time;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="_otherSample">Pre-existing sample</param>
        /// <param name="_amountFactor">How much of the other sample to copy over</param>
        public Sample(Sample _otherSample, float _amountFactor = 1.0f)
        {
            if (_otherSample.SourceArea != null) m_sourceArea = _otherSample.SourceArea;
            if (_otherSample.Source != null) m_source = _otherSample.Source;
            if (_otherSample.Chemical != null) m_chemicalAgent = _otherSample.Chemical;
            if (_otherSample.UsedTool != null) m_usedTool = _otherSample.UsedTool;
            m_type = _otherSample.Type;
            m_amount = (_otherSample.Amount * _amountFactor);
            m_time = _otherSample.Time;

            m_isCrossContaminated = _otherSample.IsCrossContaminated;
        }
        #endregion
    }
}

