using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Contamination : MonoBehaviour
    {
        #region Protected Variables
        public ChemicalAgent m_agent;
        public MatterType m_type;
        public float m_concentration = 100.0f;

        public Vector3 m_epicenter;
        public float m_baseDistanceFromOrigin;
        public float m_maxDistanceFromEpicenter;

        public float m_slope;

        protected ChemicalContaminantSite m_parentSite;
        protected bool m_isIdentified = false;
        protected Action<Sample> m_onIdentified;
        #endregion
        #region Public Properties
        public ChemicalAgent Agent { get => m_agent; }
        public MatterType Type { get => m_type; }
        public float Concentration { get => m_concentration; }

        public Vector3 Epicenter { get => m_epicenter; }

        public ChemicalContaminantSite ParentSite { get => m_parentSite; }
        public bool IsIdentified { get => m_isIdentified; }

        public Action<Sample> OnIdentified { get => m_onIdentified; set => m_onIdentified = value; }
        #endregion

        #region Initialization
        public void Init(ChemicalAgent _agent, MatterType _type, float _concentration = 100.0f)
        {
            // Cache references/values
            m_agent = _agent;
            m_type = _type;
            m_concentration = _concentration;
        }

        public void SetEpicenter(Vector3 _point, float _distanceFromOrigin, float _maxDistance)
        {
            m_epicenter = _point;
            m_baseDistanceFromOrigin = _distanceFromOrigin;
            m_maxDistanceFromEpicenter = _maxDistance;

            m_slope = (0.0f - m_concentration) / (m_maxDistanceFromEpicenter);
        }
        #endregion

        #region Identification-Related Functionality
        public void OnSampleIdentified(Sample _sample)
        {
            if (m_isIdentified) return;
            // Set identified state
            m_isIdentified = true;
            // Invoke any necessary events
            m_onIdentified?.Invoke(_sample);
        }
        #endregion

        #region Helper Methods
        public bool PointWithinContaminantBounds(Vector3 _point)
        {
            // Get the distance from the epicenter
            float epicenterDistance = GetDistanceFromEpicenter(_point);
            return (epicenterDistance <= m_maxDistanceFromEpicenter);
        }

        public float GetDistanceFromEpicenter(Vector3 _point)
        {
            return Mathf.Abs(MathHelper.QuickDistance(m_epicenter, _point));
        }

        public float GetConcentrationFromPoint(Vector3 _point)
        {
            // Get the distance from the epicenter
            float epicenterDistance = Mathf.Abs(MathHelper.QuickDistance(m_epicenter,_point));

            float concentration = Mathf.Clamp((m_slope * epicenterDistance) + m_concentration, 0.0f, m_concentration);

            //float concentration = (m_slope * epicenterDistance) + m_concentration;
            UnityEngine.Debug.Log($"{gameObject.name}: {_point.ToString()} - distance from epicenter: {epicenterDistance} | Concentration: {concentration} || Time: {Time.time}");
            return concentration;
        }

        public static bool IsTraceAmount(float value)
        {
            return (value <= 10.0f);
        }
        #endregion
    }

}
