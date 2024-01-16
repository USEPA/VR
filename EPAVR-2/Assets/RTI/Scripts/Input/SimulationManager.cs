using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SimulationManager : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Simulation Configuration")]
        [SerializeField] List<ScenarioAsset> m_scenarios;
        [SerializeField] List<SampleArea> m_sampleAreas;
        [SerializeField] int m_defaultStage = 0;
        [SerializeField] Transform m_toolSpawnPoint;
        [SerializeField] SampleArea m_defaultArea = default;
        [SerializeField] SampleToolOld m_defaultTool = default;

        [SerializeField] bool m_debugMode = false;
        [Header("Scene References")]
        [SerializeField] SimulationStageGUI m_stageGUI = default;
        [SerializeField] L58XRHand m_leftHand;
        [SerializeField] L58XRHand m_rightHand;
        [Header("Asset References")]
        [SerializeField] XRInputReferences m_inputRefs;
        #endregion
        #region Private Variables
        private Dictionary<ScenarioAsset, List<SampleReportOld>> m_stageReports;
        private ScenarioAsset m_currentScenario;
        private ScenarioStep m_currentStageStep;
        private SampleArea m_currentSampleArea;
        private SampleTool m_currentSampleTool;

        private int m_currentStageIndex = -1;
        private int m_currentStepIndex = -1;

        private bool m_simulationCompleted = false;
        private Action<ScenarioAsset> m_onNewStage;
        private Action<ScenarioStep> m_onNewStep;
        private Action<SampleReportOld> m_onSamplingStepComplete;
        private Action<ScenarioAsset> m_onSimulationComplete;
        #endregion
        #region Public Properties
        public bool SimulationCompleted { get => m_simulationCompleted; }
        public XRInputReferences InputRefs { get => m_inputRefs; }
        public L58XRHand LeftHand { get => m_leftHand; }
        public L58XRHand RightHand { get => m_rightHand; }

        public List<ScenarioAsset> SimulationStages { get => m_scenarios; }

        public ScenarioAsset CurrentScenario { get => m_currentScenario; }
        public ScenarioStep CurrentStageStep { get => m_currentStageStep; }

        public List<SampleArea> SampleAreas { get => m_sampleAreas; }
        public SampleArea CurrentSampleArea { get => m_currentSampleArea; }
        public SampleTool CurrentSampleTool { get => m_currentSampleTool; }

        public Dictionary<ScenarioAsset, List<SampleReportOld>> StageReports { get => m_stageReports; }

        public bool DebugMode { get => m_debugMode; }
        #endregion

        public static SimulationManager Instance;

        #region Initialization
        private void Awake()
        {
            // Set singleton
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Set up actions
            m_onNewStage += i => m_stageGUI.UpdateStageGUI(i);
            m_onNewStep += i => m_stageGUI.UpdateStepGUI(i);
            m_onSamplingStepComplete += i => m_stageGUI.UpdateReport(i);
            m_onSimulationComplete += i => m_stageGUI.ClearVideo(i);
            // Initialize systems
            m_stageGUI.Init();
            foreach (SampleArea sampleArea in m_sampleAreas) sampleArea.Init(CoreGameManager.Instance.Config.GetRandomChemical());
            // Start simulation
            StartSimulation();
            //
        }

        private void OnEnable()
        {
            if (m_inputRefs.ActionAsset != null)
            {
                m_inputRefs.ActionAsset.Enable();
            }
        }

        /// <summary>
        /// Initializes the dictionary of sample reports organized by the step of the current stage
        /// </summary>
        private void InitStageReportDictionary()
        {
            // Create the dictionary
            m_stageReports = new Dictionary<ScenarioAsset, List<SampleReportOld>>();
            // Loop through the stage list and add an item to the dictionary accordingly
            foreach(ScenarioAsset stage in m_scenarios)
            {
                // Create a new list to hold all of the reports for each step
                List<SampleReportOld> stageStepReport = new List<SampleReportOld>(stage.Steps.Count);
                // Add to the dictionary
                m_stageReports.Add(stage, stageStepReport);
            }
        }
        #endregion

        #region Scenario Functionality
        public void SetScenario(ScenarioAsset _scenario)
        {
            m_currentScenario = _scenario;
        }
        #endregion

        #region Simulation Stage Functionality
        /// <summary>
        /// Ends gameplay loop and clears values appropriately
        /// </summary>
        public void CompleteSimulation()
        {
            m_onSimulationComplete?.Invoke(m_currentScenario);
            m_stageGUI.gameObject.SetActive(false);

            m_currentStageIndex = -1;
            m_currentStepIndex = -1;
            m_currentScenario = null;
            m_currentStageStep = null;

            m_simulationCompleted = true;

            m_stageGUI.ButtonPrompt.text = "Restart";
        }

        /// <summary>
        /// Begiins the simulation, initializing values and setting defaults
        /// </summary>
        public void StartSimulation()
        {
            // Initialize the stage report dictionary
            InitStageReportDictionary();
            // Initialize defaults
            SetSampleArea(m_defaultArea);
            // Set initial values
            m_simulationCompleted = false;
            m_stageGUI.gameObject.SetActive(true);
            m_stageGUI.ReportGUI.gameObject.SetActive(false);
            // Initialize current stage
            SetStage(m_defaultStage);
        }
        
        /// <summary>
        /// Ends the current stage and either moves on to next one in the list or completes simulation
        /// </summary>
        public void CompleteStage()
        {
            // Disable the sample area
            if (m_currentSampleArea != null) m_currentSampleArea.HideArea();
            int nextStage = m_currentStageIndex + 1;
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
                SampleReportOld report = m_currentSampleTool.GenerateReport(m_currentStageStep);
                m_onSamplingStepComplete?.Invoke(report);
            }
            // Check if this would complete the stage
            if (nextStep < m_currentScenario.Steps.Count)
            {
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


        public void CompleteStageStep(SampleReportOld _report)
        {
            // Set the report of this stage step
            m_stageReports[m_currentScenario][m_currentStepIndex] = _report;
            // Call base functionality for completing stage step
            CompleteStageStep();
        }


        public void SetStage(int _index)
        {
            // If this is a valid index, set the stage
            if (_index >= 0 && _index < m_scenarios.Count) 
            {
                // Set current stage
                m_currentStageIndex = _index;
                m_currentScenario = m_scenarios[m_currentStageIndex];
                // Instantiate current tool if necessary
                //if (!m_currentSampleTool || (m_currentSampleTool.Type != m_currentScenario.ToolPrefab.Type)) SpawnSampleTool();
                // Set stage title display
                m_onNewStage?.Invoke(m_currentScenario);
                // Reset step index
                m_currentStepIndex = 0;
                SetStageStep(m_currentStepIndex);
            }
        }

        public void SetStageStep(int _index)
        {
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
        }
        #endregion

        #region Sample Area/Tool Functionality
        public void SetSampleArea(SampleArea _sampleArea)
        {
            // If there is already a sample area, unsubscribe it from the next step action
            if (m_currentSampleArea != null)
            {
                m_onNewStep -= i => m_currentSampleArea.ConfigureSampleArea(i);
                m_currentSampleArea.OnSetMovable -= i => m_stageGUI.Fade(i);
            }
                
            // Set the new reference and subscribe it to next step action
            m_currentSampleArea = _sampleArea;
            m_onNewStep += i => m_currentSampleArea.ConfigureSampleArea(i);
            m_currentSampleArea.OnSetMovable += i => m_stageGUI.Fade(i);
        }

        public void SetSampleTool(SampleTool _sampleTool)
        {
            // Set reference
            m_currentSampleTool = _sampleTool;
            // Initialize the tool
            m_currentSampleTool.SetSampleArea(m_currentSampleArea);
            // Hook up actions
            //m_inputRefs.GetAction(XRActionMap.UI, XRAction.Position).action.performed += i => m_currentSampleTool.SetDebugPosition(i.ReadValue<Vector2>());

            //m_inputRefs.GetAction(XRActionMap.UI, XRAction.PrimaryButton).action.performed += i => m_currentSampleTool.ToggleActive();
        }
        public void SpawnSampleTool()
        {
            // If there is a pre-existing sample tool, destroy it
            if (m_currentSampleTool != null) 
            {
                // Remove input actions
                //m_inputRefs.GetAction(XRActionMap.UI, XRAction.Position).action.performed -= i => m_currentSampleTool.SetDebugPosition(i.ReadValue<Vector2>());
                //m_inputRefs.GetAction(XRActionMap.UI, XRAction.PrimaryButton).action.performed -= i => m_currentSampleTool.ToggleActive();
                // Destroy the tool
                Destroy(m_currentSampleTool.gameObject);
            } 
            // Create the necessary sample tool
            //m_currentSampleTool = Instantiate(m_currentStage.ToolPrefab, m_toolSpawnPoint.position, m_toolSpawnPoint.rotation);
            //SetSampleTool(Instantiate(m_currentScenario.ToolPrefab, m_toolSpawnPoint.position, m_toolSpawnPoint.rotation));
           
        }
        #endregion
        #region Input Helpers
        public XRActionMap GetHandMap(L58XRHand _hand)
        {
            if (_hand == m_leftHand)
                return XRActionMap.LeftHand;
            else
                return XRActionMap.RightHand;
        }
        #endregion

        #region Debug Functionality
        public void InitNonVRDebug()
        {
            // Set events
            //m_inputRefs.GetAction(XRActionMap.Advisor, XRAction.SecondaryButton).action.performed += i => UnityEngine.Debug.Log($"What || Time: {Time.time}");
        }
        #endregion

        void TestSpacePress(UnityEngine.InputSystem.InputAction.CallbackContext _context)
        {
            UnityEngine.Debug.Log($"Space pressed || Time: {Time.time}");
            DebugManager.Instance.OnSpacePressed?.Invoke();
        }
    }
}

