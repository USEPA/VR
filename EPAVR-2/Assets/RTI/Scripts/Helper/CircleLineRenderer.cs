using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleLineRenderer : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected float m_radius = 0.1f;
        [SerializeField] protected float m_lineWidth = 0.01f;
        #endregion
        #region Protected Variables
        protected LineRenderer line;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache line renderer reference
            line = GetComponent<LineRenderer>();
        }

        public void DrawCircle()
        {
            if (TryGetComponent<LineRenderer>(out line))
            {
                var segments = 360;
                line.useWorldSpace = false;
                line.startWidth = m_lineWidth;
                line.endWidth = m_lineWidth;
                line.positionCount = segments + 1;

                var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
                var points = new Vector3[pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    var rad = Mathf.Deg2Rad * (i * 360f / segments);
                    points[i] = new Vector3(Mathf.Sin(rad) * m_radius, 0, Mathf.Cos(rad) * m_radius);
                }

                line.SetPositions(points);
            }
        }

        public void SetColorGradient(Gradient _gradient)
        {
            if (!line) return;
            line.colorGradient = _gradient;
        }

        #region Helper Methods
        #if UNITY_EDITOR
        [ContextMenu("Force Draw Circle")]
        public void ForceDrawCircle()
        {
            DrawCircle();
        }
        #endif
        #endregion
    }
}

