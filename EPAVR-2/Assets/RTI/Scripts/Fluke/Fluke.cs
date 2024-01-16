using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Fluke : SampleTool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private FlukeGUI m_gui;
        [SerializeField] private RadSensor m_sensor;
        [SerializeField] private MeshRenderer m_screenMesh;
        [Header("Default Configuration")]
        [SerializeField] private bool m_startActive = false;
        [SerializeField] private float m_sensorTickInterval = 0.05f;

        public float m_radLevel = 0.0f;

        #endregion
        #region Private Variables
        private bool m_isActive = false;

        private System.Action<bool> m_onSetActive;
        #endregion
        #region Public Properties
        public override ToolType Type => ToolType.Fluke;

        public override float CurrentReading => m_sensor.CurrentReading;
        public override float MaxReading => m_sensor.MaxReading;

        public RadSensor Sensor { get => m_sensor; }

        public System.Action<bool> OnSetActive { get => m_onSetActive; set => m_onSetActive = value; }
        #endregion

        #region Initialization
        public override void Init()
        {
            // Initialize the rad sensor
            m_sensor.Init();
            m_sensor.TickInterval = m_sensorTickInterval;
            m_onSetActive += i => m_sensor.SetActive(i);
            // Initialize the GUI
            m_gui.Init(this);
            // Set whether or not this should be active
            SetActive(m_startActive);
        }
        #endregion


        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isActive) return;
            m_sensor.Tick();

            m_radLevel = CurrentReading;
        }

        #region Helper Methods
        public void SetActive(bool _value)
        {
            m_isActive = _value;
            m_onSetActive?.Invoke(_value);
        }

        public void ToggleActive()
        {
            SetActive(!m_isActive);
        }

        public override SampleReportOld GenerateReport(ScenarioStep _step)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        private void OnDestroy()
        {
            m_onSetActive = null;
        }

        #if UNITY_EDITOR
        [ContextMenu("Force Toggle Active")]
        public void ForceToggleActive()
        {
            ToggleActive();
        }
        #endif
    }
}

