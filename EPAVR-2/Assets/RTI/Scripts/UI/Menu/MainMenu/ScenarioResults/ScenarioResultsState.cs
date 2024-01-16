using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class ScenarioResultsState : MenuState
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI m_scenarioName;
        [SerializeField] TextMeshProUGUI m_completionTime;
        [SerializeField] TextMeshProUGUI m_totalScoreValue;
        [SerializeField] FloatValueEventHandler m_gradeDisplay;
        [SerializeField] RectTransform m_scoreContainerRect;
        [SerializeField] EndScenarioDetailsDisplayInjector m_endScenarioDetailsContainer;
        [Header("Prefab References")]
        [SerializeField] DoubleTextDisplay m_scoreItemDisplayPrefab;
        [SerializeField] ParentTextDisplay m_parentScoreItemDisplayPrefab;
        #endregion
        #region Private Variables
        private ScenarioResults m_scenarioResults;
        private List<DoubleTextDisplay> m_scoreDisplayItems;
        #endregion
        #region Public Properties
        public ScenarioResults CurrentResults { get => m_scenarioResults; }
        #endregion

        #region Enter/Exit-Related Functionality
        public override void OnStateEnter()
        {
            // Load scenarios if necessary
            if (m_scenarioResults == null && ScoreManager.Instance != null && ScoreManager.Instance.ScenarioResults != null)
            {
                LoadScenarioResults(ScoreManager.Instance.ScenarioResults);
            }

            // Call base functionality
            base.OnStateEnter();
        }

        public override void OnStateExit()
        {
            // Call base functionality
            base.OnStateExit();
        }
        #endregion

        #region Scenario Data-Related Functionality
        public void LoadScenarioResults(ScenarioResults _scenarioResults)
        {
            // Cache reference
            m_scenarioResults = _scenarioResults;
            // Set display
            m_scenarioName.text = _scenarioResults.ScenarioName;
            TimeSpan time = TimeSpan.FromSeconds(_scenarioResults.CompletionTime);
            m_completionTime.text = "Time: " + time.ToString(@"hh\:mm\:ss");
            m_totalScoreValue.text = _scenarioResults.TotalScore.ToString();
            // Create the necessary display items for the score
            CreateScoreDisplayItems(_scenarioResults.ScoreItems);
            // Update the grade display
            m_gradeDisplay.SetValue(_scenarioResults.Grade);

            // Check if there are end results
            if (ScenarioManager.Instance.EndScenarioInfo != null)
            {
                m_endScenarioDetailsContainer.LoadScenarioInfo(ScenarioManager.Instance.EndScenarioInfo);
            }
        }
        #endregion

        #region Display Functionality
        void CreateScoreDisplayItems(List<ScoreItem> _scoreItems)
        {
            // Make sure there are score items
            if (_scoreItems == null || _scoreItems.Count < 1) return;
            // Loop through each score item and create a display for it
            for(int i = 0; i < _scoreItems.Count; i++)
            {
                ScoreItem item = _scoreItems[i];
                if (item.Title.Contains("Penalty") && m_scenarioResults != null && m_scenarioResults.Penalties != null)
                {
                    UnityEngine.Debug.Log($"{gameObject.name} found penalty container reference | Penalty Count: {m_scenarioResults.Penalties.Count} || Time: {Time.time}");
                    // Create a parent child item
                    DoubleTextDisplay itemDisplay = CreateScoreDisplayItem(item, true);
                    if (itemDisplay is ParentTextDisplay parentDisplay)
                    {
                        // Create all the necessary child display items for the penalties
                        CreatePenaltyDisplayItems(parentDisplay, m_scenarioResults.Penalties);
                    }
                }
                else
                {
                    // Create an individual child item
                    CreateScoreDisplayItem(item);
                }
                
            }
        }

        void CreatePenaltyDisplayItems(ParentTextDisplay _parent, List<ScoreItem> _penalties)
        {
            if (_penalties.Count < 1) return;
            // Loop through each penalty and create a display for it under the parent
            for(int i = 0; i < _penalties.Count; i++)
            {
                ScoreItem penalty = _penalties[i];
                _parent.AddChild(penalty.Title, penalty.Value.ToString());
            }
        }

        DoubleTextDisplay CreateScoreDisplayItem(ScoreItem _scoreItem, bool _isParent = false)
        {
            // Instantiate a text display to the specified container
            DoubleTextDisplay scoreItemDisplay = Instantiate(((!_isParent) ? m_scoreItemDisplayPrefab : m_parentScoreItemDisplayPrefab), m_scoreContainerRect);
            // Load the score information
            //string scoreValue = (_scoreItem.MaxValue != 0) ? $"{_scoreItem.Value.ToString()}/{_scoreItem.MaxValue.ToString()}" : _scoreItem.Value.ToString();
            string scoreValue = _scoreItem.Value.ToString();
            scoreItemDisplay.SetText(_scoreItem.Title, scoreValue);

            // Add this to the list
            if (m_scoreDisplayItems == null) m_scoreDisplayItems = new List<DoubleTextDisplay>();
            m_scoreDisplayItems.Add(scoreItemDisplay);
            return scoreItemDisplay;
        }
        #endregion


    }
}

