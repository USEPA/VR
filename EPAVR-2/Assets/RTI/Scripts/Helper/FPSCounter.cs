using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class FPSCounter : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Events")]
        [SerializeField] private UnityEvent<float> m_onUpdateFPS;
        [Header("Default Configuration")]
        [SerializeField] private float m_updateInterval = 1.0f;
        #endregion
        #region Private Variables
        private int m_frameCount = 0;

        private float m_updateTimer;
        private float m_deltaTime;
        private float m_fps;
        #endregion
        #region Public Properties
        public float FPS
        {
            get => m_fps;
            private set
            {
                m_fps = value;
                m_onUpdateFPS?.Invoke(m_fps);
            }
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            // Set update interval
            m_updateTimer = m_updateInterval;
        }

        private void OnEnable()
        {
            // Set update interval
            m_updateTimer = m_updateInterval;
        }

        private void OnDisable()
        {
            // Set update interval
            m_updateTimer = m_updateInterval;
            // Reset numbers
            m_deltaTime = 0.0f;
            m_frameCount = 0;
            m_fps = 0.0f;
        }
        // Update is called once per frame
        void Update()
        {
            // Update timer
            m_updateTimer -= Time.deltaTime;
            m_deltaTime += Time.timeScale / Time.deltaTime;
            m_frameCount++;

            if (m_updateTimer <= 0.0f)
            {
                // Calculate FPS
                FPS = (m_deltaTime / m_frameCount);
                m_updateTimer = m_updateInterval;
                m_deltaTime = 0.0f;
                m_frameCount = 0;
            }
        }
    }
}

