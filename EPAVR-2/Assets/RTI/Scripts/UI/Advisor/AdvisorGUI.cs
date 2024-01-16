using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class AdvisorGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] TextMeshProUGUI m_globalTimerDisplay;
        [SerializeField] TextMeshProUGUI m_scenarioSampleProgressDisplay;
        [SerializeField] RectTransform m_sampleAreaListContainer;
        [SerializeField] RectTransform m_sampleChecklistContainer;
        [SerializeField] RawImage m_minimapImage;
        [Header("Prefab References")]
        [SerializeField] SampleAreaSelectableGUI m_sampleAreaSelectablePrefab;
        [SerializeField] SampleChecklistItemGUI m_sampleChecklistItemPrefab;
        #endregion
        #region Private Variables
        private List<SampleAreaSelectableGUI> m_selectableAreas;

        private SampleAreaSelectableGUI m_currentSelectedArea;
        private List<SampleChecklistItemGUI> m_currentChecklistItems;
        #endregion
        #region Public Properties
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize lists
            m_selectableAreas = new List<SampleAreaSelectableGUI>();
            // Make sure there is a ScenarioManager with valid sample areas
            if (!ScenarioManager.Instance || ScenarioManager.Instance.SampleAreas == null || ScenarioManager.Instance.SampleAreas.Count < 1) return;
            // Loop through each sample area and create a selectable object for it
            for (int i = 0; i < ScenarioManager.Instance.SampleAreas.Count; i++)
            {
                SampleArea sampleArea = ScenarioManager.Instance.SampleAreas[i];
                SampleAreaSelectableGUI sampleAreaSelectable = Instantiate(m_sampleAreaSelectablePrefab, m_sampleAreaListContainer);
                sampleAreaSelectable.Init(sampleArea, i);
                // Add this to the list of selectables
                m_selectableAreas.Add(sampleAreaSelectable);
            }
            // Initialize default values
            SetTargetCount(0);
            SetSelectedSampleArea(m_selectableAreas[0]);
            // Hook up scenario timer update event
            //ScenarioManager.Instance.OnGlobalScenarioTimerTick += i => UpdateTimer(i);
         }
        #endregion

        #region View-Related Functionality
        public void SetSelectedSampleArea(SampleAreaSelectableGUI _item)
        {
            // Check if there was a previously referenced selection
            if (m_currentSelectedArea != null)
            {
                // Remove the complete checklist item event
                m_currentSelectedArea.OnCompleteChecklistItem -= i => CompleteChecklistItem(i);
            }
            // Configure whether or not checklist container is active
            if (_item == null)
                m_sampleChecklistContainer.transform.parent.parent.gameObject.SetActive(false);
            else
                m_sampleChecklistContainer.transform.parent.parent.gameObject.SetActive(true);
            // Cache selected area references
            m_currentSelectedArea = _item;
            // Create the checklist items
            CreateChecklistItems(_item);
            // Hook up checklist item completion event
            m_currentSelectedArea.OnCompleteChecklistItem += i => CompleteChecklistItem(i);
        }

        public void CreateChecklistItems(SampleAreaSelectableGUI _item)
        {
            // Initialize checklist item list
            if (m_currentChecklistItems == null) m_currentChecklistItems = new List<SampleChecklistItemGUI>();
            // Check if there are any previous checklist items active
            if (m_currentChecklistItems.Count > 0)
            {
                // Destroy each of the game objects
                for (int i = 0; i < m_currentChecklistItems.Count; i++) Destroy(m_currentChecklistItems[i].gameObject);
                // Clear the list
                m_currentChecklistItems.Clear();
            }

            // Loop through each checklist item and create an object for it
            for (int i = 0; i < _item.ChecklistItems.Count; i++)
            {
                // Create the GUI checklist item object
                SampleChecklistItemGUI checklistGUIObject = Instantiate(m_sampleChecklistItemPrefab, m_sampleChecklistContainer);
                ChecklistItem checklistItem = _item.ChecklistItems[i];
                checklistGUIObject.Init(checklistItem);
                // Add this to the current checklist item list
                m_currentChecklistItems.Add(checklistGUIObject);
            }
        }

        public void CompleteChecklistItem(ChecklistItem _item)
        {
            // Make sure there are active checklist item GUI objects
            if (m_currentChecklistItems == null || m_currentChecklistItems.Count < 1 || !(m_currentChecklistItems.Any(i => (i.Reference == _item)))) return;
            // Get the checklist item GUI object
            SampleChecklistItemGUI checklistItemGUI = m_currentChecklistItems.First(i => i.Reference == _item);
            checklistItemGUI.SetCompleted(true);
        }
        #endregion

        #region Individual UI-Related Functionality
        public void UpdateTimer(float value)
        {
            m_globalTimerDisplay.text = $"{Mathf.Floor(value / 60).ToString("00")}:{(value % 60).ToString("00")}";
        }

        public void SetTargetCount(int count)
        {
            m_scenarioSampleProgressDisplay.text = $"{count}/{m_selectableAreas.Count} Targets";
        }
        #endregion
    }
}

