using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class ConfigSlider : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected Slider m_slider;
        [SerializeField] protected TextMeshProUGUI m_valueDisplay;
        #endregion
        #region Protected Variables
        protected float m_value;
        protected Action<float> m_onValueChanged;
        #endregion
        #region Public Properties
        public float Value
        {
            get => m_value;
            set
            {
                m_value = value;
                m_onValueChanged?.Invoke(value);
            }
        }
        public Action<float> OnValueChanged { get => m_onValueChanged; set => m_onValueChanged = value; }
        #endregion

        #region Initialization
        public virtual void Init(float _minValue, float _maxValue, float _defaultValue)
        {
            // Hook up text display
            if (m_valueDisplay) m_onValueChanged += i => SetDisplayText(i);
            // Set min and max value of slider
            m_slider.minValue = _minValue;
            m_slider.maxValue = _maxValue;
            // Set default value
            m_slider.value = _defaultValue;
        }
        #endregion

        public void ForceSetValue(float _value)
        {
            m_slider.value = _value;
        }

        public virtual void SetDisplayText(float _value)
        {
            m_valueDisplay.text = _value.ToString("0.000");
        }
        /*
        private void OnEnable()
        {
            // Hook up text display
            if (m_valueDisplay) m_onValueChanged += i => m_valueDisplay.text = i.ToString("0.000");
        }

        private void OnDisable()
        {
            // Hook up text display
            if (m_valueDisplay) m_onValueChanged += i => m_valueDisplay.text = i.ToString("0.000");
        }
        */
    }
}

