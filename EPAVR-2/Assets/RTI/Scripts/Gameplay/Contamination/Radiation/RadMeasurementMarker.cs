using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadMeasurementMarker : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private MeshRenderer m_markerMesh;
        [Header("Default Configuration")]
        [SerializeField] private Gradient m_measurementMarkerGradient;
        #endregion
        #region Private Variables
        private float m_value;
        #endregion

        #region Initialization
        public void Init(float _value)
        {
            // Set value
            m_value = _value;
            // Adjust marker color accordingly
            MeshHelper.SetMeshColor(m_markerMesh, m_measurementMarkerGradient.Evaluate(m_value));
        }
        #endregion
    }
}

