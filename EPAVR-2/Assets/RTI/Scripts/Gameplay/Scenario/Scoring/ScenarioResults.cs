using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [System.Serializable]
    public class ScenarioResults
    {
        #region Protected Variables
        protected ScenarioAsset m_scenario;
        protected List<ScoreItem> m_scoreItems;
        protected List<ScoreItem> m_penalties;

        protected int m_totalScore = 0;
        protected float m_completionTime;
        protected float m_grade;
        #endregion
        #region Public Properties
        public ScenarioAsset Scenario { get => m_scenario; }
        public string ScenarioName { get => m_scenario.Title; }
        public List<ScoreItem> ScoreItems { get => m_scoreItems; }
        public List<ScoreItem> Penalties { get => m_penalties; }
        public int TotalScore { get => m_totalScore; }
        public float CompletionTime { get => m_completionTime; }
        public float Grade { get => m_grade; set => m_grade = value; }
        #endregion

        #region Constructors
        public ScenarioResults(ScenarioAsset _scenario, float _completionTime, List<ScoreItem> _scoreItems = null, List<ScoreItem> _penalties = null)
        {
            // Cache references/values
            m_scenario = _scenario;
            m_completionTime = _completionTime;
            // Create the score items
            LoadScoreItems(_scoreItems);
            // Create the penalty score items
            LoadPenalties(_penalties);
        }

        public ScenarioResults(ScenarioResults other)
        {
            // Copy references/values
            m_scenario = other.Scenario;
            m_completionTime = other.CompletionTime;
            // Copy the score items
            LoadScoreItems(other.ScoreItems);
            // Copy the penalty score items
            LoadPenalties(other.Penalties);
        }
        #endregion

        #region Score-Related Functionality
        void LoadScoreItems(List<ScoreItem> _scoreItems)
        {
            int total = 0;
            m_scoreItems = new List<ScoreItem>();
            for(int i = 0; i < _scoreItems.Count; i++)
            {
                ScoreItem item = _scoreItems[i];
                m_scoreItems.Add(item);
                total += item.Value;
            }
            m_totalScore = total;
        }

        void LoadPenalties(List<ScoreItem> _penalties)
        {
            UnityEngine.Debug.Log($"Loading Penalties: {_penalties.Count} || Time: {Time.time}");
            if (_penalties == null || _penalties.Count < 1) return;
            m_penalties = new List<ScoreItem>();
            foreach(ScoreItem penalty in _penalties)
            {
                m_penalties.Add(penalty);
            }
        }
        #endregion
    }

}
