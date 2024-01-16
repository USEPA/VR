using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadTestMarker : MonoBehaviour
    {
        public RadTestZone m_parent;
        public float m_targetDistance = 0.0f;
        public float m_targetAngle = 0.0f;
        public float m_targetAngleDifference = 0.0f;
        public float m_score = 0.0f;
        public float m_dotProduct = 0.0f;
        public float m_radLevel;
        public Color m_outputColor;

        public float RadLevel { get => m_radLevel; }


        public void Init(RadTestZone _parent, float _distance, float _angle, float _angleDifference, float _dotProduct, float _score, float _radLevel, Color _outputColor)
        {
            m_parent = _parent;
            m_targetDistance = _distance;
            m_targetAngle = _angle;
            m_targetAngleDifference = _angleDifference;
            m_dotProduct = _dotProduct;
            m_score = _score;
            m_radLevel = _radLevel;
            m_outputColor = _outputColor;

            if (Application.isPlaying && TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
            {
                mesh.material.color = m_outputColor;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_parent)
            {
                Gizmos.color = m_outputColor;
                Gizmos.DrawLine(transform.position, m_parent.transform.position);
            }
        }

    }
}

