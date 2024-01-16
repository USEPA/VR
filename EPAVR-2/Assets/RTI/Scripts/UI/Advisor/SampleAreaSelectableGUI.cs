using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class SampleAreaSelectableGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Button m_button;
        [SerializeField] private TextMeshProUGUI m_areaTitleDisplay;
        #endregion
        #region Private Variables
        private SampleArea m_sampleArea;
        private List<ChecklistItem> m_checklistItems;
        private List<SampleChecklistItemGUI> m_checklistItemGUIElements;

        private System.Action<ChecklistItem> m_onCompleteChecklistItem;
        #endregion
        #region Public Properties
        public SampleArea SampleArea { get => m_sampleArea; }
        public List<ChecklistItem> ChecklistItems { get => m_checklistItems; }

        public Button Button { get => m_button; }

        public System.Action<ChecklistItem> OnCompleteChecklistItem { get => m_onCompleteChecklistItem; set => m_onCompleteChecklistItem = value; }
        #endregion

        #region Initialization
        public void Init(SampleArea _sampleArea, int _index)
        {
            // Set sample area reference
            m_sampleArea = _sampleArea;
            // Set display text
            string name = $"{_index}_{m_sampleArea.TypeID}";
            m_areaTitleDisplay.text = name;
            gameObject.name = name;
            // Create checklist items
            CreateChecklistItems();
            // Hook up events
            m_button.onClick.AddListener(OnSelected);
            m_sampleArea.OnIdentifyAgent += () => CompleteChecklistItem(0);
        }
        #endregion

        #region Checklist-Related Functionality
        public void CreateChecklistItems()
        {
            // Initialize new list
            m_checklistItems = new List<ChecklistItem>();
            // By default, add the checklist item for identifying the surface's chemical agent
            m_checklistItems.Add(new ChecklistItem("Identified Chemical Agent"));
        }

        public void CompleteChecklistItem(int _index)
        {
            if (m_checklistItems == null || m_checklistItems.Count < 1 || _index < 0 || _index >= m_checklistItems.Count) return;
            // Cache checklist item reference
            ChecklistItem item = m_checklistItems[_index];
            item.Completed = true;
            m_onCompleteChecklistItem?.Invoke(item);
        }
        #endregion

        #region Menu-Related Functionality
        public void OnSelected()
        {
            // Make sure there is an active Advisor GUI
            if (!AdvisorManager.Instance || !AdvisorManager.Instance.GUI) return;
            // Tell the Advisor GUI to select this
            AdvisorManager.Instance.GUI.SetSelectedSampleArea(this);
        }
        #endregion
    }

    public class ChecklistItem
    {
        #region Public Properties
        public string Title { get; }
        public bool Completed { get; set; }
        #endregion

        public ChecklistItem(string _title, bool _completed = false)
        {
            Title = _title;
            Completed = _completed;
        }
    }
}

