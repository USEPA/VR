using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace L58.EPAVR
{
    public class ScoreManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private ScoreConfig m_scoreConfig;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;

        private Dictionary<ScoreType, int> m_scores;
        private Dictionary<PenaltyType, int> m_penalties;
        private int m_maxPossibleScore = 0;
        private ScenarioResults m_scenarioResults;

        private ScenarioManager m_scenarioManager;
        private float m_maxBonusTime = 600.0f;
        private int m_maxSiteClearScoreValue = 0;
        private int m_maxCompletionScoreValue = 0;
        private int m_unsafeChemicalContactPenalty = 20;
        private int m_maxUnsafeChemicalContactPenalty = 200;
        #endregion
        #region Public Properties
        public static ScoreManager Instance { get; set; }
        public ManagerStatus Status => m_status;

        public int MaxPossibleScore { get => m_maxPossibleScore; }
        public int MaxSiteClearScoreValue { get => m_maxSiteClearScoreValue; }
        public int SiteClearValue { get => m_scoreConfig.SiteClearValue; }
        public int MaxTimeBonusValue { get => m_scoreConfig.MaxTimeBonusValue; }
        public int UnsafeChemicalHandlingPenalty { get => m_scoreConfig.UnsafeChemicalContactPenaltyValue; }
        public int MaxUnsafeChemicalHandlingPenalty { get => m_scoreConfig.MaxUnsafeChemicalContactPenaltyValue; }

        public Dictionary<ScoreType, int> Scores { get => m_scores; }
        public Dictionary<PenaltyType, int> Penalties { get => m_penalties; }
        public ScenarioResults ScenarioResults { get => m_scenarioResults; }
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
            // Clear references if needed
            m_scenarioResults = null;
            // Cache references if necessary
            if (!m_scenarioManager && ScenarioManager.Instance != null) m_scenarioManager = ScenarioManager.Instance;
            // Create the score objects
            InitScores();
            // Calculate max possible score
            CalculateMaxPossibleScore();
            // Hook up events
            if (m_scenarioManager)
            {
                // Cache max bonus time
                m_maxBonusTime = m_scenarioManager.CurrentScenario.MaxBonusTime;
                m_scenarioManager.OnObjectiveCompleted.AddListener(AddObjectiveScore);

                //m_scenarioManager.OnSiteCleared += (i, j) => AddScore(ScoreType.SitesCleared, 100);
                //m_scenarioManager.OnScenarioExit += (i, j) => GenerateScenarioResults(i, j);
                m_scenarioManager.OnScenarioExited.AddListener(GenerateScenarioResults);
            }
            // Finish initialization
            UnityEngine.Debug.Log($"ScoreManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }

        void InitScores()
        {
            if (m_scores != null && m_scores.Count > 0) m_scores.Clear();
            m_scores = new Dictionary<ScoreType, int>();
            //var scoreTypes = EnumHelper.GetValues<ScoreType>().ToList();
            var scoreTypes = CoreGameManager.Instance.CurrentGamemodeConfig.ScoreTypes;
            for (int i = 0; i < scoreTypes.Count; i++)
            {
                ScoreType type = scoreTypes[i];
                m_scores.Add(type, 0);
                //UnityEngine.Debug.Log($"Init score: {type} | Value: {m_scores[type]} || Time: {Time.time}");
            }
            m_penalties = new Dictionary<PenaltyType, int>();
        }

        void CalculateMaxPossibleScore()
        {
            if (!m_scenarioManager) return;

            switch (CoreGameManager.Instance.CurrentGamemode)
            {
                case Gamemode.ChemicalHunt:
                    // Get max site clear value
                    m_maxSiteClearScoreValue = SiteClearValue * m_scenarioManager.ContaminantSiteCount;
                    m_maxCompletionScoreValue = m_maxSiteClearScoreValue;
                    break;
                case Gamemode.RadiationSurvey:
                    // Get radiation spread max score values;
                    m_maxSiteClearScoreValue = 500;
                    int maxSiteEstimateScoreValue = 200;
                    m_maxCompletionScoreValue = m_maxSiteClearScoreValue + maxSiteEstimateScoreValue;
                    break;
                default:
                    m_maxCompletionScoreValue = 500;
                    break;
            }
            // Get max site clear value
            int maxTimeBonus = (CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt) ? MaxTimeBonusValue : 0;
            // Simply add up values
            m_maxPossibleScore = m_maxCompletionScoreValue + maxTimeBonus;
            //m_maxPossibleScore = m_maxSiteClearScoreValue + maxTimeBonus;
        }
        #endregion

        #region Score-Related Functionality
        public void AddScore(ScoreType _type, int _value)
        {
            UnityEngine.Debug.Log($"Added score: {_type} | Value: {_value} || Time: {Time.time}");
            if (m_scores == null || !m_scores.ContainsKey(_type)) return;
            // Increase score value
            m_scores[_type] += _value;
        }

        public void AddObjectiveScore(ScenarioObjective _objective)
        {
            if (m_scores == null || !m_scores.ContainsKey(_objective.ScoreType)) return;
            // Increase score value
            m_scores[_objective.ScoreType] += _objective.ScorePoints;
        }

        public void AddPenalty(PenaltyType _type, int _value)
        {
            // Initialize penalty dictionary if needed
            if (m_penalties == null) m_penalties = new Dictionary<PenaltyType, int>();
            if (m_penalties.ContainsKey(_type))
            {
                m_penalties[_type] -= _value;
            }
            else
            {
                UnityEngine.Debug.Log($"ScoreManager - Added Penalty: {_type} | Value: {_value} || Time: {Time.time}");
                m_penalties.Add(_type, -_value);
            }
            // Increase penalty value
            AddScore(ScoreType.Penalty, -_value);
        }

        public int CalculateTimeBonus(float _time)
        {
            if (m_scenarioManager == null || m_scenarioManager.ActiveObjectiveCount > 0) return 0;

            if (_time >= m_maxBonusTime)
            {
                return 0;
            }
            else if (_time >= (m_maxBonusTime * 0.75f))
            {
                return (int) (MaxTimeBonusValue * 0.25f);
            }
            else if (_time >= (m_maxBonusTime * 0.5f))
            {
                return (int)(MaxTimeBonusValue * 0.5f);
            }
            else
            {
                return MaxTimeBonusValue;
            }
        }
        #endregion

        #region Penalty-Related Functionality
        public void CalculateActiveToolPenalty()
        {
            if (!VRUserManager.Instance || !VRUserManager.Instance.Player) return;
            // Get how many tools are currently active in the simulation
            int activeToolCount = VRUserManager.Instance.Player.GetActiveItemCount();

            int penalty = Mathf.Clamp(10 * activeToolCount, 0, 300);
            if (penalty > 0) AddPenalty(PenaltyType.AbandonedEquipment, penalty);
        }

        public void AddUnsafeChemicalContactPenalty()
        {
            // Add the penalty
            AddPenalty(PenaltyType.UnsafeChemicalHandling, UnsafeChemicalHandlingPenalty);
            // Clamp the penalty
            m_penalties[PenaltyType.UnsafeChemicalHandling] = Mathf.Clamp(m_penalties[PenaltyType.UnsafeChemicalHandling], 0, MaxUnsafeChemicalHandlingPenalty);
        }
        #endregion

        #region Grading-Related Functionality
        public void GenerateScenarioResults(ScenarioInstance _scenarioInstance, float _time)
        {
            ScenarioAsset scenario = _scenarioInstance.Scenario;
            // Check base conditions
            if (scenario == null) return;
            if (CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt)
            {
                // Get time bonus
                m_scores[ScoreType.TimeBonus] = CalculateTimeBonus(_time);
            }
            // Calculate active tool penalty (if applicable)
            CalculateActiveToolPenalty();
            // Total out scores
            List<ScoreItem> finalScores = new List<ScoreItem>();
            List<ScoreItem> penalties = new List<ScoreItem>();
            foreach (KeyValuePair<ScoreType, int> item in m_scores)
            {
                ScoreItem scoreDisplay;
                // Add the score
                switch (item.Key)
                {
                    case ScoreType.SitesCleared:
                        scoreDisplay = new ScoreItem(EnumHelper.ToFormattedText(item.Key), item.Value, m_maxSiteClearScoreValue);
                        break;
                    case ScoreType.EstimatedSpread:
                        scoreDisplay = new ScoreItem(EnumHelper.ToFormattedText(item.Key), item.Value, m_maxSiteClearScoreValue);
                        break;
                    case ScoreType.EstimatedEpicenter:
                        scoreDisplay = new ScoreItem(EnumHelper.ToFormattedText(item.Key), item.Value, 200);
                        break;
                    default:
                        scoreDisplay = new ScoreItem(EnumHelper.ToFormattedText(item.Key), item.Value);
                        break;
                }
                finalScores.Add(scoreDisplay);
            }

            if (m_penalties.Count > 0)
            {
                foreach(KeyValuePair<PenaltyType, int> penalty in m_penalties)
                {
                    // Add the penalty
                    ScoreItem penaltyDisplay = new ScoreItem(EnumHelper.ToFormattedText(penalty.Key), penalty.Value);
                    penalties.Add(penaltyDisplay);
                }
                UnityEngine.Debug.Log($"ScoreManager - Found Penalties: {penalties.Count} || Time: {Time.time}");
            }
            ScenarioResults results = new ScenarioResults(scenario, _time, finalScores, penalties);
            results.Grade = CalculateGrade(results.TotalScore);
            m_scenarioResults = results;
        }

        public void GenerateScenarioResults(ScenarioInstance _scenarioInstance)
        {
            GenerateScenarioResults(_scenarioInstance, _scenarioInstance.GlobalScenarioTimer);
        }

        public float CalculateGrade(int _score)
        {
            float grade = (float) _score / (float) m_maxPossibleScore;
            UnityEngine.Debug.Log($"ScoreManager calculated grade: {grade * 100.0f}% | Total Score: {_score} | Max Possible Score: {m_maxPossibleScore} || Time: {Time.time}");
            return grade;
        }

        public int GetScoreValue(ScoreType _scoreType)
        {
            if (m_scores == null || m_scores.Count < 1 || !m_scores.ContainsKey(_scoreType)) return 0;
            return m_scores[_scoreType];
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin reset
            m_status = ManagerStatus.Resetting;
            // Reset events
            if (m_scenarioManager)
            {
                //m_scenarioManager.OnObjectiveCompleted.RemoveListener(AddObjectiveScore);
                m_scenarioManager.OnScenarioExited.RemoveListener(GenerateScenarioResults);
                //m_scenarioManager.OnSiteCleared -= (i, j) => AddScore(ScoreType.SitesCleared, 100);
                //m_scenarioManager.OnScenarioExit -= (i, j) => GenerateScenarioResults(i, j);
            }
            // Finish reset
            UnityEngine.Debug.Log($"ScoreManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
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

        }
    }
}

