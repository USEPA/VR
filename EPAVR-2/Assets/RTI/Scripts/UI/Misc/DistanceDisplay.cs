using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    public class DistanceDisplay : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private bool m_startActive = false;
        #endregion
        #region Private Variables
        private TextMeshProUGUI m_distanceDisplay;
        private RadSurveyScenarioInstance m_radScenarioInstance;

        private bool m_isActive = false;
        #endregion
        #region Public Properties
        public bool Active
        {
            get => m_isActive;
            set
            {
                m_isActive = value;
                gameObject.SetActive(value);
                //TextDisplay.enabled = value;
            }
        }

        public TextMeshProUGUI TextDisplay
        {
            get
            {
                if (!m_distanceDisplay) m_distanceDisplay = GetComponent<TextMeshProUGUI>();
                return m_distanceDisplay;
            }
        }
        #endregion



        // Start is called before the first frame update
        void Start()
        {
            // Cache reference
            m_distanceDisplay = GetComponent<TextMeshProUGUI>();
            if (ScenarioManager.Instance)
            {
                if (ScenarioManager.Instance.CurrentScenarioInstance != null && ScenarioManager.Instance.CurrentScenarioInstance is RadSurveyScenarioInstance radScenarioInstance)
                {
                    // Cache reference
                    m_radScenarioInstance = radScenarioInstance;
                    // Hook up event
                    m_radScenarioInstance.OnEpicenterEstimateDistanceChanged.AddListener(UpdateDistance);
                    Active = m_startActive;
                }
                else
                {
                    Active = false;
                }
            }
        }

        public void UpdateDistance(float _distance)
        {
            if (!m_isActive && m_radScenarioInstance.IsEpicenterMarkerPlaced) Active = true;
            m_distanceDisplay.text = $"{_distance.ToString("0.00")} m";
        }
    }
}

