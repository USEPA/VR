using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    public class GlobalTimerTest : MonoBehaviour
    {
        public float m_timer = 0.0f;
        #region Inspector Assigned Variables
        [SerializeField] TextMeshProUGUI m_displayText;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

            if (ScenarioManager.Instance)
            {
                ScenarioManager.Instance.CurrentScenarioInstance.OnTimerTick.AddListener(UpdateTimer);
                //ScenarioManager.Instance.OnGlobalScenarioTimerTick += i => UpdateTimer(i);
            }
            else if (RadScenarioTest.Instance)
            {
                RadScenarioTest.Instance.OnGlobalScenarioTimerTick += i => UpdateTimer(i);
            }
        }

        public void UpdateTimer(float value)
        {
            m_timer = value;
            // Convert the time into a readable format
            TimeSpan time = TimeSpan.FromSeconds(value);
            m_displayText.text = time.ToString(@"hh\:mm\:ss");
            //m_displayText.text = $"{Mathf.Floor(value / 60).ToString("00")}:{Mathf.Floor(value % 60).ToString("00")}";
        }

        private void OnDestroy()
        {
            /*
            if (ScenarioManager.Instance && ScenarioManager.Instance.CurrentScenarioInstance != null)
            {
                ScenarioManager.Instance.CurrentScenarioInstance.OnTimerTick.RemoveListener(UpdateTimer);
                //ScenarioManager.Instance.OnGlobalScenarioTimerTick -= i => UpdateTimer(i);
            }
            else if (RadScenarioTest.Instance)
            {
                RadScenarioTest.Instance.OnGlobalScenarioTimerTick -= i => UpdateTimer(i);
            }
            */
        }
    }
}

