using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class GeminiGUI : DeviceGUI<Gemini>
    {
        #region Inspector Assigned Variables
        [Header("Individual UI Component References")]
        [SerializeField] TextMeshProUGUI m_globalTitle;
        [SerializeField] Button m_homeButton;
        [SerializeField] List<Image> m_statusImages;
        #endregion
        #region Private Variables
        private Dictionary<GModeType, DeviceGUIState> m_modeUIRefs;
        #endregion
        #region Public Properties
        public Dictionary<GModeType, DeviceGUIState> Modes { get => m_modeUIRefs; }
        #endregion

        #region Initialization
        public override void Init(Gemini _parent)
        {
            // Cache references
            m_parent = _parent;
            // Hook up state change event
            m_parent.OnStateChange += i => SetState(i);
            // Initialize device state dictionary
            m_modeUIRefs = new Dictionary<GModeType, DeviceGUIState>();
            // Disable all state objects by default
            for(int i = 0; i < m_stateObjects.Count; i++) 
            {
                DeviceGUIState state = m_stateObjects[i];
                if ((i-1) >= 0)
                {
                    // Add this to the dictionary
                    m_modeUIRefs.Add((GModeType)(i - 1), state);
                }
                state.Init();
                state.gameObject.SetActive(false);
            }
            // Hook up events
            m_homeButton.onClick.AddListener(m_parent.ResetMode);
            // Initialize in default state
            SetState(null);
            for (int i = 0; i < m_statusImages.Count; i++) SetButtonStatusActive(i, false);
        }
        #endregion

        #region State-Related Functionality
        public void SetState(GeminiState _mode)
        {
            if (m_currentStateObject != null) m_currentStateObject.OnExit();
            if (_mode == null) 
            {
                m_currentStateObject = m_stateObjects[0];
                m_globalTitle.text = "Scan";
            }
            else
            {
                m_globalTitle.text = _mode.Title;
                m_currentStateObject = m_modeUIRefs[_mode.Type];
                //m_currentStateObject = m_stateObjects[((int)_mode.Type) + 1];
            }
            m_currentStateObject.OnEnter();
            //m_currentStateObject.gameObject.SetActive(true);
        }
        #endregion

        #region Status-Related Functionality
        public void SetButtonStatusActive(int _index, bool _active)
        {
            if (m_statusImages == null || m_statusImages.Count < 1 || _index < 0 || _index >= m_statusImages.Count) return;

            m_statusImages[_index].color = (_active) ? new Color(255, 255, 255, 1) : new Color(255, 255, 255, 0.25f);
        }
        #endregion
    }
}

