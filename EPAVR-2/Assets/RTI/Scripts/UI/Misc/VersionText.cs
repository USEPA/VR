using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionText : MonoBehaviour
    {
        #region Private Variables
        private TextMeshProUGUI m_versionDisplay;
        #endregion

        #region Initialization
        private void Awake()
        {
            // Cache component
            m_versionDisplay = GetComponent<TextMeshProUGUI>();
        }
        // Start is called before the first frame update
        void Start()
        {
            if (!CoreGameManager.Instance) return;
            // Automatically populate the text field with the current game version
            SetVersionText(CoreGameManager.Instance.Config.Version);
        }
        #endregion

        void SetVersionText(string _value)
        {
            m_versionDisplay.text = _value;
        }
    }
}

