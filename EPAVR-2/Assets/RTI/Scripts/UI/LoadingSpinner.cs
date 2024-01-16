using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class LoadingSpinner : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] private float m_rotateSpeed = 200.0f;
        #endregion
        #region Private Variables
        private RectTransform m_rectTransform;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache component
            TryGetComponent<RectTransform>(out m_rectTransform);
        }

        // Update is called once per frame
        void Update()
        {
            m_rectTransform.Rotate(0, 0, m_rotateSpeed * Time.deltaTime);
        }
    }
}

