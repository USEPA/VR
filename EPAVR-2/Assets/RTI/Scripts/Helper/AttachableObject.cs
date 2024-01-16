using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class AttachableObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected bool m_attachAtStart = true;
        #endregion
        #region Protected Variables
        protected ObjectAttachPoint m_attachPoint;
        #endregion
        #region Public Properties
        public ObjectAttachPoint Parent { get => m_attachPoint; }
        #endregion

        #region Initialization
        public void Init(ObjectAttachPoint _parent)
        {
            m_attachPoint = _parent;
            if (m_attachAtStart) m_attachPoint.AttachObject(gameObject);
        }
        #endregion

        #region Helper Methods
        public void SnapToParent()
        {
            if (!m_attachPoint) return;
            m_attachPoint.AttachObject(gameObject);
        }
        #endregion
    }
}

