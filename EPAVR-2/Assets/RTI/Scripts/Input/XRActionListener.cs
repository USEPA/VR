using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class XRActionListener : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField]
        protected InputActionReference m_ActionReference;
        [SerializeField]
        protected UnityEvent m_onPerformAction;
        #endregion
        #region Public Properties
        public InputActionReference ActionReference { get => m_ActionReference; set => m_ActionReference = value; }
        #endregion

        #region Enable/Disable Functionality
        private void OnEnable()
        {
            m_ActionReference.action.Enable();
            m_ActionReference.action.started += OnActionStarted;
            m_ActionReference.action.canceled += OnActionCanceled;
            m_ActionReference.action.performed += OnActionPerformed;
        }

        private void OnDisable()
        {
            m_ActionReference.action.Disable();
            m_ActionReference.action.started -= OnActionStarted;
            m_ActionReference.action.canceled -= OnActionCanceled;
            m_ActionReference.action.performed -= OnActionPerformed;
        }
        #endregion

        #region Processors
        public virtual void OnActionStarted(InputAction.CallbackContext _context)
        {
            // Do the thing
        }
        public virtual void OnActionPerformed(InputAction.CallbackContext _context)
        {
            //UnityEngine.Debug.Log($"{gameObject.name} - action performed || Time: {Time.time}");
            // Invoke any necessary events
            m_onPerformAction?.Invoke();
        }

        public virtual void OnActionCanceled(InputAction.CallbackContext _context)
        {
            // Do the thing
        }
        #endregion

        private void OnDestroy()
        {
            OnDisable();
        }
    }
}

