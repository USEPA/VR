using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class FloatValueEventHandler : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] UnityEvent<float> m_onSetValue;
        #endregion
        #region Protected Variables
        public float m_value;
        #endregion

        public void SetValue(float value)
        {
            m_value = value;
            m_onSetValue?.Invoke(m_value);
        }
    }
}

