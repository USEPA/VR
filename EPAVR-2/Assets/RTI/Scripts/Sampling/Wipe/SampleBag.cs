using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SampleBag : MonoBehaviour, IDisposable
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected XRSocket m_sampleSocket;
        #endregion
        #region Protected Variables
        protected XRGrabInteractable m_interactableObject;

        protected SampleWipe m_currentWipe;

        #endregion
        #region Public Properties
        public SampleWipe CurrentWipe { get => m_currentWipe; }

        public XRGrabInteractable Interactable 
        {
            get
            {
                if (m_interactableObject == null) m_interactableObject = GetComponent<XRGrabInteractable>();
                return m_interactableObject;
            }
        }

        public Sample CurrentSample 
        { 
            get 
            { 
                if (m_currentWipe != null && m_currentWipe.CurrentSample != null)
                {
                    return m_currentWipe.CurrentSample;
                }
                return null;
            } 
        }

        public bool HasSample { get => (m_currentWipe != null);  }

        public System.Action OnDisposed { get; set; }
        #endregion

        private void Awake()
        {
            // Cache components
            m_interactableObject = GetComponent<XRGrabInteractable>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!m_sampleSocket) return;
            m_sampleSocket.Init(false);
        }

        public void BagSample(SampleWipe _wipe)
        {
            UnityEngine.Debug.Log($"{gameObject.name} bagging object: {_wipe.gameObject.name} || Time: {Time.time}");
            // Cache reference
            m_currentWipe = _wipe;
            // Disable grabbing 
            _wipe.SocketItem.SetGrabbable(false);
        }

        public void TryBagSample(XRToolbeltItem _potentialWipe)
        {
            if (_potentialWipe.TryGetComponent<SampleWipe>(out SampleWipe wipe))
            {
                // Bag the sample
                BagSample(wipe);
            }
        }

        public void Dispose()
        {
            if (m_currentWipe != null)
            {
                if (m_currentWipe.SocketItem != null)
                {
                    XRToolbeltItem item = m_currentWipe.SocketItem;
                    if (!item.LinkSocket) item.LinkSocket = true;

                    item.ForceAttachToSocket(VRUserManager.Instance.Player.MainDeviceAttachPoint);
                    UnityEngine.Debug.Log($"{gameObject.name} being disposed with item: {m_currentWipe.gameObject.name} | Socket: {item.CurrentSocket} || Time: {Time.time}");
                    item.SetGrabbable(true);
                }
            }
            OnDisposed?.Invoke();
        }

        void OnDestroy()
        {
            Dispose();
        }

        #region Helper Methods
        public void ForceUnlinkSocket(XRToolbeltItem _item)
        {
            if (_item.LinkSocket && _item.HasLinkedSocket) _item.LinkSocket = false;
        }

        public void ForceRelinkSocket(XRToolbeltItem _item)
        {
            if (_item.HasLinkedSocket && !_item.LinkSocket) _item.LinkSocket = true;
        }
        #endregion
    }
}

