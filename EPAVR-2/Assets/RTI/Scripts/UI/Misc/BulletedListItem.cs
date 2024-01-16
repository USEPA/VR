using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BulletedListItem : MonoBehaviour
    {
        #region Protected Variables
        protected TextMeshProUGUI m_text;
        protected int m_indentation = 1;
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

        #region Initialization
        public void Init(string text, int indentation = 1)
        {
            // Set indentation
            SetIndentation(indentation);
            // Set text
            SetText(text);
        }
        #endregion

        #region Display-Related Functionality
        public void SetText(string value)
        {
            Text.text = $"\u2022<indent={m_indentation}em>{value}</indent>";
        }

        public void SetIndentation(int value)
        {
            m_indentation = value;
        }
        #endregion
    }
}

