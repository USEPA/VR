using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class ScenarioManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private int m_contaminantSiteCount = 3;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;
        private ScenarioInstance m_currentScenarioInstance;

        private ScenarioAsset m_currentScenario;
        private Difficulty m_difficulty;

        private ScenarioStep m_currentStageStep;

        private int m_currentStageIndex = -1;
        private int m_currentStepIndex = -1;

        private List<SampleArea> m_sampleAreas;
        private List<ChemicalContaminantSite> m_contaminantSites;
        private List<ChemicalContaminantSite> m_clearedSites;

        private bool m_scenarioActive = false;
        private float m_globalScenarioTimer = 0.0f;
        private bool m_simulationCompleted = false;
        private Dictionary<ScoreType, int> m_scores;
        private ScenarioResults m_scenarioResults;
        private EndScenarioInfo m_endScenarioInfo;

        private SimulationStageGUI m_stageGUI;
        private Action<ScenarioAsset> m_onNewStage;
        private Action<ScenarioStep> m_onNewStep;
        private Action<float> m_onGlobalScenarioTimerTick;
        private Action<SampleReportOld> m_onSamplingStepComplete;
        private Action<ScenarioAsset> m_onSimulationComplete;
        private Action<ChemicalContaminantSite, Sample> m_onSiteCleared;
        private Action<ScenarioAsset, float> m_onScenarioExit;
        private UnityEvent<ScenarioInstance> m_onScenarioExited;
        #endregion
        #region Public Properties
        public static ScenarioManager Instance { get; set; }
        public ManagerStatus Status => m_status;
        public ScenarioAsset CurrentScenario { get => m_currentScenario; }
        public ScenarioInstance CurrentScenarioInstance { get => m_currentScenarioInstance; }
        public Difficulty Difficulty { get => m_difficulty; }
        public Gamemode CurrentGamemode { get => m_currentScenario.Mode; }
        public int ContaminantSiteCount { get => m_contaminantSiteCount; }
        public int ClearedSiteCount { get => (m_clearedSites != null) ? m_clearedSites.Count : 0; }

        public Dictionary<ScoreType, int> Scores { get => m_scores; }
        public ScenarioStep CurrentStageStep { get => m_currentStageStep; }

        public List<SampleArea> SampleAreas { get => m_sampleAreas; }
        public List<ContaminationSite> ContaminantSites 
        { 
            get 
            {
                if (m_currentScenarioInstance == null || m_currentScenarioInstance.ContaminantSites == null) return null;
                return m_currentScenarioInstance.ContaminantSites;
            } 
        }

        public SimulationStageGUI StageGUI { get => m_stageGUI; }
        public bool ScenarioActive { get => m_scenarioActive; }
        public float GlobalScenarioTimer 
        {
            get => m_currentScenarioInstance.GlobalScenarioTimer;
            /*
            set 
            { 
                m_globalScenarioTimer = value;
                m_onGlobalScenarioTimerTick?.Invoke(m_globalScenarioTimer);
            }
            */
        }

        public EndScenarioInfo EndScenarioInfo { get => m_endScenarioInfo; set => m_endScenarioInfo = value; }

        public Action<float> OnGlobalScenarioTimerTick { get => m_onGlobalScenarioTimerTick; set => m_onGlobalScenarioTimerTick = value; }
        public Action<ChemicalContaminantSite, Sample> OnSiteCleared { get => m_onSiteCleared; set => m_onSiteCleared = value; }
        public Action<ScenarioAsset, float> OnScenarioExit { get => m_onScenarioExit; set => m_onScenarioExit = value; }

        public UnityEvent<ScenarioInstance> OnScenarioExited 
        { 
            get
            {
                if (m_onScenarioExited == null) m_onScenarioExited = new UnityEvent<ScenarioInstance>();
                return m_onScenarioExited;
            }
        }

        public int CompletedObjectiveCount
        {
            get => (m_currentScenarioInstance != null) ? m_currentScenarioInstance.CompletedObjectives.Count : 0;
        }

        public int ObjectiveCount
        {
            get => (m_currentScenarioInstance != null) ? m_currentScenarioInstance.ObjectiveCount : 0;
        }

        public int ActiveObjectiveCount
        {
            get => (m_currentScenarioInstance != null) ? m_currentScenarioInstance.ActiveObjectiveCount : 0;
        }

        public UnityEvent<ScenarioObjective> OnObjectiveCompleted
        {
            get
            {
                if (m_currentScenarioInstance == null) return null;
                return m_currentScenarioInstance.OnObjectiveCompleted;
            }
        }
        #endregion

        #region Initialization
        void Awake()
        {
            // Set singleton
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            // Create a scenario instance based on the active gamemode
            //m_currentScenarioInstance = m_currentScenario.CreateInstance();
            // Initialize according to the active gamemode
            Init(m_currentScenario.Mode);
      
            // Hook up VRUserManager events
            VRUserManager.Instance.OnSetSampleArea += i => ConfigureSampleArea(i);
            // Set default sample area
            if (m_sampleAreas != null && m_sampleAreas.Count > 0) VRUserManager.Instance.SetSampleArea(m_sampleAreas[0]);
            // Setup cleared sites
            // Initialize score values
            //InitScores();
            
            // Finish initialization
            UnityEngine.Debug.Log($"ScenarioManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }

        private void Init(Gamemode _mode)
        {
            // Call initialization logic in the scenario instance
            m_currentScenarioInstance.Init();
            /*
            // Check which game mode this is
            switch (_mode)
            {
                case Gamemode.Tutorial:
                    // Check if the tutorial has steps
                    if (m_currentScenario.Steps != null && m_currentScenario.Steps.Count > 0)
                    {
                        // Set default step
                        m_currentStepIndex = 0;
                        SetStageStep(0);
                    }
                    break;
                default:
                    // Initialize all contaminant sites
                    InitContaminantSites(_mode);
                    break;
            }
            */


            /*
            // Check if there are sample areas that need to be initialized
            if (m_sampleAreas != null && m_sampleAreas.Count > 0)
            {
                // Initialize all sample areas with a random chemical
                foreach (SampleArea sampleArea in m_sampleAreas) sampleArea.Init(CoreGameManager.Instance.Config.GetRandomChemical());
            }
            */
            // Check if there is a valid GUI
            if (m_stageGUI != null)
            {
                // Hook up new step event
                m_onNewStep += i => m_stageGUI.UpdateStepGUI(i);
                // Hook up complete scenario step button
                m_stageGUI.CompleteStepButton.onClick.AddListener(CompleteStageStep);
                // Initialize GUI
                m_stageGUI.Init();
                // Update the scenario title
                m_stageGUI.UpdateStageGUI(m_currentScenario);
            }
            /*
            // Set up global scenario timer
            m_globalScenarioTimer = 0.0f;
            m_scenarioActive = true;
            */
        }

        public void LoadSampleAreas(List<SampleArea> _sampleAreas)
        {
            // Initialize sample area list if necessary
            if (m_sampleAreas == null)
                m_sampleAreas = new List<SampleArea>();
            else if (m_sampleAreas.Count > 0)
                m_sampleAreas.Clear();

            // Loop through each new sample area and add it to the master list
            foreach (SampleArea sampleArea in _sampleAreas) m_sampleAreas.Add(sampleArea);
        }

        public void LoadContaminantSites(List<ContaminationSite> _contaminantSites)
        {
            // Get the scenario instance
            if (m_currentScenarioInstance == null) return;
            m_currentScenarioInstance.LoadContaminantSites(_contaminantSites);
        }
        public void LoadContaminantSites(List<ChemicalContaminantSite> _contaminantSites)
        {
            // Initialize contaminant site list if necessary
            if (m_contaminantSites == null)
                m_contaminantSites = new List<ChemicalContaminantSite>();
            else if (m_contaminantSites.Count > 0)
                m_contaminantSites.Clear();

            // Loop through each new contaminant site and add it to the master list
            foreach (ChemicalContaminantSite contaminantSite in _contaminantSites) m_contaminantSites.Add(contaminantSite);
            // Make sure contaminant site count matches
            m_contaminantSiteCount = m_contaminantSites.Count;
        }

        public void LoadStageGUI(SimulationStageGUI _gui)
        {
            // Cache GUI reference
            m_stageGUI = _gui;
        }
        public void StartSimulation()
        {
            // Set initial values
            m_simulationCompleted = false;
            m_stageGUI.gameObject.SetActive(true);
            m_stageGUI.ReportGUI.gameObject.SetActive(false);
        }
        #endregion

        #region Contamination-Related Functionality
        void InitContaminantSites(Gamemode _mode = Gamemode.ChemicalHunt)
        {
            // Make sure there are valid contaminant sites
            if (m_contaminantSites == null || m_contaminantSites.Count < 1) return;
            UnityEngine.Debug.Log($"ScenarioManager - entered InitContaminantSites: {m_currentScenario.Mode} || Time: {Time.time}");
            // Pull the contaminant distribution from scenario data
            ChemicalDistribution possibleChemicalDistribution = null;
            if (_mode == Gamemode.ChemicalHunt) possibleChemicalDistribution = new ChemicalDistribution(m_currentScenario.SpawnableChemicalDistribution);
            // Loop through each contaminant site and figure out which chemical to spawn
            foreach(ChemicalContaminantSite site in m_contaminantSites)
            {
                UnityEngine.Debug.Log($"ScenarioManager - InitContaminantSites | Arrived in initializtion loop with site: {site.gameObject.name} || Time: {Time.time}");
                // Check if this site needs a chemical initialized
                if (_mode == Gamemode.ChemicalHunt)
                {
                    // Get a chemical based on the distribution 
                    ChemicalAgent chemical = site.SelectChemicalFromDistribution(possibleChemicalDistribution);
                    //ChemicalAgent chemical = m_currentScenario.GetRandomChemical();
                    // Initialize the contaminant site with the selected chemical
                    site.Init(chemical);
                }
                // Hook up events
                //site.OnSiteCleared += (i, j) => ClearSite(site, j);
            }
            // Initialize cleared site list
            m_clearedSites = new List<ChemicalContaminantSite>();

            UnityEngine.Debug.Log($"Total Contaminant Sites: {m_contaminantSites.Count} || Time: {Time.time}");
        }

        #endregion

        #region Scenario-Related Functionality
        public void SetScenario(ScenarioAsset _scenario)
        {
            // Set the scenario reference
            m_currentScenario = _scenario;
            // Create a scenario instance based on the active gamemode
            m_currentScenarioInstance = m_currentScenario.CreateInstance();

            // Make sure contaminant site count was configured properly
            ConfigureContaminantSiteCount();
        }

        public void SetDifficulty(Difficulty _difficulty)
        {
            m_difficulty = _difficulty;
            switch (CoreGameManager.Instance.CurrentGamemode)
            {
                case Gamemode.ChemicalHunt:
                    // Configure settings by difficulty
                    switch (m_difficulty)
                    {
                        case Difficulty.Easy:
                            // Set number of sites
                            m_contaminantSiteCount = 5;
                            // Set time multiplier
                            break;
                        case Difficulty.Medium:
                            // Set number of sites
                            m_contaminantSiteCount = 5;
                            // Set time multiplier

                            break;
                        case Difficulty.Hard:
                            // Set number of sites
                            m_contaminantSiteCount = 8;
                            // Set time multiplier

                            break;
                    }
                    break;
                case Gamemode.RadiationSurvey:
                    // Set contaminant site count to 1
                    m_contaminantSiteCount = 1;
                    break;
            }

        }

        public void ConfigureContaminantSiteCount()
        {
            switch (CoreGameManager.Instance.CurrentGamemode)
            {
                case Gamemode.ChemicalHunt:
                    // Configure settings by difficulty
                    switch (m_difficulty)
                    {
                        case Difficulty.Easy:
                            // Set number of sites
                            m_contaminantSiteCount = 5;
                            // Set time multiplier
                            break;
                        case Difficulty.Medium:
                            // Set number of sites
                            m_contaminantSiteCount = 5;
                            // Set time multiplier

                            break;
                        case Difficulty.Hard:
                            // Set number of sites
                            m_contaminantSiteCount = 8;
                            // Set time multiplier

                            break;
                    }
                    break;
                case Gamemode.RadiationSurvey:
                    // Set contaminant site count to 1
                    m_contaminantSiteCount = 1;
                    break;
            }
        }

        /// <summary>
        /// Ends the current stage and either moves on to next one in the list or completes simulation
        /// </summary>
        public void CompleteStage()
        {
            // Disable the sample area
            if (VRUserManager.Instance.CurrentSampleArea != null) VRUserManager.Instance.CurrentSampleArea.HideArea();
            int nextStage = m_currentStageIndex + 1;
            // Complete simulation
            CompleteSimulation();
            /*
            // Check if this would complete the simulation
            if (nextStage < m_currentStageIndex)
            {
                // Go to the next stage
                SetStage(nextStage);
            }
            else
            {
                // Complete the simulation
                CompleteSimulation();
            }
            */
        }

        public void CompleteSimulation()
        {
            m_onSimulationComplete?.Invoke(m_currentScenario);
            m_stageGUI.gameObject.SetActive(false);

            m_currentStageIndex = -1;
            m_currentStepIndex = -1;
            //m_currentScenario = null;
            m_currentStageStep = null;

            m_simulationCompleted = true;

            m_stageGUI.ButtonPrompt.text = "Restart";
        }

        public void CompleteScenario()
        {
            // Set state
            m_simulationCompleted = true;
            m_scenarioActive = false;
            // Invoke events
            m_onSimulationComplete?.Invoke(m_currentScenario);
        }

        public void EndScenario()
        {
            UnityEngine.Debug.Log($"Arrived in EndScenario || Time: {Time.time}");
            // Perform exit logic
            m_currentScenarioInstance.OnExit();
            m_onScenarioExited?.Invoke(m_currentScenarioInstance);
  
            //m_onScenarioExit?.Invoke(m_currentScenario, m_globalScenarioTimer);
            //GenerateScenarioResults();
            // Load the main menu
            CoreGameManager.Instance.LoadMainMenu();
        }

        private ScenarioResults GenerateScenarioResults()
        {
            if (m_currentScenario != null)
            {
                // Get time bonus
                //m_scores[ScoreType.TimeBonus] = CalculateTimeBonus();
                // Total out scores
                List<ScoreItem> finalScores = new List<ScoreItem>();
                foreach(KeyValuePair<ScoreType, int> item in m_scores)
                {
                    // Add the score
                    ScoreItem scoreDisplay = new ScoreItem(EnumHelper.ToFormattedText(item.Key), item.Value);
                    finalScores.Add(scoreDisplay);
                }
                ScenarioResults results = new ScenarioResults(m_currentScenario, m_globalScenarioTimer, finalScores);
                m_scenarioResults = results;
                return results;
            }
            return null;
        }
        #endregion

        #region Scenario Stage-Related Functionality
        public void SetStageStep(int _index)
        {
            UnityEngine.Debug.Log($"Arrived in SetStageStep: {_index} || Time: {Time.time}");
            // Get the stage step
            ScenarioStep newStep = m_currentScenario.GetStep(_index);
            if (newStep != null)
            {
                // Invoke any necessary actions
                m_onNewStep?.Invoke(newStep);
                // Set the cuurrent step display
                //m_stageGUI.UpdateStepGUI(_index, newStep);
                // Set current stage step
                m_currentStageStep = newStep;
            }
            else
            {
                UnityEngine.Debug.Log($"ERROR: Invalid stage step: {_index} || Time: {Time.time}");
            }
        }

        public void CompleteStageStep()
        {
            if (m_simulationCompleted)
            {
                // Restart simulation
                StartSimulation();
                return;
            }

            int nextStep = m_currentStepIndex + 1;
            // Check if sample area was active during this current step
            if (m_currentStageStep.EnableSampleArea)
            {
                // Generate report
                SampleReportOld report = VRUserManager.Instance.CurrentTool.GenerateReport(m_currentStageStep);
                m_onSamplingStepComplete?.Invoke(report);
            }
            // Check if this would complete the stage
            if (nextStep < m_currentScenario.Steps.Count)
            {
                UnityEngine.Debug.Log($"Arrived in CompleteStageStep | Current Step: {m_currentStepIndex} | Next Step: {nextStep} || Time: {Time.time}");
                // Set the stage step
                m_currentStepIndex = nextStep;
                SetStageStep(nextStep);
            }
            else
            {
                // Compete the stage
                CompleteStage();
            }
        }
        #endregion

        #region Contaminant Site-Related Functionality
        public void ClearSite(ChemicalContaminantSite _site, Sample _sample = null)
        {
            // Add this site to the cleared list and remove it from the active list
            m_clearedSites.Add(_site);
            m_contaminantSites.Remove(_site);
            // Remove event
            //_site.OnSiteCleared -= (i, j) => ClearSite(_site, j);
            // Invoke any necessary events
            //m_scores[ScoreType.SitesCleared] += 100;
            m_onSiteCleared?.Invoke(_site, _sample);
            // Check for scenario completion
            if (m_contaminantSites.Count < 1 && !m_simulationCompleted)
            {
                // Scenario is cleared!
                CompleteScenario();
            }
        }
        #endregion

        #region Helper Methods
        public void ConfigureSampleArea(SampleArea _sampleArea)
        {
            m_onNewStep += i => 
            {
                UnityEngine.Debug.Log($"SetSampleArea grid active: {i.EnableSampleArea} || Time: {Time.time}");
                _sampleArea.ConfigureSampleArea(i); 
            };
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin resetting
            m_status = ManagerStatus.Resetting;
            // Unhook events
            VRUserManager.Instance.OnSetSampleArea -= i => ConfigureSampleArea(i);
            m_onSiteCleared = null;
            m_onScenarioExit = null;
            m_onScenarioExited.RemoveAllListeners();
            // Clear contaminant site references
            //m_contaminantSites.Clear();
            //m_clearedSites.Clear();
            // Reset scenario timer
            m_globalScenarioTimer = 0.0f;
            m_scenarioActive = false;
            m_currentScenario = null;
            m_currentScenarioInstance = null;
            // Finish reset
            UnityEngine.Debug.Log($"ScenarioManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_currentScenarioInstance == null || (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != m_currentScenario.Scene)) return;
            m_currentScenarioInstance.Tick(Time.deltaTime);
            return;

            if (m_scenarioActive && m_currentScenario != null && (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_currentScenario.Scene))
            {
                //GlobalScenarioTimer += Time.deltaTime;
                /*
                if (GlobalScenarioTimer >= 5.0f && m_clearedSites.Count < 1) 
                {
                    int allOrOne = UnityEngine.Random.Range(0, 2);
                    if (allOrOne > 0)
                    {
                        List<ContaminantSite> sitesToClear = new List<ContaminantSite>();
                        for(int i = 0; i < m_contaminantSites.Count; i++)
                        {
                            sitesToClear.Add(m_contaminantSites[i]);
                        }

                        foreach(ContaminantSite site in sitesToClear)
                        {
                            ClearSite(site);
                        }
                        EndScenario();
                    }
                    else
                    {
                        ContaminantSite randomSite = m_contaminantSites[UnityEngine.Random.Range(0, m_contaminantSites.Count)];
                        ClearSite(randomSite);
                    }
           
                }

                if (GlobalScenarioTimer >= 10.0f)
                {
                    EndScenario();
                }
                */
            }
        }


    }

    public enum Difficulty {Easy, Medium, Hard}
}

