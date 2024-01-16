using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public abstract class MX908Tool : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] protected MX908 m_debugParentReference;
        #endregion
        #region Protected Variables
        protected MX908 m_parent;
        protected bool m_isHeld = false;
        protected bool m_atAttachPoint = false;

        protected Action m_onApplied;
        protected Action m_onRemoved;

        protected Action m_onPointerEnter;
        protected Action m_onPointerExit;

        protected Action m_onSelected;
        protected Action m_onDeselected;

        #endregion
        #region Public Properties
        public abstract MXMode Mode { get; }

        public bool IsHeld { get => m_isHeld; }
        public bool AtAttachPoint { get => m_atAttachPoint; }
        
        public Action OnSelected { get => m_onSelected; set => m_onSelected = value; }
        public Action OnDeselected { get => m_onDeselected; set => m_onDeselected = value; }
        #endregion

        #region Initialization
        public virtual void Init(MX908 _parent)
        {
            m_parent = _parent;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (m_debugParentReference) Init(m_debugParentReference);
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Overridable Tool Functions
        public virtual void OnApplied()
        {
            m_onApplied?.Invoke();
        }
        public virtual void OnUpdate()
        {
            // Do thing
        }
        public virtual void OnRemoved()
        {
            m_onRemoved?.Invoke();
        }
        #endregion

        #region XR Interaction Functionality
        public virtual void OnPointerEnter(HoverEnterEventArgs args)
        {
            m_onPointerEnter?.Invoke();
        }

        public virtual void OnPointerExit(HoverExitEventArgs args)
        {
            m_onPointerExit?.Invoke();
        }

        public virtual void OnSelectEnter(SelectEnterEventArgs args)
        {
            m_isHeld = true;
            m_onSelected?.Invoke();
        }

        public virtual void OnSelectExit(SelectExitEventArgs args)
        {
            m_isHeld = false;
            m_onDeselected?.Invoke();
            // Check if this object is at the attach point
            if (m_atAttachPoint && m_parent != null)
            {
                UnityEngine.Debug.Log($"{gameObject.name} at attach point, connecting to device || Time: {Time.time}");
                m_parent.AttachTool(this);
            }
                
            else
            {
                UnityEngine.Debug.Log($"{gameObject.name} not at attach point, snapping back to belt || Time: {Time.time}");
                if (transform.parent != m_parent.AttachCollider.transform) GetComponent<XRToolbeltItem>().ForceAttachToSocket();
            }
                
        }
        #endregion

        #region Collision-Related Functionality
        public virtual void OnTriggerStay(Collider other)
        {
            if (m_parent != null && other == m_parent.AttachCollider && !m_atAttachPoint) m_atAttachPoint = true;
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (m_parent != null && other == m_parent.AttachCollider && m_atAttachPoint)
            {
                m_atAttachPoint = false;
                if (m_isHeld && m_parent.Tool == this) m_parent.RemoveTool();
            }
        }
        #endregion

        #region Helper Methods
        public void ForceAttachToDevice()
        {
            if (m_parent == null) return;
            // Remove the current tool
            if (m_parent.Tool != null) m_parent.RemoveTool();
            // Force attach the tool
            m_parent.AttachTool(this);
        }
        #if UNITY_EDITOR
        [ContextMenu("Force Attach to MX908")]
        public void ForceAttach()
        {
            ForceAttach();
        }
        #endif
        #endregion
    }
}

