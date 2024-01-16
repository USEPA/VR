using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class Gemini : SampleTool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private GeminiGUI m_gui;
        [SerializeField] private List<GeminiState> m_modeRefs;
        #endregion
        #region Private Variables
        private GeminiState m_currentMode;
        private Dictionary<GModeType, GeminiState> m_modes;

        public GModeType m_currentModeType;

        private bool m_forcedInView = false;

        private Action<GeminiState> m_onStateChange;
        #endregion

        #region Public Properties
        public override ToolType Type => ToolType.Gemini;
        public GeminiGUI GUI { get => m_gui; }
        public GeminiState CurrentMode { get => m_currentMode; }
        public Action<GeminiState> OnStateChange { get => m_onStateChange; set => m_onStateChange = value; }
        #endregion

        #region Initialization
        public override void Init()
        {
            // Initialize mode dictionary
            m_modes = new Dictionary<GModeType, GeminiState>();
            // Initialize GUI
            m_gui.Init(this);
            // Loop through each mode and initialize it
            foreach (GeminiState mode in m_modeRefs) 
            {
                if (!m_modes.ContainsKey(mode.Type))
                {
                    m_modes.Add(mode.Type, mode);
                    mode.Init(this);
                }
            }
        }

        public override void OnSpawn()
        {
            // Do the thing
        }

        public override void OnDespawn()
        {
            // Do the thing
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            //Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_currentMode != null) m_currentMode.OnUpdate();
        }

        #region State-Related Functionality
        public void SetMode(GModeType _mode)
        {
            // Make sure conditions are valid
            if (m_modes == null || m_modes.Count < 1 || m_modes[_mode] == null) return;
            // Check if there is a current mode
            if (m_currentMode != null) m_currentMode.OnExit();
            // Assign reference
            m_currentMode = m_modes[_mode];
            m_currentMode.OnEnter();
            m_currentModeType = m_currentMode.Type;
            UnityEngine.Debug.Log($"Gemini - Set Mode: {m_currentMode.Type} || Time: {Time.time}");
            m_onStateChange?.Invoke(m_currentMode);
        }

        public void SetMode(int _modeIndex)
        {
            SetMode((GModeType)_modeIndex);
        }

        public void ResetMode()
        {
            if (m_currentMode != null) m_currentMode.OnExit();
            m_currentMode = null;
            m_onStateChange?.Invoke(m_currentMode);
        }
        #endregion

        #region Report-Related Functionality
        public override SampleReportOld GenerateReport(ScenarioStep _step)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper Methods
        public override void ForceToggleInView()
        {
            if (!m_forcedInView)
            {
                // Get headset position
                //Vector3 targetPosition = new Vector3(transform.position.x, Camera.main.transform.position.y - 0.1f, transform.position.z);
                Vector3 targetPosition = Camera.main.transform.position + (Camera.main.transform.forward * 0.35f);
                targetPosition.y -= 0.19f;

                transform.localPosition = transform.InverseTransformPoint(targetPosition);
                transform.parent = Camera.main.transform;
                transform.localEulerAngles = new Vector3(-60.0f, 0.0f, 0.0f);

                m_forcedInView = true;
            }
            else
            {
                GetComponent<XRToolbeltItem>().AttachToBelt();
                /*
                transform.parent = m_beltTransform;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                */

                m_forcedInView = false;
            }
        }
        #endregion
    }

    public enum GModeType { FTIR, Raman}
}

