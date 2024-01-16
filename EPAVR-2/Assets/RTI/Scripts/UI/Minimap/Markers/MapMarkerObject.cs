using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [RequireComponent(typeof(Renderer))]
    public class MapMarkerObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected float m_yOffset = 10.0f;
        #endregion
        #region Protected Variables
        protected Renderer m_renderer;
        protected Transform m_parent;

        protected bool m_isDynamic = false;
        #endregion
        #region Public Properties
        public virtual Renderer Renderer 
        { 
            get 
            { 
                if (!m_renderer) m_renderer = GetComponent<Renderer>();
                return m_renderer;
            } 
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Cache component
            m_renderer = GetComponent<Renderer>();
        }

        public virtual void Init(Transform _parent, Color _color, bool _isDynamic = false)
        {
            // Assign reference
            m_parent = _parent;
            // Set color
            SetColor(_color);
            // Configure whether or not this object should dynamically update
            m_isDynamic = _isDynamic;

            // Set the object's position/rotation based on the parent
            UpdateTransform();
        }
        #endregion

        private void FixedUpdate()
        {
            // Make sure this is a dynamic object
            if (!m_isDynamic) return;

            // Update the transform according to parent
            UpdateTransform();
        }

        public virtual void UpdateTransform()
        {
            // Set position/rotation according to parent
            transform.position = m_parent.transform.position + (Vector3.up * m_yOffset);
            transform.rotation = m_parent.transform.rotation;
        }

        protected virtual void SetColor(Color _color)
        {
            Renderer.material.color = _color;
        }
    }
}

