using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRToolItem : XRToolbeltItem
    {
        #region Protected Variables
        public bool m_isAtAttachPoint = false;
        #endregion
        #region Public Properties
        public bool IsAtAttachPoint { get => m_isAtAttachPoint; set => m_isAtAttachPoint = value; }
        public override bool IsAttached { get => (m_currentSocket != null && transform.parent == m_currentSocket.transform); }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Interaction-Related Functionality
        public void AttemptAttachToParent()
        {
            UnityEngine.Debug.Log($"{gameObject.name} arrived in AttemptAttachToParent | Is At Attach Point: {m_isAtAttachPoint} | Is Attached: {IsAttached} || Time: {Time.time}");
            if (!m_isAtAttachPoint || IsAttached) return;
            AttachToBelt();
        }

        public override void AttachToBelt()
        {
            //base.AttachToBelt();
            UnityEngine.Debug.Log($"{gameObject.name} arrived in AttachToBelt | Socket: {((m_currentSocket != null) ? m_currentSocket.gameObject.name : "N/A")} || Time: {Time.time}");
            if (m_currentSocket != null && m_currentSocket.CurrentItem == null) 
            {
                UnityEngine.Debug.Log($"{gameObject.name} about to attach | Socket: {m_currentSocket.gameObject.name} || Time: {Time.time}");
                AttachToTransform(m_currentSocket.transform);
                m_currentSocket.AttachItem(this);
            }
        }

        public override void AttemptRemove()
        {
            UnityEngine.Debug.Log($"{gameObject.name} arrived in AttemptRemove | Is Attached: {IsAttached} || Time: {Time.time}");
            //if (!IsAttached) return;

            if (m_currentSocket != null && m_currentSocket.CurrentItem != null && m_currentSocket.CurrentItem == this)
            {
                m_currentSocket.RemoveItem();
                UnityEngine.Debug.Log($"{gameObject.name} before - parent: {((transform.parent != null) ? transform.parent.gameObject.name : "N/A")} || Time: {Time.time}");
                transform.parent = null;
                UnityEngine.Debug.Log($"{gameObject.name} after - parent: {((transform.parent != null) ? transform.parent.gameObject.name : "N/A")} || Time: {Time.time}");


            }
        }
        #endregion
    }
}

