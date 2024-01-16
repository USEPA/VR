using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class Inventory : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Prefab References")]
        [SerializeField] protected UserTablet m_userTabletPrefab;
        [SerializeField] protected UserToolbox m_userToolboxPrefab;
        [SerializeField] protected DisposalBag m_disposalBagPrefab;
        #endregion
        #region Protected Variables
        protected PlayerController m_player;
        protected L58XRAvatar m_avatar;


        protected List<GameObject> m_equipment;
        protected XRToolbelt m_toolbelt;
        protected UserToolbox m_userToolbox;
        protected UserTablet m_userTablet;
        protected DisposalBag m_disposalBag;

        protected GamemodeConfigAsset m_currentGamemodeConfig;
        protected Dictionary<ToolType, ToolInstance> m_toolInstances;
        #endregion
        #region Public Properties
        public List<GameObject> Equipment { get => m_equipment; }

        public UserTablet Tablet { get => m_userTablet; }
        public UserToolbox Toolbox { get => m_userToolbox; }
        public DisposalBag DisposalBag { get => m_disposalBag; }

        public Dictionary<ToolType, ToolInstance> ToolInstances { get => m_toolInstances; }
        #endregion

        #region Initialization
        public void Init(PlayerController _player)
        {
            // Cache reference
            m_player = _player;
            m_avatar = m_player.Avatar;
            m_toolbelt = m_player.Avatar.Toolbelt;
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion

        #region Equipment-Related Functionality
        public void SpawnEquipment(GamemodeConfigAsset _gamemodeConfig)
        {
            // Set current gamemode config
            m_currentGamemodeConfig = CoreGameManager.Instance.CurrentGamemodeConfig;
            // Initialize equipment list
            m_equipment = new List<GameObject>();
            // Spawn the toolbelt equipment
            SpawnTablet();
            switch (_gamemodeConfig.Mode)
            {
                case Gamemode.ChemicalHunt:
                    SpawnDisposalBag();
                    break;
                default: break;
            }
            // Spawn the toolbox
            SpawnToolbox();

            // Spawn the main sample tools
            SpawnSampleTools();
        }

        public void RemoveEquipment()
        {
            // Destroy all tool instances
            if (m_toolInstances != null && m_toolInstances.Count > 0)
            {
                List<GameObject> toolObjectsToDestroy = new List<GameObject>();
                foreach (KeyValuePair<ToolType, ToolInstance> item in m_toolInstances)
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
            // Destroy any objects that spawned from the toolbox
            if (m_userToolbox != null && m_userToolbox.Items != null && m_userToolbox.Items.Count > 0)
            {
                foreach (XRToolbeltItem item in m_userToolbox.Items) 
                {
                    if (item != null && item.gameObject != null)
                    {
                        //UnityEngine.Debug.Log($"{gameObject.name} - destroying item: {item.gameObject.name} || Time: {Time.time}");
                        Destroy(item.gameObject);
                    }
                }
                
            }
            // Remove any registered items
            if (m_equipment != null && m_equipment.Count > 0)
            {
                foreach (GameObject obj in m_equipment) Destroy(obj);
            }
            // Clear all references
            m_userTablet = null;
            m_userToolbox = null;
            m_disposalBag = null;
        }

        protected void SpawnSampleTools()
        {
            // Create the tool instance dictionary
            m_toolInstances = new Dictionary<ToolType, ToolInstance>();
            // Get the device socket
            if (m_toolbelt.MainDeviceAttachPoint != null && m_toolbelt.MainDeviceAttachPoint is XRMultiSocket deviceSocket)
            {
                // Initialize the socket
                deviceSocket.Init(false);
                // Create the device socket item list
                List<XRToolbeltItem> deviceSocketItems = new List<XRToolbeltItem>();
                
                for (int i = 0; i < CoreGameManager.Instance.CurrentGamemodeConfig.AvailableTools.Count; i++)
                {
                    // Get the prefab reference
                    SampleTool toolRef = CoreGameManager.Instance.CurrentGamemodeConfig.AvailableTools[i];
                    // Get the tool type
                    ToolType type = toolRef.Type;
                    // Instantiate the tool
                    GameObject toolObject = Instantiate(toolRef.gameObject, deviceSocket.transform);
                    SampleTool tool = toolObject.GetComponent<SampleTool>();
                    if (tool.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable) && tool.TryGetComponent<XRToolbeltItem>(out XRToolbeltItem socketItem))
                    {
                        //socketItem.Init(interactable, deviceSocket.transform);
                        socketItem.Init(deviceSocket, true);
                        //socketItem.AttachToBelt();
                        deviceSocketItems.Add(socketItem);
                    }
                    /*
                    if (tool.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                    {
                        XRToolbeltItem toolbeltItem = tool.gameObject.AddComponent<XRToolbeltItem>();
                        toolbeltItem.Init(interactable, m_player.Toolbelt.MainDeviceAttachPoint);
                        toolbeltItem.AttachToBelt();
                    }
                    */
                    // Call any tool-specific instantiation logic for additional objects
                    List<GameObject> spawnedObjects = tool.SpawnAdditionalObjects();
                    // Call any initialization logic for the tool
                    tool.Init();
                    // Create the tool instance reference
                    ToolInstance toolInstance = new ToolInstance(tool, spawnedObjects);
                    // Add this to the registry of tool instances
                    m_toolInstances.Add(type, toolInstance);
                    UnityEngine.Debug.Log($"{gameObject.name} added tool instance: {type} || Time: {Time.time}");
                    // Disable the tool instance by default
                    toolInstance.SetActive(false);
                }
                // Initialize the device socket
                if (deviceSocketItems.Count > 0) deviceSocket.Init(deviceSocketItems);

                UnityEngine.Debug.Log($"{m_player.gameObject.name} inventory finished spawning device tools: {deviceSocketItems.Count} || Time: {Time.time}");
            }
        }

        #endregion

        #region Basic Spawn-Related Functionality
        public XRToolbeltItem SpawnItem(GameObject _prefab, XRSocket _socket, bool _autoReturnOnRelease = false, bool _linkSocket = true)
        {
            // Instantiate the object
            GameObject obj = Instantiate(_prefab, _socket.transform);
            // Get or add the socket item component then initialize it
            XRToolbeltItem item = obj.GetComponent<XRToolbeltItem>();
            if (!item) 
            {
                item = obj.AddComponent<XRToolbeltItem>();
                //XRGrabInteractable interactable = obj.GetComponent<XRGrabInteractable>();
                //interactable.selectEntered.AddListener(i => item.AttemptRemove());
            }
            // Initialize the socket item
            item.Init(_socket, _linkSocket, _autoReturnOnRelease);
            // Initialize the socket
            //_socket.Init(item, false);
            //item.Init(obj.GetComponent<XRGrabInteractable>(), _socket.transform, _autoReturnOnRelease);
      
            // Add this to the active equipment
            m_equipment.Add(item.gameObject);
            return item;
        }

        protected void SpawnTablet()
        {
            if (m_currentGamemodeConfig != null && m_currentGamemodeConfig.UserTabletPrefab) m_userTabletPrefab = m_currentGamemodeConfig.UserTabletPrefab;
            if (!m_userTablet && m_userTabletPrefab)
            {
                // Spawn the item
                XRToolbeltItem tabletItem = SpawnItem(m_userTabletPrefab.gameObject, m_toolbelt.TabletAttachPoint, true);
                m_userTablet = tabletItem.GetComponent<UserTablet>();
                // Initialize the tablet
                m_userTablet.Init();
            }
        }

        protected void SpawnToolbox()
        {
            if (m_currentGamemodeConfig != null && m_currentGamemodeConfig.UserToolboxPrefab) m_userToolboxPrefab = m_currentGamemodeConfig.UserToolboxPrefab;
            if (!m_userToolbox && m_userToolboxPrefab)
            {
                // Spawn the item
                XRToolbeltItem toolboxItem = SpawnItem(m_currentGamemodeConfig.UserToolboxPrefab.gameObject, m_avatar.BackAttachPoint);
                m_userToolbox = toolboxItem.GetComponent<UserToolbox>();
                //UnityEngine.Debug.Log($"{m_player.gameObject.name} spawned toolbox: {m_userToolbox.gameObject.name} || Time: {Time.time}");

                // Initialize the toolbox
                m_userToolbox.Init();
            }
        }

        protected void SpawnDisposalBag()
        {
            if (!m_disposalBag && m_disposalBagPrefab)
            {
                // Spawn the item
                XRToolbeltItem disposalBagItem = SpawnItem(m_disposalBagPrefab.gameObject, m_toolbelt.BackWaistAttachPoint, true);
                m_disposalBag = disposalBagItem.GetComponent<DisposalBag>();
                // Initialize the disposal bag
                m_disposalBag.Init();
            }
        }
        #endregion

        #region Helper Methods
        public void RespawnEquipment()
        {
            if (m_userToolbox != null) m_userToolbox.ForceReturnToSocket();
        }

        public int GetToolboxActiveItemCount()
        {
            int activeItemCount = 0;
            // Make sure inventory exists
            if (m_userToolbox && m_userToolbox.Items != null)
            {
                for(int i = 0; i < m_userToolbox.Items.Count; i++)
                {
                    XRToolbeltItem item = m_userToolbox.Items[i];
                    if (item != null && !item.IsAttached) activeItemCount++;
                }
            }
            return activeItemCount;
        }
        #endregion
        // Update is called once per frame
        void Update()
        {

        }
    }
}

