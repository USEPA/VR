using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRSocketRespawner : XRSocket
    {
        #region Inspector Assigned Variables
        [Header("Spawn Configuration")]
        [SerializeField] protected XRGrabInteractable m_itemToSpawn;
        [SerializeField] protected int m_maxActiveItems = 4;
        #endregion
        #region Protected Variables
        protected int m_activeItemCount = 0;
        protected List<XRGrabInteractable> m_activeItems;
        #endregion
        #region Public Properties
        public int ActiveItemCount { get { return (m_activeItems != null) ? m_activeItems.Count : 0; } }
        #endregion

        #region Initialization
        public override void Init(bool _tryToInitChiildItem = false)
        {
            // Spawn the first item
            SpawnItem();
            // Call base functionality
            //base.Init(_item);
        }
        /*
        public override void Init(XRToolItem _item, bool _initItem = true)
        {
            // Spawn the first item
            SpawnItem();
            // Call base functionality
            //base.Init(_item);
        }
        */

        public override void SetPresetItem(XRToolbeltItem _item)
        {
            // Do nothing
            //base.SetPresetItem(_item);
        }
        #endregion

        #region Spawn-Related Functionality
        public void TrySpawnItem()
        {
            //UnityEngine.Debug.Log($"{gameObject.name} tried to spawn an item | Socket Current Item: {(GameObjectHelper.GetNameIfAvailable(m_currentItem.gameObject))} || Time: {Time.time}");
            // Make sure the socket can spawn another item
            if (ActiveItemCount < m_maxActiveItems) SpawnItem();
        }
        protected XRToolbeltItem SpawnItem()
        {
            // Make sure the socket has nothing
            //if (m_currentItem != null) return null;
            //UnityEngine.Debug.Log($"{gameObject.name} arrived in SpawnItem: {ActiveItemCount} | Max Items: {m_maxActiveItems} || Time: {Time.time}");
            // Instantiate the item
            XRGrabInteractable interactable = Instantiate(m_itemToSpawn, transform);
            XRToolbeltItem item = interactable.GetComponent<XRToolbeltItem>();
            if (!item) item = interactable.gameObject.AddComponent<XRToolbeltItem>();
            // Initialize the item in the socket
            item.Init(this);
            /*
            item.CurrentSocket = this;
            item.Init(interactable, transform);
            UnityEngine.Debug.Log($"{gameObject.name} reached attach to belt | Item: {item.gameObject.name} || Time: {Time.time}");
            item.AttachToBelt();
            */
            // Add this to the list
            if (m_activeItems == null) m_activeItems = new List<XRGrabInteractable>();
            m_activeItems.Add(interactable);
            
            if (interactable.TryGetComponent<IDisposable>(out IDisposable disposable))
            {
                disposable.OnDisposed += () => DespawnItem(interactable);
            }
            //m_currentItem = item;
            //UnityEngine.Debug.Log($"{gameObject.name} exiting SpawnItem: {m_currentItem.gameObject.name} | Item Count: {ActiveItemCount} | Max Items: {m_maxActiveItems} || Time: {Time.time}");
            return item;
        }

        protected void DespawnItem(XRGrabInteractable _item)
        {
            //UnityEngine.Debug.Log($"{gameObject.name} arrived in DespawnItem: {_item.gameObject.name} | Count: {ActiveItemCount} | Max Items: {m_maxActiveItems}  || Time: {Time.time}");
            if (m_activeItems == null || !m_activeItems.Contains(_item)) return;
            m_activeItems.Remove(_item);
            //UnityEngine.Debug.Log($"{gameObject.name} DespawnItem - Removed: {_item.gameObject.name} | Count: {ActiveItemCount} | Max Items: {m_maxActiveItems}  || Time: {Time.time}");
            if (_item.TryGetComponent<IDisposable>(out IDisposable disposable))
            {
                disposable.OnDisposed = null;
            }

            if (m_currentItem == null) TrySpawnItem();
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

