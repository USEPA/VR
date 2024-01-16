using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    public class DoubleTextDisplay : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected TextMeshProUGUI m_mainText;
        [SerializeField] protected TextMeshProUGUI m_secondaryText;
        #endregion

        #region Display Functionality
        // Do the thing
        public void SetText(string _mainText, string _secondaryText)
        {
            // Set both text values
            SetMainValue(_mainText);
            SetSecondaryValue(_secondaryText);
        }

        public void SetMainValue(string _text)
        {
            m_mainText.text = _text;
        }
        public void SetSecondaryValue(string _text)
        {
            m_secondaryText.text = _text;
        }
        #endregion
    }
}

