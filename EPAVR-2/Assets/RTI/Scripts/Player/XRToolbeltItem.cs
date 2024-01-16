using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRToolbeltItem : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected XRSocket m_startingSocket;
        [SerializeField] protected bool m_linkSocket = false;
        [SerializeField] public bool m_autoReturnOnRelease = false;
        #endregion
        #region Protected Variables
        protected XRGrabInteractable m_interactableObject;
        public Transform m_attachPoint;
        public XRSocket m_linkedSocket;
        public XRSocket m_potentialSocket;
        public XRSocket m_currentSocket;

        protected Action<Transform> m_onAttach;
        #endregion
        #region Public Properties
        public XRGrabInteractable InteractableObject 
        {
            get 
            {
                if (m_interactableObject == null) m_interactableObject = GetComponent<XRGrabInteractable>();
                return m_interactableObject;
            }
        }

        public XRSocket CurrentSocket 
        { 
            get => m_currentSocket;
            set 
            { 
                m_currentSocket = value;
                m_attachPoint = m_currentSocket.transform;

                if (PotentialSocket != null && PotentialSocket == value)
                {
                    PotentialSocket = null;
                }
            }
        }

        public XRSocket PotentialSocket { get => m_potentialSocket; set => m_potentialSocket = value; }
        public XRSocket LinkedSocket { get => m_linkedSocket; set => m_linkedSocket = value; }

        public bool LinkSocket { get => m_linkSocket; set => m_linkSocket = value; }
        public bool HasLinkedSocket { get => (m_linkedSocket != null); }
        public Transform AttachPoint { get => m_attachPoint; }

        public bool IsSelected { get => InteractableObject.isSelected; }
        public virtual bool IsAttached { get => transform.parent == m_attachPoint; }
        public bool AutoReturnOnRelease { get => m_autoReturnOnRelease; }

        public Action<Transform> OnAttach { get => m_onAttach; set => m_onAttach = value; }
        #endregion


        #region Initialization
        protected virtual void Awake()
        {
            // Cache references
            m_interactableObject = GetComponent<XRGrabInteractable>();
        }

        public virtual void Init(bool _autoReturnOnRelease = false)
        {
            // Check if there is a preset socket
            if (m_startingSocket != null)
            {
                // Link the socket if necessary
                if (m_linkSocket)
                {
                    // Cache linked socket reference
                    m_linkedSocket = m_startingSocket;
                    // Tell the socket what its preset socket should be
                    m_linkedSocket.PresetItem = this;
                }

                // Force attach by default
                ForceAttachToSocket(m_startingSocket);
            }
        }

        public virtual void Init(XRSocket _startingSocket, bool _linkSocket = false, bool _autoReturnOnRelease = false)
        {
            // Set starting socket
            SetStartingSocket(_startingSocket, _linkSocket);
            // Set release behavior
            SetReleaseBehavior(_autoReturnOnRelease);
        }

        public virtual void Init(XRGrabInteractable _object, Transform _attachPoint = null, bool _autoReturnOnRelease = false)
        {
            // Check if there is a preset socket
            if (m_startingSocket != null)
            {
                // Link the socket if necessary
                if (m_linkSocket)
                {
                    // Cache linked socket reference
                    m_linkedSocket = m_startingSocket;
                    // Tell the socket what its preset socket should be
                    m_linkedSocket.PresetItem = this;
                }

                // Force attach by default
                ForceAttachToSocket(m_startingSocket);
            }
            // Cache references
            if (!m_interactableObject) m_interactableObject = GetComponent<XRGrabInteractable>();
            //m_interactableObject = _object;
            if (_attachPoint != null) m_attachPoint = _attachPoint;
            // Configure whether or not the object to automatically snap back to the toolbelt when released
            m_autoReturnOnRelease = _autoReturnOnRelease;
            if (m_autoReturnOnRelease)
            {
                UnityEngine.Debug.Log($"{gameObject.name} should snap back to attach point on release || Time: {Time.time}");
                InteractableObject.selectExited.AddListener(i => AttachToBelt());
            }

            // Attach the object to the belt
            AttachToBelt();
        }

        protected void SetStartingSocket(XRSocket _socket, bool _linkSocket = false)
        {
            if (m_startingSocket != _socket) m_startingSocket = _socket;
            // Link the socket if necessary
            m_linkSocket = _linkSocket;
            if (m_linkSocket)
            {
                // Cache linked socket reference
                m_linkedSocket = m_startingSocket;
                // Tell the socket what its preset socket should be
                m_linkedSocket.SetPresetItem(this);
            }

            // Force attach by default
            ForceAttachToSocket(m_startingSocket);
        }

        protected void SetReleaseBehavior(bool _autoReturnOnRelease = false)
        {
            // Initialize whether or not this item should snap back to its socket upon release
            m_autoReturnOnRelease = _autoReturnOnRelease;
            if (m_autoReturnOnRelease)
            {
                InteractableObject.selectExited.AddListener(AttemptAttach);
            }
        }
        #endregion

        /// <summary>
        /// Parents the item to its specified attach point and resets transform values
        /// </summary>
        public virtual void AttachToBelt()
        {
            AttachToTransform(m_attachPoint);
        }

        public virtual void ForceAttachToSocket()
        {
            if (m_linkSocket)
            {
                if (m_linkedSocket != null) ForceAttachToSocket(m_linkedSocket);
            }
            else if (m_currentSocket != null)
            {
                ForceAttachToSocket(m_currentSocket);
            }
            /*
            if (!m_currentSocket) return;
            ForceAttachToSocket(m_currentSocket);
            */
        }

        public virtual void ForceAttachToSocket(XRSocket _socket)
        {
            // Assign socket reference
            if (m_currentSocket != _socket) CurrentSocket = _socket;
            // Attach to the socket's transform
            AttachToTransform(AttachPoint);
            m_currentSocket.AttachItem(this);
        }

        protected virtual void AttachToTransform(Transform _target)
        {
            transform.parent = _target;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            m_onAttach?.Invoke(_target);
        }

        public virtual void AttemptRemove()
        {
            if (!IsAttached) return;
        }

        public virtual void AttemptAttach(SelectExitEventArgs args)
        {
            if (m_linkSocket && HasLinkedSocket)
            {
                ForceAttachToSocket(m_linkedSocket);
                return;
            }
            else
            {
                if (m_potentialSocket != null)
                {
                    UnityEngine.Debug.Log($"{gameObject.name} AttemptAttach - found potential socket: {m_potentialSocket.gameObject.name} || Time: {Time.time}");
                    ForceAttachToSocket(m_potentialSocket);
                }
            }


        }

        #region Helper Methods
        public void SetGrabbable(bool _value)
        {
            InteractableObject.enabled = _value;
            /*
            if (InteractableObject.colliders != null && InteractableObject.colliders.Count > 0)
            {
                foreach (Collider col in InteractableObject.colliders)
                {
                    col.enabled = _value;
                }
            }
            */
        }
        #endregion

        #region Helper Methods
        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            args.interactor.GetComponent<XRController>().SendHapticImpulse(0.2f, 0.2f);
        }
        #endregion
    }
}

