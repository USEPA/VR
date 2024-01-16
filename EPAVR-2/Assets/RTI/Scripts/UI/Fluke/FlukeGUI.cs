using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class FlukeGUI : DeviceGUI<Fluke>
    {
        #region Inspector Assigned Variables
        [SerializeField] MeshRenderer m_screenMesh;
        //[SerializeField] private MaterialSwapper m_screenMaterialControl;
        [Header("Default Configuration")]
        [SerializeField] private Color m_screenActiveColor;
        #endregion
        #region Private Variables
        FlukeMonitorGUI m_monitorState;
        #endregion
        #region Public Properties
        #endregion

        #region Initialization
        public override void Init(Fluke _parent)
        {
            // Cache parent reference
            m_parent = _parent;
            // Hook up active event
            m_parent.OnSetActive += i => SetActive(i);

            // Initialize all states
            if (m_stateObjects == null || m_stateObjects.Count < 1) return;
            foreach(DeviceGUIState state in m_stateObjects)
            {
                state.Init();
                state.gameObject.SetActive(false);
            }
            SetActive(false);
            //SetState(0);
        }
        #endregion

        // Update is called once per frame
        void Update()
        {
            // Do the thing
        }

        #region State-Related Functionality
        public override void SetState(int _stateIndex)
        {
            //UnityEngine.Debug.Log($"Fluke set state: {_stateIndex} || Time: {Time.time}");
            if (_stateIndex < 0 || _stateIndex > m_stateObjects.Count) return;
            if (m_currentStateObject != null) m_currentStateObject.OnExit();
            m_currentStateObject = m_stateObjects[_stateIndex];
            m_currentStateObject.OnEnter();
        }
        #endregion

        #region Helper Methods
        public void SetActive(bool _value)
        {
            //UnityEngine.Debug.Log($"Fluke GUI Set Active: {_value} || Time: {Time.time}");
            //m_screenMaterialControl.SetActive(_value);
            SetState((_value == true) ? 1 : 0);
            MeshHelper.SetMeshEmissionColor(m_screenMesh, (_value) ? m_screenActiveColor : Color.black);
        }
        #endregion
    }
}

