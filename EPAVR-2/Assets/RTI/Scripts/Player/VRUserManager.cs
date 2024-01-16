using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using HurricaneVR;
using HurricaneVR.Framework.Core;

namespace L58.EPAVR
{
    public class VRUserManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Core Prefab References")]
        [SerializeField] PlayerController m_xrPlayerPrefab;
        [SerializeField] GameObject m_nonXRPlayerPrefab;
        [Header("Important References")]
        [SerializeField] PlayerController m_presetPlayer;
        [SerializeField] L58XRAvatar m_avatar;
        [SerializeField] RectTransform m_menuContainer;
        [SerializeField] OffsiteLab m_offsiteLab;
        [SerializeField] L58TeleportationArea m_defaultTeleportArea;
        [Header("Input Configuration")]
        [SerializeField] InputActionReference m_moveTypeSelectAction;
        [SerializeField] InputActionReference m_activateTeleportAction;
        [SerializeField] InputActionReference m_teleportValueAction;
        [SerializeField] InputActionReference m_toggleToolMenuAction;
        [Header("Player Configuration")]
        [SerializeField] MovementMode m_defaultMovementMode = MovementMode.Teleport;
        [SerializeField] Vector2 m_cameraYOffsetRange;
        [SerializeField] float m_defaultCameraYOffset = 1.728814f;
        [SerializeField] float m_defaultEyeHeight = 1.7f;
        [SerializeField] float m_defaultHeightCalibrateOffset = 0.1596f;
        [SerializeField] float m_defaultHeightReference = 1.778f;
        [SerializeField] bool m_saveCalibratedHeight = true;
        [Header("UI References")]
        [SerializeField] XRTooltip m_tooltipPrefab;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;

        private HVRManager m_hvrManager;
        private PlayerController m_player;
        private bool m_isNonVRMode = false;
        private PlayerState m_currentState;
        private PlayerState m_previousState;

        public SampleTool m_currentTool;
        public SampleArea m_currentSampleArea;
        private ToolInstance m_currentToolInstance;
        private Dictionary<ToolType, ToolInstance> m_toolInstances;
        private List<Sample> m_deliveredSamples;

        private bool m_inputEventsInitialized = false;

        private Action<PlayerState> m_onStateChange;
        private Action<SampleTool> m_onSetTool;
        private Action<SampleArea> m_onSetSampleArea;
        private Action<Sample> m_onDeliverSample;
        private Action<SampleArea, ChemicalAgent> m_onIdentifyChemical;

        private PointerEventData m_pointerEventData;

        public const string HeightKey = "SaveHVRHeight";
        #endregion
        #region Public Properties
        public static VRUserManager Instance { get; set; }

        public ManagerStatus Status => m_status;
        public PlayerController Player { get => m_player; }

        public bool IsNonVRMode { get => m_isNonVRMode; set => m_isNonVRMode = value; }
        public L58XRAvatar Avatar { get => m_avatar; }

        public PlayerState CurrentState { get => m_currentState; }
        public PlayerState PreviousState { get => m_previousState; }

        public SampleTool CurrentTool { get => m_player.CurrentTool; }
        public ToolType CurrentToolType { get => m_player.CurrentToolType; }
        public SampleArea CurrentSampleArea { get => m_currentSampleArea; }

        public List<Sample> DeliveredSamples { get => m_deliveredSamples; }

        public RectTransform MenuContainer { get => m_menuContainer; }

        public float MinCameraYOffset { get => m_cameraYOffsetRange.x; }
        public float MaxCameraYOffset { get => m_cameraYOffsetRange.y; }
        public float DefaultCameraYOffset { get => m_defaultCameraYOffset; }
        public float DefaultHeightReference { get => m_defaultHeightReference; }
        public float DefaultHeightCalibrateOffset { get => m_defaultHeightCalibrateOffset; }
        public bool SaveCalibratedHeight { get => m_saveCalibratedHeight; }

        public Action<PlayerState> OnStateChange { get => m_onStateChange; set => m_onStateChange = value; }
        public Action<SampleTool> OnSetTool { get => m_onSetTool; set => m_onSetTool = value; }
        public Action<SampleArea> OnSetSampleArea { get => m_onSetSampleArea; set => m_onSetSampleArea = value; }
        public Action<Sample> OnDeliverSample { get => m_onDeliverSample; set => m_onDeliverSample = value; }
        public PointerEventData PointerEventData { get => m_pointerEventData; }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Set singleton
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            if (!m_player)
            {
                // Check for a pre-existing player
                //PlayerController player = (m_presetPlayer != null) ? m_presetPlayer : GameObject.FindObjectOfType<PlayerController>();
                PlayerController player = Instantiate(m_xrPlayerPrefab, Vector3.zero, Quaternion.identity);
                if (player != null)
                {
                    // Cache the player and initialize it
                    m_player = player;
                    //m_player.gameObject.SetActive(true);
                    CacheAndInitPlayerRefs();
                    DontDestroyOnLoad(m_player.gameObject);
                }

                if (m_defaultTeleportArea)
                {
                    m_defaultTeleportArea.interactionManager = m_player.GetComponentInChildren<XRInteractionManager>();
                }
            }
           
  
        }

        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            // Cache HVRManager reference if available
            if (!m_hvrManager && HVRManager.Instance) m_hvrManager = HVRManager.Instance;
            // Spawn the player if necessary
            if (!m_player)
            {
                if (m_isNonVRMode) 
                    m_player = Instantiate(m_nonXRPlayerPrefab).transform.GetChild(0).GetComponent<PlayerController>();
                else
                    m_player = Instantiate(m_xrPlayerPrefab).GetComponent<PlayerController>();
                // Cache and initialize the player
                CacheAndInitPlayerRefs();
            }
            // Get player IK
            m_player.Avatar.IK.SetMeshLayer(1);
            // Spawn equipment
            SpawnEquipment();
            

            // Check if there is a scenario manager
            if (ScenarioManager.Instance && ScenarioManager.Instance.CurrentScenario != null) 
            {
                // Hook up event
                //ScenarioManager.Instance.OnSiteCleared += (i, j) => SpawnPopup("Site Cleared");
                ScenarioManager.Instance.OnObjectiveCompleted.AddListener(SpawnObjectivePopup);
                // Spawn the default tool
                SpawnTool(ScenarioManager.Instance.CurrentScenario.DefaultTool);
            }
            // Initialize the tool selection menu
            m_player.InitToolSelectionMenu();

            // Initialize off-site lab
            m_offsiteLab?.Init();
            // Initialize samples
            m_deliveredSamples = new List<Sample>();
            // Set player state
            SetState(PlayerState.InGame);
            // Finish initialization
            UnityEngine.Debug.Log($"VRUserManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }

        void CacheAndInitPlayerRefs()
        {
            if (!m_player) return;
            // Cache references
            m_avatar = m_player.Avatar;
            m_menuContainer = m_player.MenuContainer;
            // Set any initial values necessary
            //m_player.DefaultMovementMode = m_defaultMovementMode;
            // Initialize the player
            m_player.Init();
            //float defaultHeight = (m_saveCalibratedHeight) ? FetchSavedHeight() : m_defaultCameraYOffset;
            //float defaultHeight = m_defaultCameraYOffset;
            float defaultHeight = m_defaultEyeHeight;
            m_player.InitHeight(m_cameraYOffsetRange, defaultHeight);
            m_player.OnSetTool += i => m_currentTool = i;
            // Hook up events as necessary
            if (!m_inputEventsInitialized)
            {
    
                m_moveTypeSelectAction.action.Enable();
                m_moveTypeSelectAction.action.performed += i => m_player.ToggleMovementMode();

                m_player.TeleportValueAction = m_teleportValueAction;
                m_player.TeleportValueAction.action.Enable();

                m_player.TeleportValueAction.action.started += i => m_player.EnableTeleportationInput();
                //m_player.TeleportValueAction.action.canceled += i => m_player.DisableTeleportationInput();
                m_player.TeleportValueAction.action.canceled += i => m_player.AttemptTeleportation();

                /*
                m_activateTeleportAction.action.Enable();
                m_activateTeleportAction.action.started += i => m_player.EnableTeleport();
                m_activateTeleportAction.action.canceled += i => m_player.AttemptTeleportation();
                */

                m_toggleToolMenuAction.action.Enable();
                m_toggleToolMenuAction.action.performed += i => m_player.ToggleToolSelectionMenu();
            }
            
        }
        #endregion

        #region Equipment-Related Functionality
        void SpawnEquipment()
        {
            // Spawn default equipment for player
            m_player.SpawnEquipment();
            /*
            // Create tool objects
            m_toolInstances = new Dictionary<ToolType, ToolInstance>();
            XRMultiSocket deviceSocket = m_player.Toolbelt.MainDeviceAttachPoint.GetComponent<XRMultiSocket>();
            List<XRSocketItem> deviceSocketItems = new List<XRSocketItem>();
            if (deviceSocket != null)
            {
                for (int i = 0; i < CoreGameManager.Instance.Config.AvailableTools.Count; i++)
                {
                    // Get the prefab reference
                    SampleTool toolRef = CoreGameManager.Instance.Config.AvailableTools[i];
                    // Get the tool type
                    ToolType type = toolRef.Type;
                    // Instantiate the tool
                    GameObject toolObject = Instantiate(toolRef.gameObject, deviceSocket.transform);
                    SampleTool tool = toolObject.GetComponent<SampleTool>();
                    if (tool.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable) && tool.TryGetComponent<XRSocketItem>(out XRSocketItem socketItem))
                    {
                        socketItem.Init(interactable, deviceSocket.transform);
                        socketItem.AttachToBelt();
                        deviceSocketItems.Add(socketItem);
                    }
                    // Call any tool-specific instantiation logic for additional objects
                    List<GameObject> spawnedObjects = tool.SpawnAdditionalObjects();
                    // Call any initialization logic for the tool
                    tool.Init();
                    // Create the tool instance reference
                    ToolInstance toolInstance = new ToolInstance(tool, spawnedObjects);
                    // Add this to the registry of tool instances
                    m_toolInstances.Add(type, toolInstance);
                    // Disable the tool instance by default
                    toolInstance.SetActive(false);
                }
                // Initialize the device socket
                if (deviceSocketItems.Count > 0) deviceSocket.Init(deviceSocketItems);
            }
            */
        }

        void RemoveEquipment()
        {
            /*
            // Destroy all tool instances
            if (m_toolInstances != null && m_toolInstances.Count > 0)
            {
                List<GameObject> toolObjectsToDestroy = new List<GameObject>();
                foreach(KeyValuePair<ToolType, ToolInstance> item in m_toolInstances)
                {
                    ToolInstance tool = item.Value;
                    // First, add any spawned objects to the list
                    if (tool.Objects != null && tool.Objects.Count > 0)
                        foreach (GameObject obj in tool.Objects) toolObjectsToDestroy.Add(obj);
                    // Add the tool object itself
                    toolObjectsToDestroy.Add(tool.Tool.gameObject);
                }
                // Loop through each reference and destroy it
                foreach (GameObject obj in toolObjectsToDestroy) Destroy(obj);
                // Clear the dictionary
                m_toolInstances.Clear();
                m_toolInstances = null;
            }
            */
            // Remove all equipment from player
            m_player.RemoveEquipment();
        }
        #endregion
        private void Start()
        {
            /*
            if (!SimulationManager.Instance || SimulationManager.Instance.CurrentScenario == null) return;
            SpawnTool(SimulationManager.Instance.CurrentScenario.DefaultTool);
            */
            //SpawnTool(ToolType.MX908);
        }

        private void Update()
        {
            m_pointerEventData = new PointerEventData(EventSystem.current);
            //Set the Pointer Event Position to that of the mouse position
            m_pointerEventData.position = Mouse.current.position.ReadValue();
        }


        #region Tool-Related Functionality
        public void SpawnTool(ToolType _toolType)
        {
            // Tell the player to spawn the specified tool
            m_player.SpawnTool(_toolType);
            /*
            if (m_toolInstances == null || m_toolInstances.Count < 1 || (m_currentTool != null && m_currentTool.Type == _toolType)) return;
            // Check if there is a previous tool instance active
            if (m_currentTool != null)
            {
                // Call any tool-specific despawn functionality
                m_currentTool.OnDespawn();
                // Disable the previous tool
                m_currentToolInstance.SetActive(false);
            }
            // Get the tool instance
            ToolInstance toolInstance = m_toolInstances[_toolType];
            m_currentToolInstance = toolInstance;
            m_currentTool = toolInstance.Tool;
            // Enable the tool
            m_currentToolInstance.SetActive(true);
            // Call any tool-specific spawn functionality
            m_currentTool.OnSpawn();
            // Invoke any necessary events
            m_onSetTool?.Invoke(m_currentTool);
            */
        }
        /*
        public void SpawnTool(ToolType _toolType)
        {
            if (!CoreGameManager.Instance || !m_avatar || (m_currentTool != null && m_currentTool.Type == _toolType)) return;

            // Spawn the tool
            SampleTool toolRef = CoreGameManager.Instance.Config.GetTool(_toolType);
            if (toolRef == null) return;

            GameObject toolObject = Instantiate(toolRef).gameObject;
            SampleTool tool = toolObject.GetComponent<SampleTool>();
            if (m_avatar.Toolbelt != null && toolObject.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable xrInteractable))
            {
                // Clear the belt if necessary
                m_avatar.Toolbelt.ClearItems();

                XRToolbeltItem toolbeltItem = m_avatar.Toolbelt.CreateToolObjectOnBelt(xrInteractable, true);
            }
            m_currentTool = tool;
            tool.Init();
            m_onSetTool?.Invoke(m_currentTool);
            //SimulationManager.Instance.SetSampleTool(tool);
        }
        */
        #endregion

        #region Sample Area-Related Functionality
        public void SetSampleArea(SampleArea _sampleArea)
        {
            // Cache reference
            m_currentSampleArea = _sampleArea;
            // Invoke any specific functionality
            m_onSetSampleArea?.Invoke(m_currentSampleArea);
        }
        #endregion

        #region Sample-Related Functionality
        public void DeliverSample(Sample _sample)
        {
            // Add this to the delivered samples list
            m_deliveredSamples.Add(_sample);
            m_onDeliverSample?.Invoke(_sample);
            // Invoke identified chemical
            _sample.UsedTool?.IdentifyChemical(_sample);
        }
        #endregion

        #region State-Related Functionality
        public void SetState(PlayerState _state)
        {
            if (m_previousState != m_currentState) m_previousState = m_currentState;
            m_currentState = _state;
            m_onStateChange?.Invoke(m_currentState);
        }
        #endregion

        #region Tooltip Functionality
        public void SpawnTooltip(XRHand _targetHand, string _message)
        {
            // First, check if there is already a tooltip on this hand
            if (_targetHand.CurrentTooltip != null) Destroy(_targetHand.CurrentTooltip.gameObject);
            // Create a new tooltip
            _targetHand.SetTooltip(Instantiate(m_tooltipPrefab.gameObject, _targetHand.TooltipSpawnPoint).GetComponent<XRTooltip>(), _message);
        }

        public void SpawnPopup(string _message)
        {
            // First, check if there is already a tooltip on this hand
            if (m_player.CurrentPopup != null) Destroy(m_player.CurrentPopup.gameObject);
            m_player.SetPopup(Instantiate(m_tooltipPrefab.gameObject, m_player.PopupContainer).GetComponent<XRTooltip>(), _message);
        }

        public void SpawnObjectivePopup(ScenarioObjective _objective)
        {
            SpawnPopup(_objective.ClearMessage);
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin reset
            m_status = ManagerStatus.Resetting;
            // Clear the item menu if it is open
            if (m_player.MenuCanvas != null && m_player.MenuCanvas.gameObject.activeInHierarchy) m_player.MenuCanvas.gameObject.SetActive(false);
            // Reset lab
            m_offsiteLab.ResetToStart();
            // Remove all equipment
            RemoveEquipment();
            // Unhook events
            m_player.OnSetTool -= i => m_currentTool = i;
            // Clear references
            /*
            m_avatar = null;
            m_menuContainer = null;
            if (m_player != null)
            {
                Destroy(m_player.gameObject);
                Destroy(m_player);
                m_player = null;
            }
            */
            // Teleport the player back to spawn
            m_player.CharacterController.enabled = false;
            m_player.XRRig.transform.position = Vector3.zero;
            m_player.CharacterController.enabled = true;
            // Finish reset
            UnityEngine.Debug.Log($"VRUserManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }
        #endregion

        #region Helper Methods
        public float FetchSavedHeight()
        {
            if (PlayerPrefs.HasKey(HeightKey))
            {
                var height = PlayerPrefs.GetFloat(HeightKey);
                float displayAdjustedHeight = height + m_defaultHeightCalibrateOffset;
                string text = $"{(MathHelper.ConvertMetersToFeet(displayAdjustedHeight)).ToString("0.000")} ft ({displayAdjustedHeight.ToString("0.000")} m)";
                UnityEngine.Debug.Log($"{gameObject.name} found saved height: {text} || Time: {Time.time}");
                return height;
            }
            else
            {
                return m_defaultCameraYOffset;
            }
        }
        #endregion
    }

    public enum PlayerState { InGame, Menu, Focused}

    public class ToolInstance
    {
        #region Private Variables
        private SampleTool m_tool;
        private List<GameObject> m_toolObjects;
        private XRToolbeltItem m_toolItem;

        private bool m_active;
        #endregion
        #region Public Properties
        public SampleTool Tool { get => m_tool; }
        public ToolType Type { get => m_tool.Type; }
        public List<GameObject> Objects { get => m_toolObjects; }

        public bool Active { get => m_active; }
        #endregion

        #region Constructor
        public ToolInstance(SampleTool _tool, List<GameObject> _objects)
        {
            // Cache references
            m_tool = _tool;
            m_toolItem = m_tool.GetComponent<XRToolbeltItem>();
            // Create spawned object reference
            m_toolObjects = new List<GameObject>();
            // Add each spawned object's reference
            if (_objects != null && _objects.Count > 0)
            {
                foreach (GameObject obj in _objects)
                    m_toolObjects.Add(obj);
            }
        }
        #endregion

        #region State-Related Methods
        public void SetActive(bool value)
        {
            // Set active value
            m_active = value;
            // Check if this is false
            if (!m_active && m_toolItem) m_toolItem.AttachToBelt(); 
            // Enable/disable the sample tool
            m_tool.gameObject.SetActive(value);
            // Enable/disable any additional objects
            if (m_toolObjects != null && m_toolObjects.Count > 0)
                foreach (GameObject toolObject in m_toolObjects) toolObject.gameObject.SetActive(value);
        }
        #endregion
    }
}

