using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SimpleDisplayHook : MonoBehaviour
{
    #region Inspector Assigned Variables
    [Header("Default Configuration")]
    [SerializeField] protected string m_leadingText = "";
    #endregion
    #region Protected Variables
    protected TextMeshProUGUI m_displayText;
    #endregion
    #region Public Properties
    public TextMeshProUGUI DisplayText
    {
        get
        {
            if (!m_displayText) m_displayText = GetComponent<TextMeshProUGUI>();
            return m_displayText;
        }
    }
    #endregion

    public void SetText(string _value)
    {
        DisplayText.text = $"{m_leadingText}{_value}";
    }

    public void SetText(float _value)
    {
        SetText(_value.ToString("0.00"));
    }

    public void SetText(int _value)
    {
        SetText(_value.ToString());
    }

    public void SetText(bool _value)
    {
        SetText(_value.ToString());
    }
}
