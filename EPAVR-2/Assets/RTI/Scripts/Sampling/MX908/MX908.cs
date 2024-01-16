using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;

namespace L58.EPAVR
{
    public class MX908 : SampleTool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] MX908GUI m_mxGUI;
        [SerializeField] Transform m_toolAttachPoint;
        [SerializeField] HVRSocket m_toolSocket;
        [SerializeField] Collider m_toolAttachCollider;
        [SerializeField] List<MX908Tool> m_toolPrefabs;
        [SerializeField] MX908Tool m_defaultTool;
        [SerializeField] List<Transform> m_swabSpawnPoints;
        [Header("Debug Configuration")]
        [SerializeField] bool m_initializeOnAwake = false;
        #endregion
        #region Protected Variables
        protected List<MX908Tool> m_tools;
        protected MX908Tool m_currentTool = default;
        protected MXMode m_currentMode = MXMode.Neutral;
        protected Transform m_beltTransform;
        // Action-related variables
        protected Action<MX908Tool> m_onToolApplied;
        protected Action<MX908Tool> m_onToolRemoved;

        protected bool m_forcedInView = false;
        #endregion
        #region Public Properties
        public override ToolType Type => ToolType.MX908;
        public GameObject GameObject => gameObject;

        public bool Active { get; set; } = false;

        public MXMode Mode { get => m_currentMode; }
        public MX908GUI GUI { get => m_mxGUI; }
        public MX908Tool Tool { get => m_currentTool; }

        public Collider AttachCollider { get => m_toolAttachCollider; }
        public List<Transform> SampleSwabSpawnPoints { get => m_swabSpawnPoints; }
        #endregion

        #region Initialization
        public void Awake()
        {
            if (m_initializeOnAwake) Init();
        }
        public override void Init()
        {
            // Hook up events
            if (m_toolSocket != null)
            {
                m_toolSocket.Grabbed.AddListener(ProcessToolAttach);
                m_toolSocket.Released.AddListener(ProcessToolRemove);
            }
            
            m_onToolApplied += i => m_mxGUI.UpdateToolGUI(i);
            m_onToolRemoved += i => m_mxGUI.UpdateToolGUI(null);
            // Initialize GUI
            m_mxGUI.Init();
            /*
            // Parent the MX908 to the toolbelt
            if (TryGetComponent<XRGrabInteractable>(out XRGrabInteractable mx908Interactable))
            {
                XRToolbeltItem beltItem = L58XRAvatar.Instance.Toolbelt.CreateToolObject(mx908Interactable);
            }*/
            // Initialize the attachment list if necessary
            //if (m_tools == null) m_tools = new List<MX908Tool>();

            // Check if the attachments list has been populated
            if (m_tools != null && m_tools.Count > 0)
            {
                // Initialize each attachment
                foreach (MX908Tool tool in m_tools) tool.Init(this);
            }

            /*
            // Initialize attachments
            foreach (MX908Tool toolPrefab in m_toolPrefabs) 
            {
                // Create the tool
                MX908Tool tool = Instantiate(toolPrefab);
                tool.Init(this);
                // Get the interactable component of the tool and add it to the user's toolbelt
                if (tool.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable)) 
                {
                    XRToolbeltItem beltItem = L58XRAvatar.Instance.Toolbelt.CreateToolObjectOnBelt(interactable);
                    if (beltItem != null) tool.OnDeselected += () =>
                    {
                        // Make sure that this does not happen when the player lets go of the tool to attach it
                        if (m_currentTool != tool) return;
                        // Return the tool to the player's belt
                        beltItem.AttachToBelt();
                    };
                }
                    
                // Cache this tool in the MX908 tool list
                m_tools.Add(tool);
            }
            */
            // Set Active
            Active = true;

            //if (DebugManager.Instance && DebugManager.Instance.VRSimulating) DebugManager.Instance.OnSpacePressed += ForceToggleInView;
            //AttachTool(m_defaultTool);
        }

        public override void OnSpawn()
        {
            // Do the thing
        }

        public override void OnDespawn()
        {
            // Do the thing
        }

        public override List<GameObject> SpawnAdditionalObjects()
        {
            // Initialize the attachment list if necessary
            if (m_tools == null) m_tools = new List<MX908Tool>();
            List<GameObject> spawnedObjects = new List<GameObject>();

            // Create and cache a reference to each attachment
            for(int i = 0; i < m_toolPrefabs.Count; i++)
            {
                // Get the tool prefab reference
                MX908Tool toolPrefab = m_toolPrefabs[i];
                // Get the target attach point
                if (VRUserManager.Instance.Player.Toolbox == null && VRUserManager.Instance.Player.Inventory.Toolbox != null)
                {
                    UnityEngine.Debug.Log($"{gameObject.name} ERROR: User Toolbox not found, but inventory has it || Time: {Time.time}");
                }
                XRSocket attachPoint = VRUserManager.Instance.Player.Toolbox.OtherSockets[i];
                // Create the tool
                MX908Tool tool = Instantiate(toolPrefab, attachPoint.transform);
                // Get the interactable component of the tool and add it to the user's toolbox
                if (tool.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    XRToolbeltItem beltItem = tool.gameObject.AddComponent<XRToolbeltItem>();
                    beltItem.Init(attachPoint, true);
                    //beltItem.Init(interactable, attachPoint);
                    if (beltItem != null) tool.OnDeselected += () =>
                    {
                        // Make sure that this does not happen when the player lets go of the tool to attach it
                        if (m_currentTool != tool) return;
                        beltItem.ForceAttachToSocket();
                        // Return the tool to the player's belt
                        ///beltItem.AttachToBelt();
                    };
                }
                // Cache this tool in the MX908 tool list
                m_tools.Add(tool);
                // Add it to the spawned objects list
                spawnedObjects.Add(tool.gameObject);
            }
            return spawnedObjects;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            //Init();
        }

        // Update is called once per frame
        void Update()
        {
            // Make sure the system is active
            if (!Active) return;
            // Check if there is a current tool
            if (m_currentTool != null) m_currentTool.OnUpdate();
        }
        

        #region Tool Functionality
        public void AttachTool(MX908Tool _tool)
        {
            // Make sure there is no current tool
            if (m_currentTool != null) return;
            // Set reference and call relevant functionality
            m_currentTool = _tool;
            m_currentTool.OnApplied();
            m_currentMode = _tool.Mode;
            // Hook up actions
            // Snap to the attach point and reset position/rotation
            m_currentTool.transform.parent = m_toolAttachCollider.transform;
            m_currentTool.transform.localPosition = Vector3.zero;
            m_currentTool.transform.localEulerAngles = Vector3.zero;
            // Call the action
            m_onToolApplied?.Invoke(m_currentTool);
        }

        public void RemoveTool()
        {
            UnityEngine.Debug.Log($"Removing tool: {m_currentTool.gameObject.name} || Time: {Time.time}");
            if (m_currentTool == null) return;
            // Unparent the tool
            m_currentTool.transform.parent = null;
            // Call the action and reset reference
            m_currentTool.OnRemoved();
            m_onToolRemoved?.Invoke(m_currentTool);
            m_currentTool = null;
            // Reset mode
            m_currentMode = MXMode.Neutral;
        }
        #endregion

        #region Helper Functionality
        public override void ForceToggleInView()
        {
            if (!m_forcedInView)
            {
                // Get headset position
                //Vector3 targetPosition = new Vector3(transform.position.x, Camera.main.transform.position.y - 0.1f, transform.position.z);
                Vector3 targetPosition = Camera.main.transform.position + (Camera.main.transform.forward * 0.35f);
                targetPosition.y -= 0.19f;

                transform.localPosition = transform.InverseTransformPoint(targetPosition);
                transform.parent = Camera.main.transform;
                transform.localEulerAngles = new Vector3(-40.0f, 0.0f, 0.0f);
                m_beltTransform = transform.parent;
         
                m_forcedInView = true;
            }
            else
            {
                GetComponent<XRToolbeltItem>().AttachToBelt();
                /*
                transform.parent = m_beltTransform;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                */
     
                m_forcedInView = false;
            }
        }

        public void ForceAttachTool(int index)
        {
            if (index < 0 || index > m_tools.Count) return;
            // Get the tool at the specified index
            MX908Tool tool = m_tools[index];
    
            if (m_currentTool == tool) return;
            if (m_currentTool != null) 
            {
                MX908Tool prevTool = m_currentTool;
                RemoveTool();
                prevTool.GetComponent<XRToolbeltItem>().ForceAttachToSocket();
            } 
            tool.ForceAttachToDevice();
        }

        public void ForceRemoveTool()
        {
            if (m_currentTool == null) return;
            MX908Tool prevTool = m_currentTool;
            RemoveTool();
            prevTool.GetComponent<XRToolbeltItem>().ForceAttachToSocket();
        }

        public void ProcessToolAttach(HVRGrabberBase grabber, HVRGrabbable grabbable)
        {
            // Try to get a tool component from the grabbable
            if (grabbable.TryGetComponent<MX908Tool>(out MX908Tool tool))
            {
                // Attach the tool to the device
                AttachTool(tool);
            }
        }

        public void ProcessToolRemove(HVRGrabberBase grabber, HVRGrabbable grabbable)
        {
            // Try to get a tool component from the grabbable
            if (grabbable.TryGetComponent<MX908Tool>(out MX908Tool tool))
            {
                // Remove the current tool
                RemoveTool();
            }
        }
        #endregion

        public override SampleReportOld GenerateReport(ScenarioStep _step)
        {
            throw new NotImplementedException();
        }

        public override int GetAdditionalActiveItemCount()
        {
            int activeItemCount = base.GetAdditionalActiveItemCount();
            if (m_currentMode == MXMode.TraceSampling && m_currentTool is MXTraceSampler traceSampler)
            {
                // Get active swabs
                if (traceSampler.ActiveSwabs != null && traceSampler.ActiveSwabs.Count > 0)
                {
                    for(int i = 0; i < traceSampler.ActiveSwabs.Count; i++)
                    {
                        TraceSampleSwab swab = traceSampler.ActiveSwabs[i];
                        if (swab.Active) activeItemCount++;
                    }
                }
            }
            return activeItemCount;
        }
    }

    public enum MXMode { Neutral, TraceSampling, AirMonitoring}
}

