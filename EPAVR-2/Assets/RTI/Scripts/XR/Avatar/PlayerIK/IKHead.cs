using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class IKHead : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Transform m_root;
        [SerializeField] private Transform m_followTarget;
        [SerializeField] private Vector3 m_positionOffset;
        [SerializeField] private Vector3 m_rotationOffset;
        [SerializeField] private Vector3 m_headBodyOffset;
        [Header("Default Configuration")]
        [SerializeField] private bool m_rootFollowPosition = true;
        [SerializeField] private bool m_rootFollowRotation = true;
        [Header("Debug")]
        public Vector3 followUpVector;
        public Vector3 targetForward;
        #endregion
        #region Private Variables
        #endregion
        #region Public Properties
        public bool Active { get; set; } = false;
        #endregion

        #region Initialization
        public void Init()
        {
            // Set up offset
            //m_headBodyOffset = m_root.position - m_followTarget.position;
            // Set active
            Active = true;

            // Do an initial tick
            //Tick();
        }
        #endregion

        void LateUpdate()
        {
            // Make sure solver is active
            if (!Active) return;

            // Update solver
            Tick();
        }

        public void Tick()
        {
            // Configure root object
            if (m_rootFollowPosition) m_root.position = transform.position + m_headBodyOffset;
            followUpVector = m_followTarget.up;
            targetForward = Vector3.ProjectOnPlane(m_followTarget.up, Vector3.up);
            if (m_rootFollowRotation && targetForward != Vector3.zero) m_root.forward = targetForward;
            // Configure follow target
            transform.position = m_followTarget.TransformPoint(m_positionOffset);
            transform.rotation = m_followTarget.rotation * Quaternion.Euler(m_rotationOffset);
        }
    }


}

