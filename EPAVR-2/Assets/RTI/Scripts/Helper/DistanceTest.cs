using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class DistanceTest : MonoBehaviour
    {
        #region Public Variables
        [Header("Input")]
        public Transform m_targetPoint;
        public float m_radius = 1.5f;
        [Header("Output")]
        public float m_distance = 0.0f;
        public float m_score = 0.0f;
        public string m_scoreDisplay;
        #endregion

        #if UNITY_EDITOR
        [ContextMenu("Calculate Score from Distance")]
        public void CalculateDistanceScore()
        {
            m_distance = MathHelper.QuickDistance(transform.position, m_targetPoint.position);
            //m_score = m_distance / (m_radius * 2);
            m_score = Mathf.Clamp(1 - (m_distance / m_radius), 0.0f, 1.0f);
            m_scoreDisplay = (m_score * 5).ToString("0.00");
        }
        #endif
    }
}

