using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRSocketPresetItemConditional : XRSocketConditional
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected XRGrabInteractable m_presetItem;
        #endregion

        #region Public Properties
        public XRGrabInteractable PresetItem { get => m_presetItem; set => m_presetItem = value; }
        #endregion

        #region Initialization
        public override void Init(XRSocket _parent)
        {
            // Call base functionality
            base.Init(_parent);
    
            // Set preset item reference
            if (m_presetItem != null)
            {
                XRToolbeltItem item = m_presetItem.GetComponent<XRToolbeltItem>();
                if (!item) item = m_presetItem.gameObject.AddComponent<XRToolbeltItem>();

                m_parent.PresetItem = item;
            }
            else
            {
                // Hook up event
                m_parent.OnPresetItemChange += i => PresetItem = i.GetComponent<XRGrabInteractable>();
            }
        }
        #endregion
        public override bool ItemIsValid(XRToolbeltItem _item)
        {
            return (m_parent.PresetItem != null && (m_parent.PresetItem == _item));
        }
    }
}

