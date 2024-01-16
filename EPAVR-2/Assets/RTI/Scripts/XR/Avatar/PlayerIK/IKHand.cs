using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class IKHand : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Transform m_followTarget;
        [SerializeField] private Vector3 m_positionOffset;
        [SerializeField] private Vector3 m_rotationOffset;
        #endregion
        #region Public Properties
        public bool Active { get; set; } = false;
        #endregion

        #region Initialization
        public void Init()
        {
            // Set active
            Active = true;
        }
        #endregion

        void LateUpdate()
        {
            // Make sure solver is active
            if (!Active) return;

            // Configure follow target
            transform.position = m_followTarget.TransformPoint(m_positionOffset);
            transform.rotation = m_followTarget.rotation * Quaternion.Euler(m_rotationOffset);
        }
    }
}

