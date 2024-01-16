using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class MapMarkerUIObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected Vector3 m_rotationalOffset = Vector3.zero;
        #endregion
        #region Protected Variables
        protected MapUI m_parent;
        protected Transform m_source;
        protected RectTransform m_rectTransform;
        protected Image m_image;

        protected bool m_isDynamic = false;
        protected bool m_trackRotation = false;

        protected Vector2 m_imagePosition;
        protected float m_sourceAngle;
        #endregion
        #region Public Properties
        public Transform Source { get => m_source; }

        public virtual RectTransform RectTransform
        {
            get
            {
                if (!m_rectTransform) m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public virtual Image Image
        {
            get
            {
                if (!m_image) m_image = GetComponent<Image>();
                return m_image;
            }
        }

        public Vector2 ImagePosition
        {
            get => m_imagePosition;
        }
        #endregion

        #region Initialization
        public virtual void Init(MapUI _parent, Transform _sourceTransform, bool _isDynamic = false, bool _trackRotation = false)
        {
            // Cache references
            m_parent = _parent;
            m_source = _sourceTransform;
            // Configure update mode
            m_isDynamic = _isDynamic;
            if (m_isDynamic) m_trackRotation = _trackRotation;
            // Update transform according to parent map UI
            UpdateTransform();
        }

        public virtual void Init(MapUI _parent, Vector3 _sourcePosition)
        {
            // Cache references
            m_parent = _parent;
            // Set defaults
            m_isDynamic = false;

            // Set the marker position according to the inputted world position
            UpdatePosition(_sourcePosition);
        }
        #endregion

        #region Display-Related Functionality
        public virtual void UpdateTransform()
        {
            // Update position
            UpdatePosition(m_source.transform.position);
            // Set image rotation if necessary
            if (m_trackRotation) RectTransform.localEulerAngles = CalculateRotation(m_source.transform.eulerAngles);
        }

        public virtual void UpdatePosition(Vector3 _position)
        {
            // Get bound position
            m_imagePosition = m_parent.GetImagePositionFromWorld(_position);
            // Set image position
            RectTransform.localPosition = new Vector3(m_imagePosition.x, m_imagePosition.y, RectTransform.localPosition.z);
        }
        public virtual void SetScale(float _scaleFactor = 1.0f)
        {
            RectTransform.localScale = new Vector3(_scaleFactor, _scaleFactor, RectTransform.localScale.z);
        }

        public virtual void SetColor(Color _color)
        {
            Image.color = _color;
        }
        #endregion

        #region Helper Methods
        protected Vector3 CalculateRotation(Vector3 _worldEulerAngles)
        {
            // Get Y rotation
            float sourceAngle = -_worldEulerAngles.y;
            m_sourceAngle = sourceAngle;
            // Calculate rotation
            return (new Vector3(0, 0, sourceAngle)) + m_rotationalOffset;
        }
        #endregion
    }
}

