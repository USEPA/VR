using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SampleWipe : SampleTool
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] List<Collider> m_modeColliders;
        [SerializeField] RadialMenu m_selectionMenuPrefab;
        [SerializeField] List<Sprite> m_modeSprites;
        [SerializeField] SampleWipeTemplate m_templatePrefab;
        [Header("Default Configuration")]
        [SerializeField] WipeMode m_defaultMode = WipeMode.Horizontal;
        [SerializeField] float m_respawnDelay = 5.0f;

        public float m_currentSampleAmount = 0.0f;
        public ChemicalAgent m_currentAgent = null;
        #endregion
        #region Private Variables
        protected WipeMode m_mode;

        protected Sample m_currentSample;
        protected Collider col;
        protected TriggerEventListener m_currentTriggerListener;
        protected SampleWipeState m_currentWipeState;
        protected List<SampleUnitCollision> sampleCollisionQueue;
        public int sampleCollisionsPerUpdate = 0;

        protected SampleWipeTemplate m_template;
        protected float m_wipeStateCountFactor = 1.0f;

        protected Action<SampleReportOld> m_onSampleComplete;
        public Vector2 mousePosition;
        protected Vector3 prevPosition;
        protected Vector3 initialPosition;
        private float m_respawnTimer = -1.0f;

        public Vector3 direction;

        protected Dictionary<WipeMode, TriggerEventListener> m_wipeModeTriggers;
        protected Dictionary<WipeMode, SampleWipeState> m_wipeModes;
        RadialMenu m_wipeModeSelectionMenu;
        Camera mainCamera;
        float cameraDistance;

        bool m_forcedInView = false;
        #endregion
        #region Public Properties
        public override ToolType Type => ToolType.Wipe;
        public WipeMode Mode { get => m_mode; }

        public Sample CurrentSample { get => m_currentSample; }

        public Collider Collider { get => col; }
        public List<SampleUnitCollision> SampleCollisionQueue { get => sampleCollisionQueue; }
        public int SampleCollisionsPerUpdateCount { get => sampleCollisionQueue.Count; }

        public float WipeStateCountFactor { get => m_wipeStateCountFactor; }
        public float RespawnDelay { get => m_respawnDelay; }

        public Action OnDestroyed { get; set; }
        #endregion

        #region Initialization
        public override void Init()
        {
            /*
            // Check if wipe mode dictionary has not been initialized
            if (m_wipeModeTriggers == null)
            {
                m_wipeModeTriggers = new Dictionary<WipeMode, TriggerEventListener>();
                for(int i = 0; i < m_modeColliders.Count; i++)
                {
                    // Get the wipe mode
                    WipeMode wipeMode = (WipeMode)i;
                    // Add a SampleWipeState component to this collider
                    SampleWipeState wipeState = m_modeColliders[i].gameObject.AddComponent<SampleWipeState>();
                    wipeState.Init(this, wipeMode);
                    // Add a TriggerEventListener to this collider
                    TriggerEventListener triggerListener = m_modeColliders[i].gameObject.AddComponent<TriggerEventListener>();
                    triggerListener.Init();
                    m_wipeModes.Add((WipeMode)i, triggerListener);
                    triggerListener.gameObject.SetActive(false);
                }

                SetWipeMode(m_defaultMode);
            }
            */
            if (m_wipeModes == null)
            {
                m_wipeModes = new Dictionary<WipeMode, SampleWipeState>();
                for (int i = 0; i < m_modeColliders.Count; i++)
                {
                    // Get the wipe mode
                    WipeMode wipeMode = (WipeMode)i;
                    // Add a SampleWipeState component to this collider
                    SampleWipeState wipeState = m_modeColliders[i].gameObject.AddComponent<SampleWipeState>();
                    wipeState.Init(this, wipeMode, m_modeColliders[i]);
                    m_wipeModes.Add(wipeMode, wipeState);
                    // Disable the game object by default
                    wipeState.gameObject.SetActive(false);
                }
                // Set the wipe state count factor
                //m_wipeStateCountFactor = 1.0f / (float)m_wipeModes.Count;
                m_wipeStateCountFactor = 1.0f;
            }
            /*
            m_template = Instantiate(m_templatePrefab);
            // Get the interactable component of the vial and add it to the user's toolbelt
            if (m_template.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
            {
                XRToolbeltItem beltItem = L58XRAvatar.Instance.Toolbelt.CreateToolObjectOnBelt(interactable);
            }
            */
            /*
            // Initialize the template
            m_template?.Init();
            */

            SetWipeMode(m_defaultMode);
            // Initialize sample collision queue
            sampleCollisionsPerUpdate = 0;
            sampleCollisionQueue = new List<SampleUnitCollision>();
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
            List<GameObject> spawnedObjects = new List<GameObject>();
            if (m_templatePrefab != null)
            {
                // Get the attach point
                //Transform attachPoint = VRUserManager.Instance.Player.Toolbelt.AttachPoints[0];
                XRSocket attachPoint = VRUserManager.Instance.Player.Toolbelt.AttachPoints[0].GetComponent<XRSocket>();
                // Instantiate the template
                m_template = Instantiate(m_templatePrefab, attachPoint.transform);
                if (m_template.TryGetComponent<Collider>(out Collider col))
                {
                    if (VRUserManager.Instance.Player && VRUserManager.Instance.Player.CharacterController != null)
                    {
                        Physics.IgnoreCollision(VRUserManager.Instance.Player.CharacterController, col, true);
                    }
                }
                XRToolbeltItem item = m_template.GetComponent<XRToolbeltItem>();
                if (item == null) item = m_template.gameObject.AddComponent<XRToolbeltItem>();
                // Initialize the toolbelt item
                item.Init(attachPoint, true);
                /*
                if (m_template.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    XRToolbeltItem item = m_template.gameObject.AddComponent<XRToolbeltItem>();
                    item.Init(interactable, attachPoint);
                    item.AttachToBelt();
                }
                */
                m_template.Init();
                // Add this to the spawned objects
                spawnedObjects.Add(m_template.gameObject);
            }
            return spawnedObjects;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_currentSample != null)
            {
                m_currentAgent = m_currentSample.Chemical;
                m_currentSampleAmount = m_currentSample.Amount;
            }
        }

        #region State-Related Functionality
        /*public void SetWipeMode(WipeMode _mode)
        {
            //UnityEngine.Debug.Log($"Attempting to set cloth to mode: {_mode} | Current Mode: {m_mode} || Time: {Time.time}");
            // Make sure there is a value for this wipe mode
            if (m_wipeModeTriggers[_mode] == null || m_currentTriggerListener == m_wipeModeTriggers[_mode]) return;
            // Check if there is a current collider
            if (m_currentTriggerListener != null)
            {
                m_currentTriggerListener.OnTriggerEntered -= i => ProcessTriggerEnter(i);
                m_currentTriggerListener.OnTriggerExited -= i => ProcessTriggerExit(i);
                m_currentTriggerListener.gameObject.SetActive(false);
            }
            m_mode = _mode;
            m_currentTriggerListener = m_wipeModeTriggers[_mode];
            m_currentTriggerListener.OnTriggerEntered += i => ProcessTriggerEnter(i);
            m_currentTriggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            col = m_currentTriggerListener.Collider;
            m_currentTriggerListener.gameObject.SetActive(true);
        }*/
        public void SetRespawnTimer()
        {
            GetComponent<XRToolbeltItem>().AttachToBelt();
            UnityEngine.Debug.Log($"About to start respawn timer || Time: {Time.time}");
            StartCoroutine(Respawn());
            //gameObject.SetActive(false);
        }

        public IEnumerator Respawn()
        {
            float normalizedTime = 0.0f;
            while (normalizedTime <= 1.0f)
            {
                normalizedTime += Time.deltaTime / m_respawnDelay;
                //UnityEngine.Debug.Log($"Respawn: {normalizedTime} || Time: {Time.time}");
                yield return null;
            }
            UnityEngine.Debug.Log($"Should be respawned || Time: {Time.time}");
            //gameObject.SetActive(true);
            //m_respawnTimer = -1.0f;
        }
        public void SetWipeMode(WipeMode _mode)
        {
            // Make sure there is a value for this wipe mode
            if (m_wipeModes[_mode] == null || m_currentWipeState == m_wipeModes[_mode]) return;
            // Check if there is a current wipe state
            if (m_currentWipeState != null)
            {
                // Perform exit functionality
                m_currentWipeState.OnStateExit();
            }
            // Set references
            m_mode = _mode;
            m_currentWipeState = m_wipeModes[_mode];
            col = m_currentWipeState.Collider;
            // Perform enter functionality
            m_currentWipeState.OnStateEnter();
        }

        public void ToggleWipeModeSelectionMenu()
        {
            if (m_wipeModeSelectionMenu == null)
                OpenWipeModeSelectionMenu();
            else
                CloseWipeModeSelectionMenu();
        }

        public void OpenWipeModeSelectionMenu()
        {
            m_wipeModeSelectionMenu = Instantiate(m_selectionMenuPrefab, VRUserManager.Instance.MenuContainer);
            m_wipeModeSelectionMenu.SetTitle("Select Cloth Fold Mode");
            for (int i = 0; i < m_wipeModeSelectionMenu.Options.Count; i++)
            {
                RadialMenuButton button = m_wipeModeSelectionMenu.Options[i];
                button.Icon.sprite = m_modeSprites[i];
                int index = i;
                WipeMode mode = (WipeMode)index;
                button.Button.gameObject.name = $"{button.Button.gameObject.name}-{(WipeMode)index}";

                if (mode == m_mode)
                {
                    EventSystem.current.SetSelectedGameObject(button.Button.gameObject);
                }
                else
                {
                    button.OnClick += () =>
                    {
                        SetWipeMode(mode);
                        CloseWipeModeSelectionMenu();
                    };
                }
            }
            VRUserManager.Instance.SetState(PlayerState.Menu);
        }

        public void CloseWipeModeSelectionMenu()
        {
            if (m_wipeModeSelectionMenu == null) return;
            Destroy(m_wipeModeSelectionMenu.gameObject);
            m_wipeModeSelectionMenu = null;
            VRUserManager.Instance.SetState(VRUserManager.Instance.PreviousState);
        }
        #endregion

        #region Sample-Related Functionality
        public void SetSample(Sample _newSample)
        {
            m_currentSample = _newSample;
            m_currentSample.UsedTool = this;
        }

        public void ClearSample()
        {
            m_currentSample = null;
            ClearSampleQueue();
        }

        public void ClearSampleQueue()
        {
            if (m_wipeModes == null || m_wipeModes.Count < 1) return;
            // Clear each wipe state's collision queue
            foreach(SampleWipeState wipeState in m_wipeModes.Values)
            {
                // Clear the queue
                wipeState.ClearSampleQueue();
            }
            //sampleCollisionQueue.Clear();
        }
        #endregion

        #region Collision-Related Functionality
        private void ProcessTriggerEnter(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj))
            {
                if (sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit) < 0)
                {
                    // Calculate point of impact/direction
                    Vector3 hitPoint = col.ClosestPoint(unitObj.transform.position);
                    //Vector3 hitDirection = (transform.position - hitPoint).normalized;
                    Vector3 hitDirection = direction;
                    //unitObj.Unit.SetIsSample(true, 0.0f);
                    unitObj.Unit.SetCleared(true);
                    unitObj.SetColor();
                    //if (SimulationManager.Instance.DebugMode) unitObj.SetColor();
                    // Add this to the collisions per update
                    sampleCollisionQueue.Add(new SampleUnitCollision(unitObj.Unit, hitPoint, hitDirection));
                    sampleCollisionsPerUpdate++;
                    // Check whether or not there is a sample associated with this
                }
            }
        }

        private void ProcessTriggerExit(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj))
            {
                int index = sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit);
                if (index >= 0)
                {
                    // Calculate point of exit
                    Vector3 exitPoint = col.ClosestPointOnBounds(unitObj.transform.position);
                    Vector3 exitDirection = (transform.position - exitPoint).normalized;
                    sampleCollisionQueue[index].SetExitDirection(exitDirection);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //ProcessTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            //ProcessTriggerExit(other);
        }
        #endregion

        #region Report-Related Functionality
        public override SampleReportOld GenerateReport(ScenarioStep _step)
        {
            // Return report
            var report = new SampleReportOld(_step, sampleCollisionQueue, sampleCollisionQueue.Count, ((float)sampleCollisionQueue.Count / (float)m_currentSampleArea.TotalSamples));
            UnityEngine.Debug.Log(report.ToString());
            // Fire off action with this report
            m_onSampleComplete?.Invoke(report);
            // Reset queue
            ClearSampleQueue();
            return report;
        }
        #endregion

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
            //StopAllCoroutines();
        }

        public override void IdentifyChemical(Sample _sample)
        {
            base.IdentifyChemical(_sample);
            UnityEngine.Debug.Log($"{gameObject.name} identified chemical: {_sample.Chemical.Name} || Time: {Time.time}");
        }
        #region Helper Methods
        public override void ForceToggleInView()
        {
            if (!m_forcedInView)
            {
                // Get headset position
                //Vector3 targetPosition = new Vector3(transform.position.x, Camera.main.transform.position.y - 0.1f, transform.position.z);
                Vector3 targetPosition = Camera.main.transform.position + (Camera.main.transform.forward * 0.35f);
                targetPosition.y -= 0.12f;
                transform.localPosition = transform.parent.InverseTransformPoint(targetPosition);
                //transform.localPosition = transform.InverseTransformPoint(targetPosition);
                //transform.parent = Camera.main.transform;
                //transform.localEulerAngles = new Vector3(-40.0f, 0.0f, 0.0f);

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

        public override void ReturnToBelt()
        {
            if (m_template)
            {
                m_template.ClearSurface();
                m_template.SocketItem.ForceAttachToSocket();
            }
            base.ReturnToBelt();
        }
#if UNITY_EDITOR
        [ContextMenu("Set Random Wipe Mode")]
        public void SetRandomWipeMode()
        {
            if (m_wipeModeTriggers == null) return;
            List<WipeMode> possibleModes = new List<WipeMode>();

            foreach(WipeMode possibleMode in m_wipeModeTriggers.Keys)
            {
                possibleModes.Add(possibleMode);
            }
            possibleModes.Remove(m_mode);
            SetWipeMode(possibleModes[UnityEngine.Random.Range(0, possibleModes.Count)]);
        }
        #endif
        #endregion
    }

    public enum WipeMode { Horizontal, Vertical, Diagonal, Perimeter}

    public class SampleUnitCollision
    {
        #region Private Variables
        private SampleUnit m_sampleUnit;
        private Vector3 m_hitPoint;
        private Vector2 m_hitDirection;
        private Vector2 m_exitDirection;
        #endregion
        #region Public Properties
        public SampleUnit Unit { get => m_sampleUnit; }
        public Vector3 HitPoint { get => m_hitPoint; }
        public Vector2 HitDirection { get => m_hitDirection; }
        public Vector2 ExitDirection { get => m_exitDirection; }

        public Vector2Int Index { get => m_sampleUnit.Index; }

        public Vector2 UVPosition { get => m_sampleUnit.UVPosition; }
        #endregion

        public SampleUnitCollision(SampleUnit _unit, Vector3 _hitPoint, Vector2 _hitDirection)
        {
            m_sampleUnit = _unit;
            m_hitPoint = _hitPoint;
            m_hitDirection = _hitDirection;
        }

        public void SetExitDirection(Vector2 _exitDirection)
        {
            m_exitDirection = _exitDirection;
        }
    }
}

