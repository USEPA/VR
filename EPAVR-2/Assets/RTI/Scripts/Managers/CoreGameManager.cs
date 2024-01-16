using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HurricaneVR.Framework.Core;

namespace L58.EPAVR
{
    public class CoreGameManager : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] CoreGameConfig m_config;
        [Header("Default Configuration")]
        [SerializeField] Gamemode m_defaultGamemode = Gamemode.ChemicalHunt;
        [Header("Debug Configuration")]
        [SerializeField] private int m_seed = -1;
        [SerializeField] private bool m_useSeed = false;
        [SerializeField] private bool m_usePaintSystem = true;
        #endregion
        #region Private Variables
        private GameState m_currentGameState;
        private List<IManager> m_startSequence;
        private int m_numModules = 0;
        private int m_numModulesReady = 0;
        private bool m_loadedAllManagers = false;

        private ScenarioManager m_scenarioManager;
        private PaintManager m_paintManager;
        private ContaminantManager m_contaminantManager;
        private ScoreManager m_scoreManager;
        private AdvisorManager m_advisorManager;
        private MapManager m_mapManager;
        private VRUserManager m_playerManager;
        private HVRManager m_hvrManager;

        private Gamemode m_currentGamemode = Gamemode.ChemicalHunt;
        private GamemodeConfigAsset m_currentGamemodeConfig;

        private Action<GameState> m_onSetGameState;
        private Action<GameState> m_onSceneLoadStarted;
        private Action<GameState> m_onStartupCompleted;
        #endregion
        #region Public Properties
        public static CoreGameManager Instance { get; set; }
        public CoreGameConfig Config { get => m_config; }

        public GameState CurrentState { get => m_currentGameState; }
        public Gamemode CurrentGamemode { get => m_currentGamemode; }

        public GamemodeConfigAsset CurrentGamemodeConfig { get => m_currentGamemodeConfig; }

        public bool UsePaintSystem { get => m_usePaintSystem; }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Set singleton
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            // Initialize variables
            m_startSequence = new List<IManager>();
            SetGamemode(m_defaultGamemode);
        }

        private void Start()
        {
            // Cache variables
            if (!m_scenarioManager && ScenarioManager.Instance != null) m_scenarioManager = ScenarioManager.Instance;
            if (!m_contaminantManager && ContaminantManager.Instance != null) m_contaminantManager = ContaminantManager.Instance;
            if (!m_paintManager && PaintManager.Instance != null) m_paintManager = PaintManager.Instance;
            if (!m_scoreManager && ScoreManager.Instance != null) m_scoreManager = ScoreManager.Instance;
            if (!m_playerManager && VRUserManager.Instance != null) m_playerManager = VRUserManager.Instance;
            if (!m_advisorManager && AdvisorManager.Instance != null) m_advisorManager = AdvisorManager.Instance;
            if (!m_mapManager && MapManager.Instance != null) m_mapManager = MapManager.Instance;
            if (!m_hvrManager && HVRManager.Instance != null) m_hvrManager = HVRManager.Instance;

            if (!HasXRDevicesLoaded()) UnityEngine.Debug.Log($"No XR Input Devices found || Time: {Time.time}");

            // Hook up events
            /*
            if (m_hvrManager != null)
            {
                m_onSceneLoadStarted += i => m_hvrManager.ScreenFade(1.0f, 10.0f);
                m_onStartupCompleted += i => m_hvrManager.ScreenFade(0.0f, 10.0f);
            }
            */
            if (ScreenFader.Instance != null)
            {
                m_onSceneLoadStarted += i => ScreenFader.Instance.Fade(1.0f);
                m_onStartupCompleted += i => ScreenFader.Instance.Fade(0.0f);
                //m_onSetGameState += i => ScreenFader.Instance.Fade(0.0f);
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Sets up the sequence of managers to be initialized in a given game state
        /// To be called upon loading any new scene
        /// </summary>
        /// <returns>Do the managers need to be called</returns>
        private bool SetupStartSequence()
        {
            UnityEngine.Debug.Log($"SetupStartSequence: {m_currentGameState} || Time: {Time.time}");
            // Check if we have existing managers in start sequence
            if (m_startSequence.Count > 0)
            {
                // Check if managers need to be reset
                ResetManagers();
                // Clear start sequence
                m_startSequence.Clear();
            }

            // Check for any state-specific functionality
            switch (m_currentGameState)
            {
                case GameState.InGame:
                    if (m_useSeed && m_seed != -1)
                    {
                        UnityEngine.Random.InitState(m_seed);
                    }
                    else
                    {
                        m_seed = UnityEngine.Random.seed;
                    }
                    if (m_usePaintSystem) m_startSequence.Add(m_paintManager);
                    m_startSequence.Add(m_scenarioManager);
                    m_startSequence.Add(m_contaminantManager);
                    m_startSequence.Add(m_scoreManager);
                    //m_startSequence.Add(m_advisorManager);
                    m_startSequence.Add(m_mapManager);
                    m_startSequence.Add(m_playerManager);
                    return true;
                default:
                    break;
            }
            // Override start up event
            m_onStartupCompleted?.Invoke(m_currentGameState);
            return false;
        }

        /// <summary>
        /// Iterates through the start up sequence and starts up the managers in order
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartupManagers()
        {
            // Get the number of modules to start up in the start sequence
            m_numModules = m_startSequence.Count;
            int readyBeforehand = 0;
            // Get all modules that are ready
            foreach (IManager manager in m_startSequence)
            {
                if (manager.Status == ManagerStatus.Started)
                {
                    // Increment the number of modules ready
                    readyBeforehand++;
                }
            }
            UnityEngine.Debug.Log($"Startup {readyBeforehand}/{m_numModules} Managers on frame: {Time.frameCount} | Game State: {m_currentGameState} || Time: {Time.time}");

            if (readyBeforehand >= m_numModules) yield break;

            // Loop through each manager in the start sequence
            for (int i = 0; i < m_startSequence.Count; i++)
            {
                //UnityEngine.Debug.Log($"Starting up manager[{i}]/{m_numModules} || Time: {Time.time}");
                // Startup the specified manager
                m_startSequence[i].Startup();
            }
            yield return null;

            // Set the number of modules ready to 0
            m_numModulesReady = 0;
            // Keep looping until all managers have been started up
            while (m_numModulesReady < m_numModules)
            {
                int lastReady = m_numModulesReady;
                m_numModulesReady = 0;
                foreach (IManager manager in m_startSequence)
                {
                    if (manager.Status == ManagerStatus.Started)
                    {
                        // Increment the number of modules ready
                        m_numModulesReady++;
                    }
                }
                // Check if the number of modules ready is greater than the last number of modules ready recorded
                if (m_numModulesReady > lastReady)
                {
                    // Debug.Log("Progress: " + numReady + "/" + numModules);
                }
                // Pause for one frame before checking again
                yield return null;
            }
            // Invoke any necessary actions
            m_onStartupCompleted?.Invoke(m_currentGameState);
            // Set managers loaded
            m_loadedAllManagers = true;
            UnityEngine.Debug.Log($"CoreGameManager finished loading managers: {m_numModules} | State: {m_currentGameState} || Time: {Time.time}");
        }

        private void ResetManagers()
        {
            // Loop through all managers and reset them
            foreach(IManager manager in m_startSequence)
            {
                manager.ResetToStart();
            }
        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }

        #region State-Related Functionality
        public void SetGameStateOverride(GameState newState)
        {
            // Make sure this isn't already the current state
            if (m_currentGameState == newState) return;

            // Set the game state
            m_currentGameState = newState;
            m_onSetGameState?.Invoke(m_currentGameState);
            // Check if managers needed to be added
            if (SetupStartSequence()) StartCoroutine(StartupManagers());
        }
        #endregion

        #region Gamemode-Related Functionality
        public void SetGamemode(Gamemode _mode)
        {
            // Set the gamemode
            m_currentGamemode = _mode;
            // Set the config reference
            m_currentGamemodeConfig = m_config.AvailableGamemodes[((int)_mode)];
        }
        #endregion

        #region Scenario-Related Functionality
        public void LoadScenario(ScenarioAsset _scenario)
        {
            if (!m_scenarioManager) return;
            // TEMP: Calibrate the player's height just before the scene is loaded
            //if (m_playerManager.Player != null && !m_playerManager.Player.InitialHeightCalibrated) m_playerManager.Player?.CalibrateHeight();
            m_scenarioManager.SetScenario(_scenario);
            LoadScene(m_scenarioManager.CurrentScenario.Scene);
        }
        #endregion

        #region Application-Related Methods
        public void ExitToDesktop()
        {
            UnityEngine.Debug.Log($"Qutting Game || Time: {Time.time}");
            Application.Quit();
        }

        public void LoadScene(string _sceneName)
        {
            StartCoroutine(LoadSceneRoutine(_sceneName));
            //m_onSceneLoadStarted?.Invoke(m_currentGameState);
            //SceneManager.LoadScene(_sceneName);
        }

        IEnumerator LoadSceneRoutine(string _sceneName)
        {
            m_onSceneLoadStarted?.Invoke(m_currentGameState);
            if (ScreenFader.Instance != null)
            {
                while (ScreenFader.Instance.Active) yield return null;
            }
            // Start the scene load
            //UnityEngine.Debug.Log($"Finished fade || Time: {Time.time}");
            SceneManager.LoadScene(_sceneName);
        }
        public void LoadMainMenu()
        {
            LoadScene(m_config.MenuScene);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if this is the main menu
            if (scene.name.Contains("MainMenu"))
            {
                // Do main menu logic
                if (ScoreManager.Instance && ScoreManager.Instance.ScenarioResults != null)
                {
                    // Find the main menu
                    MainMenu menu = FindObjectOfType<MainMenu>();
                    if (menu != null) menu.SetState(3);
                    //if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
                    SetGameStateOverride(GameState.MainMenu);
                    m_playerManager.Player.XRRig.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
                }
                return;
            }
            // Find any manager injectors
            List<IManagerInjector> injectors = FindObjectsOfType<MonoBehaviour>().OfType<IManagerInjector>().ToList();
            // Check if any injectors were found
            if (injectors != null && injectors.Count > 0)
            {
                // Inject any scene-specific data into managers
                foreach (IManagerInjector injector in injectors) injector.Init();
            }
            // Set game state to in-game
            SetGameStateOverride(GameState.InGame);
        }

        public bool HasXRDevicesLoaded()
        {
            List<UnityEngine.XR.InputDevice> xrInputDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(xrInputDevices);

            // Check if any XR input devices were found
            if (xrInputDevices.Count > 0) return true;
            return false;
        }
        #endregion

        public static GamemodeConfigAsset GetGamemodeConfig(Gamemode _mode)
        {
            if (Instance == null || Instance.Config == null) return null;

            return Instance.Config.GetGamemode(_mode);
        }
    }

    public enum GameState {StartScreen, MainMenu, Loading, InGame}
}

