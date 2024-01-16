using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public abstract class DeviceGUI<T> : MonoBehaviour where T : SampleTool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected List<DeviceGUIState> m_stateObjects;
        #endregion
        #region Protected Variables
        protected T m_parent;
        public DeviceGUIState m_currentStateObject;
        #endregion
        #region Public Properties
        #endregion

        #region Initialization
        public abstract void Init(T _parent);
        #endregion

        #region State-Related Functionality
        public virtual void SetState(int _stateIndex)
        {
            // Do the thing
        }
        #endregion
    }
}

