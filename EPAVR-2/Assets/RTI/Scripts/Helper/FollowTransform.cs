using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class FollowTransform : MonoBehaviour
    {
        #region Protected Variables
        protected Transform m_followTarget;
        #endregion
        #region Public Properties
        public Transform FollowTarget { get => m_followTarget; set => m_followTarget = value; }
        #endregion

        private void LateUpdate()
        {
            // Check if there is a follow target
            if (!m_followTarget) return;
            // Match the transform to the follow target
            MatchTransform(m_followTarget);
        }

        #region Helper Methods
        public void MatchTransform(Transform _target)
        {
            // Set position/rotation
            transform.position = _target.position;
            transform.rotation = _target.rotation;
        }

        public void ResetTransform()
        {
            // Reset local position/rotation
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
        #endregion
    }
}

