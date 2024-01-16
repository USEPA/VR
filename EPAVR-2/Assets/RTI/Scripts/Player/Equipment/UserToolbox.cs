using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using HurricaneVR.Framework.Core.Grabbers;

namespace L58.EPAVR
{
    public class UserToolbox : HVRGrabbableContainer
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private List<XRGrabInteractable> m_defaultItems;
        [SerializeField] private List<XRSocket> m_sockets;
        [SerializeField] private List<HVRSocket> m_hvrSockets;
        [SerializeField] private List<XRSocket> m_otherSockets;
        [SerializeField] private Transform m_topContainer;
        [SerializeField] private TriggerEventListener m_boxTriggerListener;
        [SerializeField] private Animator m_anim;
        [SerializeField] private GameObject m_model;
        [Header("Default Configuration")]
        [SerializeField] private bool m_startActive = true;
        [SerializeField] private bool m_startOpen = false;
        [SerializeField] private bool m_useHVR = false;
        #endregion
        #region Private Variables
        private Collider m_collider;
        private bool m_isOpen = false;
        private List<XRToolbeltItem> m_items;
        private XRToolbeltItem m_socketItem;

        private Action<bool> m_onSetOpen;
        
        #endregion
        #region Public Properties
        public bool IsOpen { get => m_isOpen; }

        public XRToolbeltItem SocketItem
        {
            get
            {
                if (m_socketItem == null) m_socketItem = GetComponent<XRToolbeltItem>();
                return m_socketItem;
            }
        }
        public List<XRSocket> OtherSockets { get => m_otherSockets; }

        public List<XRToolbeltItem> Items { get => m_items; }
        public Action<bool> OnSetOpen { get => m_onSetOpen; set => m_onSetOpen = value; }
        #endregion

        #region Initialization
        protected virtual void Awake()
        {
            TryGetComponent<Collider>(out m_collider);
            if (!m_useHVR) m_onSetOpen += i => SetSocketsActive(i);
            m_onSetOpen += i => m_anim.SetBool("isOpen", i);
            if (m_useHVR) Init();
            SetOpen(m_startOpen);
        }

        // Start is called before the first frame update
        void Start()
        {
            //if (m_useHVR) Init();
        }

        public override void Init()
        {
            // Check what mode to use
            if (!m_useHVR)
            {
                // Initialize list
                m_items = new List<XRToolbeltItem>();
                // Loop through each socket and initialize it
                if (m_sockets != null)
                {
                    foreach(XRSocket socket in m_sockets)
                    {
                        //UnityEngine.Debug.Log($"{gameObject.name} initializing socket: {socket.gameObject.name} || Time: {Time.time}");
                        // Check if there is already a preset item
                        socket.Init(true);
                        if (socket.CurrentItem != null)
                        {
                            m_items.Add(socket.CurrentItem);
                        }
                    }
                }
                /*
                // Loop through each default item and initialize it as a toolbox item
                for (int i = 0; i < m_defaultItems.Count; i++)
                {
                    // Cache reference
                    XRGrabInteractable interactable = m_defaultItems[i];
                    // Create the attach point
                    GameObject attachPoint = new GameObject();
                    attachPoint.name = $"Attach Point #{m_items.Count + 1}";
                    attachPoint.transform.parent = m_boxTriggerListener.transform;
                    attachPoint.transform.localPosition = interactable.transform.localPosition;
                    attachPoint.transform.localEulerAngles = interactable.transform.localEulerAngles;
                    // Create the item component
                    XRToolbeltItem item = interactable.gameObject.AddComponent<XRToolbeltItem>();
                    item.Init(interactable, attachPoint.transform, false);
                    item.AttachToBelt();
                    // Add this to the toolbox's item list
                    m_items.Add(item);
                }
                */
            }
            else
            {
                base.Init();
            }
    
            // Hook up events
            //m_onSetOpen += i => m_anim.SetBool("isOpen", i);
            m_onSetOpen += i => m_boxTriggerListener.gameObject.SetActive(i);
            if (!m_useHVR) 
            {
                m_onSetOpen += i => m_boxTriggerListener.Collider.enabled = i;
                m_onSetOpen += i => m_boxTriggerListener.Active = i;
            }

            m_onSetOpen += i => m_topContainer.gameObject.SetActive(i);
            m_onSetOpen += i => SetSocketsActive(i);
            if (m_boxTriggerListener != null && !m_useHVR)
            {
                m_boxTriggerListener.OnTriggerEntered += i => ProcessTriggerEnter(i);
                m_boxTriggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            }

            if (TryGetComponent<XRToolbeltItem>(out m_socketItem))
            {
                m_socketItem.OnAttach += i => SetOpen(false);
                m_socketItem.OnAttach += i => SetModelVisibility(false);
            }
     
            // Set initial state
            SetOpen(m_startOpen);
            SetModelVisibility(m_startActive);
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

        public void ReturnItemToToolbox(XRToolbeltItem _item)
        {
            UnityEngine.Debug.Log($"{_item.gameObject.name} returning to toolbox || Time: {Time.time}");
            // Return the item to its attach point
            _item.AttachToBelt();
            // Remove the listener
            //_item.InteractableObject.selectExited.RemoveListener(i => ReturnItemToToolbox(_item));
            _item.InteractableObject.selectExited.RemoveAllListeners();
        }

        public void SetSocketsActive(bool value)
        {
            if (!m_useHVR)
            {
                if (m_sockets == null || m_sockets.Count < 1) return;
                // Loop through each socket and set whether or not it should be active
                foreach(XRSocket socket in m_sockets)
                {
                    // Set whether or not it should be active
                    socket.gameObject.SetActive(value);
                }

            }
            else
            {
                // Make sure there are sockets
                if (m_hvrSockets == null || m_hvrSockets.Count < 1) return;
                // Loop through each socket and configure whether or not grabbables can be removed
                foreach (HVRSocket socket in m_hvrSockets)
                {
                    socket.AllowGrabbing = value;
                    socket.AllowHovering = value;

                    if (socket.GrabbedTarget != null)
                    {
                        socket.GrabbedTarget.gameObject.SetActive(value);
                    }

                    //socket.CanRemoveGrabbable = value;
                }
            }
      
     
        }
        #endregion

        #region Collision-Related Functionality
        public void ProcessTriggerEnter(Collider other)
        {
            // Try to get a toolbelt item component
            if (other.TryGetComponent<XRToolbeltItem>(out XRToolbeltItem item) && item.InteractableObject.isSelected && m_items.Contains(item) && !item.IsAttached)
            {
                //UnityEngine.Debug.Log($"ProcessTriggerEnter: {item.gameObject.name} | Event Count: {item.InteractableObject.selectExited.GetPersistentEventCount()}  || Time: {Time.time}");
                // Add the re-attach event
                item.InteractableObject.selectExited.AddListener(item.AttemptAttach);
                //item.InteractableObject.selectExited.AddListener(i => ReturnItemToToolbox(item));
            }
        }

        public void ProcessTriggerExit(Collider other)
        {
            // Try to get a toolbelt item component
            if (other.TryGetComponent<XRToolbeltItem>(out XRToolbeltItem item) && item.InteractableObject.isSelected && m_items.Contains(item) && !item.IsAttached)
            {
                //UnityEngine.Debug.Log($"Entered ProcessTriggerExit: {item.gameObject.name} | Event Count: {item.InteractableObject.selectExited.GetPersistentEventCount()} || Time: {Time.time}");
                // Remove the re-attach event
                //item.InteractableObject.selectExited.RemoveListener(i => ReturnItemToToolbox(item));
                item.InteractableObject.selectExited.RemoveListener(item.AttemptAttach);
                //item.InteractableObject.selectExited.RemoveAllListeners();
            }
        }
        #endregion

        #region Helper Methods
        public void ToggleOpen()
        {
            UnityEngine.Debug.Log($"ToggleOpen: {!m_isOpen} || Time: {Time.time}");
            SetOpen(!m_isOpen);
        }

        public void SetModelVisibility(bool value)
        {
            if (m_model != null) m_model.SetActive(value);
        }

        public void ForceReturnToSocket()
        {
            // Check active items
            if (m_items != null && m_items.Count > 0)
            {
                foreach(XRToolbeltItem item in m_items)
                {
                    if (item.HasLinkedSocket && !item.IsAttached) item.ForceAttachToSocket();
                }
            }

            if (!m_socketItem.IsAttached) m_socketItem.ForceAttachToSocket();
        }
        #endregion


        // Update is called once per frame
        void Update()
        {

        }
    }
}

