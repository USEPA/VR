using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRSocket : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected List<XRSocketConditional> m_conditions;
        [SerializeField] protected bool m_allowSocketIfNoConditional = true;
        [Header("Events")]
        [SerializeField] protected UnityEvent<XRToolbeltItem> m_onAttachItem;
        [SerializeField] protected UnityEvent<XRToolbeltItem> m_onRemoveItem;

        [SerializeField] protected UnityEvent<XRToolbeltItem> m_onItemHoverEnter;
        [SerializeField] protected UnityEvent<XRToolbeltItem> m_onItemHoverExit;
        #endregion
        #region Protected Variables
        public XRToolbeltItem m_presetItem;
        protected Collider m_collider;

        protected bool m_isActive = true;
        public XRToolbeltItem m_currentItem;
        #endregion
        #region Public Properties
        public XRToolbeltItem PresetItem 
        { 
            get => m_presetItem; 
            set 
            {
                m_presetItem = value;
                OnPresetItemChange?.Invoke(value);
            } 
        }
        public Collider Collider 
        {
            get
            {
                if (!m_collider) m_collider = GetComponent<Collider>();
                return m_collider;
            }
        }

        public bool Active
        {
            get => m_isActive;
            set
            {
                m_isActive = value;
                Collider.enabled = value;
            }
        }

        public XRToolbeltItem CurrentItem { get => m_currentItem; }

        public List<XRSocketConditional> Conditions { get => m_conditions; }
        public UnityEvent<XRToolbeltItem> OnAttachItem { get => m_onAttachItem; set => m_onAttachItem = value; }
        public UnityEvent<XRToolbeltItem> OnRemoveItem { get => m_onAttachItem; set => m_onRemoveItem = value; }

        public UnityEvent<XRToolbeltItem> OnItemHoverEnter { get => m_onItemHoverEnter; set => m_onItemHoverEnter = value; }
        public UnityEvent<XRToolbeltItem> OnItemHoverExit { get => m_onItemHoverExit; set => m_onItemHoverExit = value; }

        public Action<XRToolbeltItem> OnPresetItemChange { get; set; }
        #endregion

        #region Initialization
        protected void Awake()
        {
            // Cache collider component
            m_collider = GetComponent<Collider>();
            // Initialize all socket conditionals
            if (m_conditions != null && m_conditions.Count > 0)
            {
                foreach (XRSocketConditional condition in m_conditions)
                    condition.Init(this);
            }
        }

        public virtual void Init(bool _tryToInitChiildItem = false)
        {
            if (_tryToInitChiildItem)
            {
                XRToolbeltItem item = GetComponentInChildren<XRToolbeltItem>();
                if (item != null)
                {
                    //UnityEngine.Debug.Log($"{gameObject.name} arrived in Init and found child item: {item.gameObject.name} || Time: {Time.time}");
                    item.Init(this, true);
                }
            }
        }

        public virtual void Init(XRToolItem _item = null, bool _initItem = true)
        {
            if (m_presetItem == null && _item != null)
            {
                // Cache reference
                m_presetItem = _item;
            }
            /*
            // Check if the preset item is valid
            if (m_presetItem != null)
            {
                // Get the interactable component
                XRGrabInteractable interactable = m_presetItem.GetComponent<XRGrabInteractable>();
                // Auto-attach the preset item
                m_presetItem.CurrentSocket = this;
                if (_initItem)
                {
                    m_presetItem.Init(interactable, transform);
                }
            }
            */
            // Cache reference
            //m_presetItem = _item;
        }

        public virtual void SetPresetItem(XRToolbeltItem _item)
        {
            PresetItem = _item;
        }
        #endregion

        #region Configuration Functionality
        protected virtual bool ItemIsValid(XRToolbeltItem _item)
        {
            if (m_currentItem != null) return false;
            if (m_conditions != null && m_conditions.Count > 0)
            {
                // Loop through each condition and validate it
                foreach(XRSocketConditional condition in m_conditions)
                {
                    if (!condition.ItemIsValid(_item)) return false;
                }
                return true;
            }
            return (m_allowSocketIfNoConditional);
            //return (_item == m_presetItem);
        }
        #endregion

        #region Attach/Detach-Related Functionality
        public virtual void AttachItem(XRToolbeltItem _item)
        {
            //UnityEngine.Debug.Log($"{gameObject.name} arrived in AttachItem: {_item.gameObject.name} || Time: {Time.time}");
            // Cache reference
            m_currentItem = _item;
            // Fire off any necessary events
            m_onAttachItem?.Invoke(m_currentItem);
            // Hook up remove item event
            m_currentItem.InteractableObject.selectEntered.AddListener(RemoveItem);
            //UnityEngine.Debug.Log($"{gameObject.name} about to exit AttachItem: {m_currentItem.gameObject.name} || Time: {Time.time}");
        }


        public virtual void RemoveItem()
        {
            if (!m_currentItem) return;
            //UnityEngine.Debug.Log($"{gameObject.name} arrived in RemoveItem: {m_currentItem.gameObject.name} || Time: {Time.time}");
            if (m_currentItem.gameObject.activeInHierarchy)
            {
                // Unparent the item
                m_currentItem.transform.parent = null;
                XRToolbeltItem item = m_currentItem;
                // Clear reference
                m_currentItem = null;
                // Fire off any necessary events
                m_onRemoveItem?.Invoke(item);
            }
            //UnityEngine.Debug.Log($"{gameObject.name} exiting RemoveItem: {((m_currentItem != null) ? m_currentItem.gameObject.name : "N/A")} || Time: {Time.time}"); 
        }

        public virtual void RemoveItem(SelectEnterEventArgs args)
        {
            //UnityEngine.Debug.Log($"{gameObject.name} arrived in RemoveItem: {GameObjectHelper.GetNameIfAvailable(m_currentItem.gameObject)} || Time: {Time.time}");
            if (!m_currentItem) return;
            // Unhook remove item event
            m_currentItem.InteractableObject.selectEntered.RemoveListener(RemoveItem);
            // Call normal remove item functionality
            RemoveItem();
        }
        #endregion

        #region Collision-Related Functionality
        public virtual void OnTriggerEnter(Collider other)
        {
            // Check if this is the item
            if (other.TryGetComponent<XRToolbeltItem>(out XRToolbeltItem item) && !item.AutoReturnOnRelease && item.IsSelected && ItemIsValid(item))
            {
                //UnityEngine.Debug.Log($"{gameObject.name} OnTriggerEnter: {item.gameObject.name} || Time: {Time.time}");
                SetPotentialItem(item);
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            // Check if this is the item
            if (other.TryGetComponent<XRToolbeltItem>(out XRToolbeltItem item) && !item.AutoReturnOnRelease && item.IsSelected && ItemIsValid(item))
            {
                //UnityEngine.Debug.Log($"{gameObject.name} OnTriggerExit: {item.gameObject.name} || Time: {Time.time}");
                ClearPotentialItem(item);
            }
        }
        #endregion

        #region Helper Methods
        protected virtual void SetPotentialItem(XRToolbeltItem _item)
        {
            // Set potential socket reference
            _item.PotentialSocket = this;
            // Add attach event
            _item.InteractableObject.selectExited.AddListener(_item.AttemptAttach);
            // Invoke any necessary events
            m_onItemHoverEnter?.Invoke(_item);
            /*
            // Tell this object it is at the attach point
            _item.IsAtAttachPoint = true;
            // Relay what socket this is
            _item.CurrentSocket = this;
            */
        }

        protected virtual void ClearPotentialItem(XRToolbeltItem _item)
        {
            _item.PotentialSocket = null;
            // Remove attach event
            _item.InteractableObject.selectExited.RemoveListener(_item.AttemptAttach);
            // Invoke any necessary events
            m_onItemHoverExit?.Invoke(_item);
            /*
            // Tell this object it is no longer at the attach point
            _item.IsAtAttachPoint = false;
            // Clear socket reference on object
            if (!_item.AutoReturnOnRelease) _item.CurrentSocket = null;
            */
        }

        public void SetItemGrabbable(bool _value)
        {
            if (!m_currentItem) return;

            m_currentItem.SetGrabbable(_value);
        }

#if UNITY_EDITOR
        [ContextMenu("Cache Conditionals")]
        public void CacheConditionals()
        {
            Component[] conditionals;
            conditionals = GetComponents(typeof(XRSocketConditional));
            if (conditionals != null && conditionals.Length > 0)
            {
                if (m_conditions == null) m_conditions = new List<XRSocketConditional>();
                foreach(XRSocketConditional condition in conditionals)
                {
                    if (m_conditions.Count > 0 && m_conditions.Contains(condition)) continue;
                    m_conditions.Add(condition);
                }
            }
        }
#endif
    #endregion
    }
}

