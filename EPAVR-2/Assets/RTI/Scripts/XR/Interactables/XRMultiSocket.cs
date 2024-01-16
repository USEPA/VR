using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRMultiSocket : XRSocket
    {
        #region Protected Variables
        protected List<XRToolbeltItem> m_items;
        #endregion

        #region Initialization
        public void Init(List<XRToolbeltItem> _items)
        {
            // Initialize list
            m_items = new List<XRToolbeltItem>();
            foreach (XRToolbeltItem item in _items) m_items.Add(item);
        }

        public override void SetPresetItem(XRToolbeltItem _item)
        {
            // Do nothing
            //base.SetPresetItem(_item);
        }
        #endregion

        #region Configuration Functionality
        protected override bool ItemIsValid(XRToolbeltItem _item)
        {
            return (m_items != null && m_items.Contains(_item));
        }
        #endregion

        /*
        #region Collision-Related Functionality
        public override void OnTriggerEnter(Collider other)
        {
            // Check if this is the item
            if (other.TryGetComponent<XRSocketItem>(out XRSocketItem item) && m_items.Contains(item))
            {
                // Tell this object it is at the attach point
                item.IsAtAttachPoint = true;
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            // Check if this is the item
            if (other.TryGetComponent<XRSocketItem>(out XRSocketItem item) && m_items.Contains(item))
            {
                // Tell this object it is no longer at the attach point
                item.IsAtAttachPoint = false;
            }
        }
        #endregion
        */
    }
}

