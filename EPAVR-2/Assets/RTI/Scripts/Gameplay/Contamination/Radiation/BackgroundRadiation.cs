using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class BackgroundRadiation : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private float m_tickInterval = 0.5f;
        [SerializeField] private Vector2 m_backgroundRadiationRange = new Vector2(0.0f, 0.02f);
        #endregion
        #region Private Variables
        private float m_currentBackgroundRadiationLevel = 0.0f;
        private float m_updateTimer = 0.0f;
        #endregion
        #region Public Properties
        public static BackgroundRadiation Instance { get; private set; }

        public float CurrentLevel
        {
            get => m_currentBackgroundRadiationLevel;
            private set
            {
                m_currentBackgroundRadiationLevel = value;
            }
        }
        #endregion

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Init(float _minBackgroundRadiation = 0.0f, float _maxBackgroundRadiation = 0.02f)
        {
            // Set range of background radiation
            m_backgroundRadiationRange = new Vector2(_minBackgroundRadiation, _maxBackgroundRadiation);
        }

        // Update is called once per frame
        void Update()
        {
            // Get the level of background radiation
            float rawValue = UnityEngine.Random.Range(m_backgroundRadiationRange.x, m_backgroundRadiationRange.y);
            CurrentLevel = ((rawValue < (m_backgroundRadiationRange.y * 0.5f))) ? 0.0f : rawValue;
            /*
            m_updateTimer += Time.deltaTime;
            // Check if it is time to update again
            if (m_updateTimer >= m_tickInterval)
            {
                // Get the level of background radiation
                float rawValue = UnityEngine.Random.Range(m_backgroundRadiationRange.x, m_backgroundRadiationRange.y);
                CurrentLevel = rawValue;
                //CurrentLevel = (float) System.Math.Round(rawValue, 2);
                m_updateTimer = 0.0f;
            }
            */

        }
    }
}

