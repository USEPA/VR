using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SliderValueDisplay : MonoBehaviour
    {
        #region Protected Variables
        protected TextMeshProUGUI m_text;
        #endregion
        #region Public Properties
        public TextMeshProUGUI Text
        {
            get
            {
                if (!m_text) m_text = GetComponent<TextMeshProUGUI>();
                return m_text;
            }
        }
        #endregion

        public void UpdateValue(float _value)
        {
            float value = Mathf.Clamp(_value * 100.0f, 0.0f, 100.0f);
            string text = (value != 100.0f) ? value.ToString("0.0") : "100";
            Text.text = $"{text}%";
        }
    }
}

