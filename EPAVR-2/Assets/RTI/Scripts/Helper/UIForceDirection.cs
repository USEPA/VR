using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class UIForceDirection : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Configuration")]
        [SerializeField] float m_forcedZRotation = 0f;
        #endregion
        #region Private Variables
        private RectTransform m_rectTransform;
        private Vector3 m_rotation = Vector3.zero;
        #endregion

        private void Awake()
        {
            // Cache RectTransform component and initialize default rotation
            m_rotation.z = m_forcedZRotation;
            m_rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            // Set rotation Z value
            m_rotation.z = m_forcedZRotation;

            // Make sure RectTransform reflects this rotation
            if (m_rectTransform.eulerAngles != m_rotation) m_rectTransform.eulerAngles = m_rotation;
        }
    }
}

