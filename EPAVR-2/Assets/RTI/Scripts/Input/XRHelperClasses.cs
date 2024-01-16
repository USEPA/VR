using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace L58.EPAVR
{
    [Serializable]
    public class XRActionMapReference
    {
        #region Inspector Assigned Variables
        [SerializeField] XRActionMap m_actionMap;
        [SerializeField] List<XRInputReference> m_actions;
        #endregion
        #region Public Properties
        public XRActionMap ActionMap { get => m_actionMap; }
        public List<XRInputReference> Actions { get => m_actions; }
        #endregion
    }
    [Serializable]
    public class XRInputReference
    {
        #region Inspector Assigned Variables
        public XRAction m_action;
        public InputActionReference m_actionReference;
        #endregion
        #region Public Properties
        public XRAction Action { get => m_action; }
        public InputActionReference Reference { get => m_actionReference; }
        #endregion
    }
}

