using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class FTIRLever : XRSimpleInteractable, IGrabbable
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private float m_snapThreshold = 5.0f;
        [SerializeField] private GameObject m_markerObject;
        #endregion
        #region Private Variables
        private bool m_locked = false;
        private XRBaseInteractor m_currentInteractor;

        private Action<bool> m_onSetLocked;
        #endregion
        #region Public Properties
        public bool Locked { get => m_locked; }

        public Action<bool> OnSetLocked { get => m_onSetLocked; set => m_onSetLocked = value; }
        #endregion

        #region Rotate-Related Functionality
        public void SnapToLockRotation()
        {
            // Set rotation
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
            // Set locked
            m_locked = true;
            m_onSetLocked?.Invoke(m_locked);
        }

        public void SnapToUnlockRotation()
        {
            // Set rotation
            transform.localEulerAngles = Vector3.zero;
            // Set locked
            m_locked = false;
            m_onSetLocked?.Invoke(m_locked);
        }

        public void ToggleRotationLock()
        {
            UnityEngine.Debug.Log($"Arrived in Toggle Rotation Lock: {m_locked} || Time: {Time.time}");
            if (!m_locked)
                SnapToLockRotation();
            else
                SnapToUnlockRotation();
        }
        #endregion

        #region Interaction-Related Functionality
        public void OnSelected(XRBaseInteractor _interactor)
        {
            /*
            // Set unlocked
            m_locked = false;
            m_onSetLocked?.Invoke(m_locked);
            */
            m_currentInteractor = _interactor;
            //m_markerObject.SetActive(true);
        }

        public void OnDeselected(XRBaseInteractor _interactor)
        {
            // Check if the current rotation is close enough to snap to lock rotation
            //if (Mathf.Abs(transform.localEulerAngles.z - 90.0f) <= m_snapThreshold) SnapToLockRotation();
            m_currentInteractor = null;
            //m_markerObject.SetActive(false);
        }

        public void OnGrab()
        {
            // Do the thing
        }

        public void OnDrop()
        {
            // Do the thing
        }

        /*
        protected override void Grab()
        {
            // Call base functionality
            base.Grab();
            // Invoke any specific grab functionality
            OnGrab();
        }

        protected override void Drop()
        {
            // Call base functionality
            base.Drop();
            // Invoke any specific drop functionality
            OnDrop();
        }
        */
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            // Check if there is a current interactor
            if (!m_currentInteractor) return;
            /*
            // Get the current position of the object
            Vector3 interactorLocalPosition = transform.InverseTransformPoint(m_currentInteractor.transform.position);
            interactorLocalPosition.z = 0.0f;
            m_markerObject.transform.localPosition = interactorLocalPosition;
            Vector2 targetPosition = new Vector2(-interactorLocalPosition.x, -interactorLocalPosition.y);
            // Get the angle
            float angle = (Mathf.Atan2(targetPosition.y, targetPosition.x) * Mathf.Rad2Deg);
            transform.localEulerAngles = new Vector3(0, 0, angle);
            */
        }
    }
}
