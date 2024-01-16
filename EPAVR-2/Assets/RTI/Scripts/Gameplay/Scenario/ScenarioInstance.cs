using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class ScenarioInstance
    {
        #region Public Variables
        [Header("Important References")]
        public ScenarioAsset m_scenario;
        #endregion
        #region Protected Variables
        protected bool m_isActive = false;
        protected bool m_isCleared = false;
        protected float m_timer = 0.0f;

        protected List<ScenarioObjective> m_objectives;
        protected List<ScenarioObjective> m_completedObjectives;
        protected int m_objectiveCount;

        protected List<ContaminationSite> m_contaminantSiteMasterList;
        protected List<ContaminationSite> m_activeSites;
        protected List<ContaminationSite> m_clearedSites;

        protected UnityEvent<float> m_onTimerTick;
        protected UnityEvent<ScenarioObjective> m_onObjectiveCompleted;
        #endregion
        #region Public Properties
        public bool IsCleared { get => m_isCleared; }

        public ScenarioAsset Scenario { get => m_scenario; }
        public Gamemode Gamemode { get => m_scenario.Mode; }
        public float MaxBonusTime { get => m_scenario.MaxBonusTime; }

        public bool IsActive { get => m_isActive; }
        public float GlobalScenarioTimer
        {
            get => m_timer;
            set
            {
                m_timer = value;
                m_onTimerTick?.Invoke(m_timer);
            }
        }

        public List<ScenarioObjective> Objectives
        {
            get
            {
                if (m_objectives == null) m_objectives = new List<ScenarioObjective>();
                return m_objectives;
            }
        }

        public List<ScenarioObjective> CompletedObjectives
        {
            get
            {
                if (m_completedObjectives == null) m_completedObjectives = new List<ScenarioObjective>();
                return m_completedObjectives;
            }
        }

        public int ObjectiveCount { get => m_objectiveCount; }

        public int ActiveObjectiveCount
        {
            get
            {
                if (m_objectives == null) return 0;
                return m_objectives.Count;
            }
        }
        public List<ContaminationSite> ContaminantSites { get => m_contaminantSiteMasterList; }
        public List<ContaminationSite> ActiveSites { get => m_activeSites; }
        public List<ContaminationSite> ClearedSites { get; }

        public int ClearedSiteCount
        {
            get
            {
                if (m_clearedSites != null) return m_clearedSites.Count;
                return 0;
            }
        }

        public UnityEvent<float> OnTimerTick 
        { 
            get
            {
                if (m_onTimerTick == null) m_onTimerTick = new UnityEvent<float>();
                return m_onTimerTick;
            }
        }
        public UnityEvent<ScenarioObjective> OnObjectiveCompleted 
        { 
            get
            {
                if (m_onObjectiveCompleted == null) m_onObjectiveCompleted = new UnityEvent<ScenarioObjective>();
                return m_onObjectiveCompleted;
            }
            set => m_onObjectiveCompleted = value; 
        }
        #endregion

        #region Initialization
        public ScenarioInstance(ScenarioAsset _scenario)
        {
            // Cache source reference
            m_scenario = _scenario;
        }

        public virtual void Init()
        {
            // Set isActive
            m_isActive = true;
        }
        #endregion

        #region Update Logic
        public virtual void Tick(float _deltaTime)
        {
            // Make sure scenario is active
            if (!m_isActive) return;
            // Increment global timer
            GlobalScenarioTimer += _deltaTime;
        }
        #endregion

        #region Completion-Related Functionality
        public void ClearScenario()
        {
            m_isActive = false;
        }
        #endregion

        #region Objective-Related Functionality
        public void AddClearSiteObjective(ContaminationSite _site)
        {
            // Create a new objective based on this contamination site
            ClearSiteScenarioObjective siteObjective = new ClearSiteScenarioObjective(this, _site);
            // Add this to the list of objectives
            AddObjective(siteObjective);
        }

        public void AddObjective(ScenarioObjective _objective)
        {
            // Add this to the list of objectives
            Objectives.Add(_objective);
            // Increase objective count
            m_objectiveCount++;
        }

        public void CompleteObjective(ScenarioObjective _objective)
        {
            // Make sure there are active objectives
            if (m_objectives == null || m_objectives.Count < 1 || !m_objectives.Contains(_objective)) return;

            // Remove this objective from the active objectives and add it to the completed ones
            Objectives.Remove(_objective);
            CompletedObjectives.Add(_objective);

            // Call any necessary events
            m_onObjectiveCompleted?.Invoke(_objective);

            // Check for scenario completion
            if (Objectives.Count < 1 && !IsCleared)
            {
                // Clear the scenario
                ClearScenario();
            }
        }
        #endregion

        #region Contamination-Site Related Objectives
        public void ClearSite()
        {
            // Do the thing
        }

        public virtual void LoadContaminantSites(List<ContaminationSite> _sites)
        {
            // Initialize the active site list
            m_contaminantSiteMasterList = new List<ContaminationSite>();
            m_activeSites = new List<ContaminationSite>();
            // Loop through each site, add it to the active ones, and create an objective 
            foreach(ContaminationSite site in _sites)
            {
                // Add it to the active site list
                m_activeSites.Add(site);
                m_contaminantSiteMasterList.Add(site);
                UnityEngine.Debug.Log($"Scenario Instance - Added Contaminant Site: {site.gameObject.name} || Time: {Time.time}");
                // Create a clear site objective for this
                AddClearSiteObjective(site);
            }
        }

        public virtual void InitContaminantSites()
        {
            // Do the thing
        }
        #endregion

        #region Score-Related Functionality
        public virtual void CalculateScore()
        {
            if (!ScoreManager.Instance) return;
        }

        public virtual EndScenarioInfo GenerateEndInfo()
        {
            return null;
        }
        #endregion

        #region On-Exit Functionality
        public virtual void OnExit()
        {
            // Remove all event listeners
            OnObjectiveCompleted.RemoveAllListeners();
            OnTimerTick.RemoveAllListeners();
            // Clear objective lists
        }
        #endregion
    }
}