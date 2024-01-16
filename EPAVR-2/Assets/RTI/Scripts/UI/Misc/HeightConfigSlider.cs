using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class HeightConfigSlider : ConfigSlider
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private bool m_useMeters = false;
        #endregion
        public override void SetDisplayText(float _value)
        {
            //string metersDisplay = $"{_value.ToString("0.000")} m";
            //string feetDisplay = $"{(MathHelper.ConvertMetersToFeet(_value)).ToString("0.000")} ft";
            //float adjustedValue = _value + VRUserManager.Instance.DefaultHeightCalibrateOffset;
            float adjustedValue = _value;
            string text = (m_useMeters) ? $"{adjustedValue.ToString("0.000")} m" : $"{(MathHelper.ConvertMetersToFeet(adjustedValue)).ToString("0.000")} ft";
            m_valueDisplay.text = text;
        }
    }
}

