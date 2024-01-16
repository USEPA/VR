using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class DeliveryBoxGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] private List<DeviceGUIState> m_stateObjects;
        #endregion
        #region Private Variables
        private DeliveryBox m_parent;
        public DeviceGUIState m_currentStateObject;
        #endregion

        #region Initialization
        public void Init(DeliveryBox _parent)
        {
            // Cache references
            m_parent = _parent;
            // Disable all states
            for (int i = 0; i < m_stateObjects.Count; i++)
            {
                m_stateObjects[i].Init();
                m_stateObjects[i].gameObject.SetActive(false);
            }
            // Set the default state
            SetState(0);
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion

        #region State-Related Functionality
        public virtual void SetState(int _stateIndex)
        {
            if (m_currentStateObject != null) m_currentStateObject.OnExit();
            if (_stateIndex < 0 || _stateIndex > m_stateObjects.Count)
            {
                m_currentStateObject = m_stateObjects[0];
            }
            else
            {
                m_currentStateObject = m_stateObjects[_stateIndex];
                //m_currentStateObject = m_stateObjects[((int)_mode.Type) + 1];
            }
            m_currentStateObject.OnEnter();
        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }
    }
}

