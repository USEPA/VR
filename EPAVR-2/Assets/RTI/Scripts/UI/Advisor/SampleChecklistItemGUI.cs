using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class SampleChecklistItemGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private TextMeshProUGUI m_itemTitle;
        [SerializeField] private Image m_iconContainer;
        [Header("Configuration")]
        [SerializeField] private Sprite m_incompleteIcon;
        [SerializeField] private Sprite m_completeIcon;
        [SerializeField] private Color m_defaultTextColor;
        [SerializeField] private Color m_completedTextColor;
        #endregion
        #region Private Variables
        private bool m_completed = false;

        private ChecklistItem m_itemRef;
        #endregion
        #region Public Properties
        public ChecklistItem Reference { get => m_itemRef; }
        #endregion

        #region Initialization
        public void Init(ChecklistItem _item)
        {
            // Cache reference
            m_itemRef = _item;
            // Set item text
            m_itemTitle.text = _item.Title;
            // Set default completion state
            SetCompleted(_item.Completed);
        }

        public void Init(string _itemText, bool _startCompleted = false)
        {
            // Set item text
            m_itemTitle.text = _itemText;
            // Set default completion state
            SetCompleted(_startCompleted);
        }
        #endregion

        public void SetCompleted(bool value)
        {
            // Set icon
            m_iconContainer.sprite = (value) ? null : m_incompleteIcon;
            // Change text if necessary
            m_itemTitle.fontStyle = (value) ? FontStyles.Strikethrough : FontStyles.Normal;
            m_itemTitle.color = (value) ? m_completedTextColor : m_defaultTextColor;
            // Set completed bool
            m_completed = value;
        }
    }
}

