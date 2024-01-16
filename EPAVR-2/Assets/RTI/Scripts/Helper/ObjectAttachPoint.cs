using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ObjectAttachPoint : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] protected AttachableObject m_defaultObject;
        #endregion
        #region Private Variables
        private Collider m_collider;
        private GameObject m_attachedObject;
        #endregion
        #region Public Properties
        public Collider Collider { get => m_collider; }
        public GameObject AttachedObject { get => m_attachedObject; }
        #endregion

        private void Awake()
        {
            // Cache collider reference
            m_collider = GetComponent<Collider>();
            // Check if there is a default object
            if (m_defaultObject != null) m_defaultObject.Init(this);
        }

        #region Attach-Related Functionality
        public void AttachObject(GameObject _object)
        {
            // Cache reference
            m_attachedObject = _object;
            // Parent the object and zero out its transform values
            m_attachedObject.transform.parent = transform;
            m_attachedObject.transform.localPosition = Vector3.zero;
            m_attachedObject.transform.localEulerAngles = Vector3.zero;
        }
        #endregion
    }
}

