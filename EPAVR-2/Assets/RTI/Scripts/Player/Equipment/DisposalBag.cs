using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class DisposalBag : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] TriggerEventListener m_disposalTriggerListener;
        [SerializeField] Animator m_anim;
        #endregion
        #region Private Variables
        private bool m_isOpen = false;

        private Action<bool> m_onSetOpen;
        #endregion
        #region Public Properties
        public Action<bool> OnSetOpen { get => m_onSetOpen; set => m_onSetOpen = value; }
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize disposal trigger listener
            m_disposalTriggerListener.Init(false);
            // Hook up events
            m_onSetOpen += i => m_anim.SetBool("isOpen", i);
            m_onSetOpen += i => m_disposalTriggerListener.Collider.enabled = i;
            m_onSetOpen += i => m_disposalTriggerListener.Active = i;
            m_disposalTriggerListener.OnTriggerStayed += i => ProcessTriggerStay(i);
            m_disposalTriggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            // Set initial state
            SetOpen(false);
        }
        #endregion

        #region Interaction-Related Functionality
        public void SetOpen(bool value)
        {
            // Set value
            m_isOpen = value;
            // Invoke any necessary events
            m_onSetOpen?.Invoke(m_isOpen);
        }

        public void DeleteObject(XRGrabInteractable _interactable)
        {
            // Get the IDisposable component
            if (_interactable.TryGetComponent<IDisposable>(out IDisposable disposable))
            {
                _interactable.selectExited.RemoveListener(i => DeleteObject(_interactable));
                disposable.Dispose();
                Destroy(_interactable.gameObject);
            }
            
            //Destroy(_interactable.gameObject);
        }
        #endregion

        #region Collision-Related Functionality
        public void ProcessTriggerStay(Collider other)
        {
            if (other.TryGetComponent<IDisposable>(out IDisposable disposable))
            {
                UnityEngine.Debug.Log($"{other.gameObject.name} entered disposal bag || Time: {Time.time}");
                // Hook up the select exited event
                disposable.Interactable.selectExited.AddListener(i => DeleteObject(disposable.Interactable));
            }
        }

        public void ProcessTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IDisposable>(out IDisposable disposable))
            {
                UnityEngine.Debug.Log($"{other.gameObject.name} exited disposal bag || Time: {Time.time}");
                // Remove the select exited event
                disposable.Interactable.selectExited.RemoveListener(i => DeleteObject(disposable.Interactable));
            }
        }
        #endregion

        #region Helper Methods
        public void ToggleOpen()
        {
            SetOpen(!m_isOpen);
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

