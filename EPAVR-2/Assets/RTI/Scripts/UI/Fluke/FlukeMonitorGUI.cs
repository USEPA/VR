using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{

    public class FlukeMonitorGUI : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [SerializeField] private TextMeshProUGUI m_valueDisplay;
        [SerializeField] private Slider m_barGraph;
        [Header("Default Configuration")]
        [SerializeField] Vector2 m_valueRange = new Vector2(0, 250.0f);
        #endregion

        #region Graph-Related Functionality
        public void UpdateGraph(float _currentValue)
        {
            m_valueDisplay.text = (_currentValue * m_valueRange.y).ToString("0.00");
            m_barGraph.value = _currentValue;
        }
        #endregion
    }
}

