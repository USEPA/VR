using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class UserMeasurementReport : MonoBehaviour
    {
        #region Inspector Asssigned Variables
        [Header("Important References")]
        [SerializeField] TextMeshProUGUI m_title;
        [SerializeField] TextMeshProUGUI m_recording;
        #endregion
        #region Protected Variables
        protected UserMeasurement m_measurement;
        protected float m_timeStamp;
        protected float m_value;
        #endregion
        #region Public Properties
        public float TimeStamp 
        { 
            get => m_timeStamp; 
            set 
            {
                m_timeStamp = value;
                TimeSpan time = TimeSpan.FromSeconds(value);
                m_title.text = time.ToString(@"hh\:mm\:ss");
            }
        }
        public float Value 
        {
            get => m_value;
            set
            {
                m_value = value;
                m_recording.text = $"{m_value.ToString("0.00")} R/h";
            }
        }
        #endregion

        #region Initialization
        public void Init(UserMeasurement _measurement)
        {
            m_measurement = _measurement;
            Value = m_measurement.Value;
            // Cache values
            TimeStamp = m_measurement.TimeStamp;
        }
        public void Init(float _time, float _value)
        {
            Init(new UserMeasurement(_time, _value));

            TimeStamp = _time;
            Value = _value;
        }
        #endregion
    }

    public struct UserMeasurement
    {
        public float Value;
        public float TimeStamp;

        public UserMeasurement(float _value, float _timeStamp)
        {
            Value = _value;
            TimeStamp = _timeStamp;
        }
    }
}

