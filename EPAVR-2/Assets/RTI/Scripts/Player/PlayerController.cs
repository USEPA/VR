using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    [RequireComponent(typeof(Inventory))]
    public abstract class PlayerController : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected XROrigin m_xrRig;
        [SerializeField] protected L58XRAvatar m_avatar;
        [SerializeField] protected RectTransform m_menuContainer;
        [SerializeField] protected Transform m_popupContainer;
        [SerializeField] protected VRUserToolSelection m_toolSelectionMenuPrefab;
        [SerializeField] protected DebugConsole m_debugConsole;
        [SerializeField] protected UserTablet m_userTabletPrefab;
        [SerializeField] protected UserToolbox m_userToolboxPrefab;
        [SerializeField] protected DisposalBag m_disposalBagPrefab;
        [SerializeField] protected GameObject m_scoopulaPrefab;
        [SerializeField] protected GameObject m_pipettePrefab;
        [SerializeField] protected Transform m_neck;
        [Header("Default Configuration")]
        [SerializeField] SitStandingMode m_sitStandMode = SitStandingMode.Standing;
        [SerializeField] protected MovementMode m_defaultMovementMode;
        [SerializeField] protected Vector2 m_teleportInputRange = new Vector2(0.3f, -0.5f);
        [Tooltip("If true the player will ignore the first HMD movement on detection. " +
             "If the HMD is not centered the player would move away from it's placed position to where the HMD is.")]
        [SerializeField] protected bool m_initialHMDAdjustment = true;
        [SerializeField] protected bool m_initWithOffset = true;
        #endregion
        #region Protected Variables
        protected MovementMode m_currentMovementMode;
        public bool m_hasTeleportInput = false;
        protected bool m_isTeleporting = false;
        protected CharacterController m_characterController;
        protected Inventory m_inventory;
        protected List<GameObject> m_equipment;
        protected UserTablet m_userTablet;
        public UserToolbox m_userToolbox;
        protected DisposalBag m_disposalBag;


        protected L58TeleportationArea m_currentTeleportTarget;
        protected XRTooltip m_currentPopup;
        protected Vector2 m_cameraYOffsetRange;
        protected bool m_initialHeightCalibrated = false;
        //protected VRUserToolSelection m_toolSelectionMenu;

        public SampleTool m_currentTool;
        private ToolInstance m_currentToolInstance;
        public bool m_userIsPresent = false;

        protected InputActionReference m_teleportValueAction;
        public Vector2 m_teleportActionValue;
        public bool m_teleportInputValid = false;

        protected Camera m_camera;
        protected Transform m_cameraRig;
        protected Transform m_cameraScaleTransform;

        protected float m_playerHeight = 1.66f;
        protected float m_eyeHeight = 1.66f;
        protected float m_sittingOffset = 0.0f;
        protected float m_adjustedCameraHeight;

        protected bool m_waitingForCameraMovement = true;
        protected Vector3 m_cameraStartingPosition;

        protected float m_rigScale = 1f;


        protected Action<MovementMode> m_onSetMovementMode;
        protected Action<SitStandingMode> m_onSetSitStandMode;
        protected Action<float> m_onHeightCalibrated;

        private Action<SampleTool> m_onSetTool;

        #endregion
        #region Public Properties
        public XROrigin XRRig { get => m_xrRig; }
        public L58XRAvatar Avatar { get => m_avatar; }
        public CharacterController CharacterController { get => m_characterController; }
        public virtual Canvas MenuCanvas { get; }
        public RectTransform MenuContainer { get => m_menuContainer; }
        public Transform PopupContainer { get => m_popupContainer; }
        public XRTooltip CurrentPopup { get => m_currentPopup; }

        public SampleTool CurrentTool { get => m_currentTool; }
        public ToolType CurrentToolType { get => m_currentTool.Type; }

        public float CurrentToolReading { get => m_currentTool.CurrentReading; }

        public Inventory Inventory { get => m_inventory; }
        public List<GameObject> Equipment { get => m_equipment; }
        public UserTablet Tablet { get => m_inventory.Tablet; }
        public UserToolbox Toolbox { get => m_inventory.Toolbox; }
        public DisposalBag DisposalBag { get => m_inventory.DisposalBag; }

        public XRToolbelt Toolbelt { get => m_avatar.Toolbelt; }
        public XRSocket MainDeviceAttachPoint { get => Toolbelt.MainDeviceAttachPoint; }

        public DebugConsole DebugConsole { get => m_debugConsole; }

        public MovementMode DefaultMovementMode { get => m_defaultMovementMode; set => m_defaultMovementMode = value; }
        public MovementMode CurrentMovementMode { get => m_currentMovementMode; }

        public SitStandingMode SitStandMode { get => m_sitStandMode; set => m_sitStandMode = value; }
        public XROrigin.TrackingOriginMode TrackingMode { get => m_xrRig.RequestedTrackingOriginMode; }

        public Transform CameraScale { get => m_avatar.CameraScale; }
        public Camera Camera 
        { 
            get
            {
                if (!m_camera) m_camera = m_xrRig.Camera;
                return m_camera;
            }
        }

        public Transform CameraRig
        {
            get
            {
                if (!m_cameraRig) m_cameraRig = m_xrRig.CameraFloorOffsetObject.transform;
                return m_cameraRig;
            }
        }
        public float CalibratedHeight { get => m_playerHeight; }
        public float EyeHeight { get => m_eyeHeight; set => m_eyeHeight = value; }
        public float SittingOffset { get => m_sittingOffset; set => m_sittingOffset = value; }
        public float AdjustedCameraHeight { get => m_adjustedCameraHeight; set => m_adjustedCameraHeight = value; }

        public float RigScale { get => m_rigScale; }

        public Transform CameraFloorOffset { get => m_xrRig.CameraFloorOffsetObject.transform; }
        public float CameraYOffset { get => m_xrRig.CameraYOffset; set => m_xrRig.CameraYOffset = value; }

        public bool UserIsPresent { get => m_userIsPresent; }
        public bool InitialHeightCalibrated { get => m_initialHeightCalibrated; }


        public InputActionReference TeleportValueAction { get => m_teleportValueAction; set => m_teleportValueAction = value; }
        public Action<MovementMode> OnSetMovementMode { get => m_onSetMovementMode; set => m_onSetMovementMode = value; }
        public Action<SitStandingMode> OnSetSitStandMode { get => m_onSetSitStandMode; set => m_onSetSitStandMode = value; }
        public Action<float> OnHeightCalibrated { get => m_onHeightCalibrated; set => m_onHeightCalibrated = value; }

        public Action<SampleTool> OnSetTool { get => m_onSetTool; set => m_onSetTool = value; }
        #endregion

        private void Awake()
        {
            if (VRUserManager.Instance && VRUserManager.Instance.Player != null && VRUserManager.Instance.Player != this)
            {
                Destroy(gameObject);
            }
            // Cache references
            m_inventory = GetComponent<Inventory>();
            m_camera = m_xrRig.Camera;
            m_cameraRig = m_xrRig.CameraFloorOffsetObject.transform;
            // Get initial position
            m_cameraStartingPosition = m_camera.transform.localPosition;
        }

        #region Initialization
        private void Start()
        {
            Reset();
        }

        public virtual void Init()
        {
            var offset = CameraYOffset;

            if (m_initWithOffset && CameraFloorOffset)
            {
                var pos = CameraFloorOffset.localPosition;
                CameraFloorOffset.transform.localPosition = new Vector3(pos.x, offset, pos.z);
            }

            // Set default movement mode
            m_currentMovementMode = MovementMode.Continuous;
            // Initialize the avatar
            m_avatar.Init(this);
            Vector3 cameraPos = Camera.transform.position;
            m_xrRig.transform.position = Vector3.zero;
            m_xrRig.MoveCameraToWorldLocation(new Vector3(0.0f, cameraPos.y, 0.0f));
            //m_xrRig.transform.position = new Vector3(cameraPos.x, transform.position.y, cameraPos.z);
            // Cache character controller if available
            m_xrRig.TryGetComponent<CharacterController>(out m_characterController);
            // Initialize the inventory references
            m_inventory.Init(this);
        }
        #endregion

        #region Update
        protected virtual void Update()
        {
            // Update rig height as needed
            UpdateCharacterHeight();
            // Check for teleprot input
            m_teleportActionValue = m_teleportValueAction.action.ReadValue<Vector2>();
            if (m_hasTeleportInput) CheckForTeleport();
        }


        #endregion

        #region Fixed Update
        protected virtual void FixedUpdate()
        {
            // Check for headset movement
            if (m_waitingForCameraMovement)
                CheckCameraMovement();

            if (!CharacterController || !CharacterController.enabled) return;

            // Handle HMD movement
            HandleHMDMovement();
        }

        protected virtual void CheckCameraMovement()
        {
            if (Vector3.Distance(m_cameraStartingPosition, Camera.transform.localPosition) < .05f)
            {
                return;
            }

            var delta = Camera.transform.position - CharacterController.transform.position;
            delta.y = 0f;
            CameraRig.transform.position -= delta;
            m_waitingForCameraMovement = false;
        }

        protected virtual void HandleHMDMovement()
        {
            // Check if character controller exists
            if (m_waitingForCameraMovement || !CharacterController) return;

            var originalCameraPosition = CameraRig.transform.position;
            var originalCameraRotation = CameraRig.transform.rotation;
            // Get the distance from the neck to the character controller
            var delta = m_neck.transform.position - CharacterController.transform.position;
            delta.y = 0.0f;
            // Move the character controller along with the headset
            if (delta.magnitude > 0.0f && CharacterController.enabled)
            {
                CharacterController.Move(delta);
            }

            // Match rotation
            m_xrRig.transform.rotation = Quaternion.Euler(0.0f, m_neck.rotation.eulerAngles.y, 0.0f);

            CameraRig.transform.position = originalCameraPosition;
            var local = CameraRig.transform.localPosition;
            local.y = 0f;
            CameraRig.transform.localPosition = local;
            CameraRig.transform.rotation = originalCameraRotation;
        }
        #endregion
        #region Equipment-Related Functionality
        public void SpawnEquipment()
        {
            // Tell the inventory handler to spawn equipment
            m_inventory.SpawnEquipment(CoreGameManager.Instance.CurrentGamemodeConfig);
            // Cache references
            if (m_inventory.Tablet != null) m_userTablet = m_inventory.Tablet;
            if (m_inventory.Toolbox != null) m_userToolbox = m_inventory.Toolbox;
            if (m_inventory.DisposalBag != null) m_disposalBag = m_inventory.DisposalBag;
            /*
            // Initialize equipment list
            m_equipment = new List<GameObject>();
            // Spawn the user tablet
            if (!m_userTablet && m_userTabletPrefab)
            {
                // Get the target socket
                XRSocket tabletSocket = m_avatar.Toolbelt.TabletAttachPoint.GetComponent<XRSocket>();

                //m_userTablet = Instantiate(m_userTabletPrefab, m_avatar.IK.BackAttachPoint).GetComponent<UserTablet>();
                m_userTablet = Instantiate(m_userTabletPrefab, tabletSocket.transform).GetComponent<UserTablet>();
                if (m_userTablet.TryGetComponent<XRSocketItem>(out XRSocketItem socketItem))
                {
                    // Initialize the item
                    socketItem.Init(m_userTablet.GetComponent<XRGrabInteractable>(), tabletSocket.transform);
                    // Initialize the tablet socket
                    tabletSocket.Init(socketItem);
                }
                m_userTablet.Init();
                m_equipment.Add(m_userTablet.gameObject);
            }
            // Spawn the user toolbox
            if (!m_userToolbox && m_userToolboxPrefab)
            {
                // Get the target socket
                XRSocket toolboxSocket = m_avatar.IK.BackAttachPoint.GetComponent<XRSocket>();
                m_userToolbox = Instantiate(m_userToolboxPrefab, toolboxSocket.transform).GetComponent<UserToolbox>();
                // Initialize the socket component of the toolbox
                if (m_userToolbox.TryGetComponent<XRSocketItem>(out XRSocketItem socketItem))
                {
                    // Initialize the item
                    socketItem.Init(m_userToolbox.GetComponent<XRGrabInteractable>(), toolboxSocket.transform);
                    // Initialize the tablet socket
                    toolboxSocket.Init(socketItem);
                }
                m_userToolbox.Init();
                m_equipment.Add(m_userToolbox.gameObject);
            }
            // Spawn the disposal bag
            if (!m_disposalBag && m_disposalBagPrefab)
            {
                m_disposalBag = Instantiate(m_disposalBagPrefab);
                if (m_disposalBag.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    m_avatar.Toolbelt.CreateToolObject(interactable, m_avatar.Toolbelt.BackWaistAttachPoint, true);
                }
                m_disposalBag.Init();
                m_equipment.Add(m_disposalBag.gameObject);
            }
            */
        }

        public void RemoveEquipment()
        {
            // Tell the inventory to remove all equipment
            m_inventory.RemoveEquipment();
            // Tell the avatar to remove any gloves equipped
            m_avatar.ClearGloves();
            // Clear all references
            m_userTablet = null;
            m_userToolbox = null;
            m_disposalBag = null;
            /*
            // Remove any registered items
            if (m_equipment != null && m_equipment.Count > 0)
            {
                foreach (GameObject obj in m_equipment) Destroy(obj);
            }
            */
        }

        public int GetActiveItemCount()
        {
            // Make sure inventory exists
            if (!m_inventory) return 0;

            int activeitemCount = 0;
            activeitemCount += m_inventory.GetToolboxActiveItemCount();

            if (m_currentTool != null && m_currentTool.SocketItem != null)
            {
                activeitemCount += m_currentTool.GetAdditionalActiveItemCount();
            }
            return activeitemCount;
        }
        #endregion

        #region Tool-Related Functionality
        public void SpawnTool(ToolType _toolType)
        {
            // Make sure conditions are valid
            if (m_inventory.ToolInstances == null || m_inventory.ToolInstances.Count < 1 || (m_currentTool != null && m_currentTool.Type == _toolType)) return;
            // Check if there is a previous tool instance active
            if (m_currentTool != null)
            {
                // Call any tool-specific despawn functionality
                m_currentTool.OnDespawn();
                // Check if this tool is linked to the main device point
                if (m_currentTool.SocketItem != null && m_currentTool.SocketItem.LinkSocket)
                {
                    UnityEngine.Debug.Log($"{gameObject.name} about to return {m_currentTool.gameObject.name} to socket: {m_currentTool.SocketItem.LinkedSocket} || Time: {Time.time}");
                    // Force attach the socket back to the main device point
                    m_currentTool.SocketItem?.ForceAttachToSocket();
                }

                // Disable the previous tool
                m_currentToolInstance.SetActive(false);
            }
            // Get the tool instance
            ToolInstance toolInstance = m_inventory.ToolInstances[_toolType];
            m_currentToolInstance = toolInstance;
            m_currentTool = toolInstance.Tool;
            // Enable the tool
            m_currentToolInstance.SetActive(true);
            // Call any tool-specific spawn functionality
            m_currentTool.OnSpawn();
            m_onSetTool?.Invoke(m_currentTool);
        }
        #endregion

        #region UI-Related Functionality
        public virtual void ToggleToolSelectionMenu()
        {

        }

        public virtual void InitToolSelectionMenu() 
        { 
        }


        public virtual void SetPopup(XRTooltip _popup, string _message)
        {
            m_currentPopup = _popup;
            m_currentPopup.SetText(_message);
            Destroy(m_currentPopup.gameObject, 2.0f);
        }
        #endregion

        #region Calibration-Related Functionality
        public void InitHeight(Vector2 _heightRange, float _defaultHeight = 0.0f)
        {
            // Cache height range
            m_cameraYOffsetRange = _heightRange;
            // Set default height
            CalibrateHeight(_defaultHeight);
            //SetHeight(_defaultHeight);
        }

        public void SetSitStandMode(SitStandingMode _mode)
        {
            // Set the new mode
            SitStandMode = _mode;
            // Invoke any necessary events
            OnSetSitStandMode?.Invoke(_mode);
            // Re-calibrate height
            CalibrateHeightFromOffset();
        }

        public void SetTrackingMode(XROrigin.TrackingOriginMode _mode)
        {
            // Set the requested tracking mode
            m_xrRig.RequestedTrackingOriginMode = _mode;
        }

        public void ToggleSitStandMode()
        {
            if (m_sitStandMode == SitStandingMode.Standing)
            {
                SetSitStandMode(SitStandingMode.Sitting);
            }
            else if (m_sitStandMode == SitStandingMode.Sitting)
            {
                SetSitStandMode(SitStandingMode.Standing);
            }
        }

        public void ToggleTrackingMode()
        {
            if (TrackingMode == XROrigin.TrackingOriginMode.Device)
            {
                SetTrackingMode(XROrigin.TrackingOriginMode.Floor);
            }
            else if (TrackingMode == XROrigin.TrackingOriginMode.Floor)
            {
                SetTrackingMode(XROrigin.TrackingOriginMode.Device);
            }

            ResetRigScale();
        }

        public void ResetRigScale()
        {
            m_rigScale = 1f;
            if (CameraScale)
            {
                CameraScale.localScale = new Vector3(m_rigScale, m_rigScale, m_rigScale);
            }
        }
        public void CalibrateHeightFromOffset()
        {
            // Get height based on the camera's current local position and the current Y offset
            float height = Camera.transform.localPosition.y + CameraYOffset;
            CalibrateHeight(height);
        }

        public void CalibrateHeightFromFloorTrackMode()
        {
            // Get the current camera Y offset
            float currentOffset = CameraYOffset;
            Vector3 cameraLocalPos = Camera.main.transform.localPosition;
            float offsetHeight = currentOffset + cameraLocalPos.y;
            float raycastHeight = 0.0f;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, -Vector3.up, out hit, 10.0f, LayerMask.GetMask("Teleport")))
            {
                // Get the distance
                raycastHeight = hit.distance;
            }
            //UnityEngine.Debug.Log($"{gameObject.name} - Height Calibration | Offset Distance: {offsetHeight} m ({MathHelper.ConvertMetersToFeet(offsetHeight)} ft) | Actual Distance: {raycastHeight} m ({MathHelper.ConvertMetersToFeet(raycastHeight)} ft) | Current Offset: {currentOffset} | Camera Y Local Pos: {cameraLocalPos.y} || Time: {Time.time}");
            float targetHeight = offsetHeight;
            if (cameraLocalPos.y > 0.0f)
            {
                targetHeight = offsetHeight - cameraLocalPos.y;
            }
            // Start the calibrate height routine
            StartCoroutine(CalibrateHeightRoutine());
            /*
            // Set the camera T offset
            SetHeight(targetHeight);
            //Camera.main.transform.localPosition = new Vector3(cameraLocalPos.x, 0.0f, cameraLocalPos.z);
            m_onHeightCalibrated?.Invoke(CameraYOffset);
            */

            if (!m_initialHeightCalibrated) m_initialHeightCalibrated = true;
        }

        protected IEnumerator CalibrateHeightRoutine()
        {
            // First, briefly switch the tracking mode to floor
            //m_xrRig.TrackingOriginMode = UnityEngine.XR.TrackingOriginModeFlags.Floor;
            m_xrRig.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            // Wait a frame
            yield return null;
            // Get the camera Y Offset
            float trackedCameraHeight = Camera.main.transform.localPosition.y;
            float scaleFactor = GetPlayerScaleFactor(trackedCameraHeight);
            float heightRefDifference = VRUserManager.Instance.DefaultHeightReference - trackedCameraHeight;
            float adjustedHeight = trackedCameraHeight * scaleFactor;
            UnityEngine.Debug.Log($"Switched mode to floor | Tracked Height: {MathHelper.GetMeasurementDisplay(trackedCameraHeight)} | Height Difference: {MathHelper.GetMeasurementDisplay(heightRefDifference)} || Time: {Time.time}");
         
            // Set this as the desired Y offset
            SetHeight(trackedCameraHeight);
            m_onHeightCalibrated?.Invoke(CameraYOffset);
            // Return to device tracking
            //m_xrRig.trackingOriginMode = UnityEngine.XR.TrackingOriginModeFlags.Device;
            m_xrRig.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

            if (VRUserManager.Instance.SaveCalibratedHeight)
            {
                PlayerPrefs.SetFloat(VRUserManager.HeightKey, CameraYOffset);
                PlayerPrefs.Save();
            }
        }

        public void CalibrateHeight(float _height)
        {
            // Check sit stand mode
            if (m_sitStandMode == SitStandingMode.Standing)
            {
                // Cap height at current eye height if it's too low
                if (_height < .01f)
                {
                    _height = EyeHeight;
                }

                SittingOffset = 0f;
                // Set the scale of the rig
                m_rigScale = EyeHeight / _height;
            }
            else if (m_sitStandMode == SitStandingMode.Sitting)
            {
                SittingOffset = EyeHeight - _height;
                m_rigScale = 1f;
            }

            m_playerHeight = _height;
            UnityEngine.Debug.Log($"{gameObject.name} finishing height calibration: {GetHeightDisplay(_height)} | Scale: {m_rigScale} | Mode: {m_sitStandMode} | Sitting Offset: {SittingOffset} || Time: {Time.time}");
            // Scale the rig as necessary
            if (CameraScale)
            {
                CameraScale.localScale = new Vector3(m_rigScale, m_rigScale, m_rigScale);
            }
            // Invoke any necessary events
            m_onHeightCalibrated?.Invoke(m_playerHeight);
            // Check if height should be saved
            if (VRUserManager.Instance.SaveCalibratedHeight)
            {
                PlayerPrefs.SetFloat(VRUserManager.HeightKey, _height);
                PlayerPrefs.Save();
            }
        }
        #endregion

        #region Helper Methods
        public virtual void ToggleMovementMode()
        {
            SetMovementMode((m_currentMovementMode == MovementMode.Continuous) ? MovementMode.Teleport : MovementMode.Continuous);
            //m_onSetMovementMode.Invoke(m_currentMovementMode);
        }
        public virtual void SetMovementMode(MovementMode _newMode)
        {
            // Check for teleport cancel
            if (m_currentMovementMode == MovementMode.Teleport && m_isTeleporting)
                DisableTeleportation();
            m_currentMovementMode = _newMode;
            // Invoke any attached events
            m_onSetMovementMode?.Invoke(_newMode);
        }

        public virtual void ToggleTeleportation()
        {
            // Make sure this only occurs when in teleportation
            if (m_currentMovementMode != MovementMode.Teleport) return;

            if (!m_isTeleporting)
                EnableTeleportationInput();
            else
                DisableTeleportation();
        }

        public virtual void EnableTeleportationInput()
        {
            //UnityEngine.Debug.Log($"Arrived in EnableTeleportation || Time: {Time.time}");
            m_hasTeleportInput = true;
            // Make sure this only occurs when in teleportation
            //if (m_currentMovementMode != MovementMode.Teleport) return;
            /*
            if (m_teleportValueAction)
            {
                // Get the value
                Vector2 axis = m_teleportValueAction.action.ReadValue<Vector2>();
            }
            //return TeleportController.JoystickAxis.y < -.5f && Mathf.Abs(TeleportController.JoystickAxis.x) < .30;
            // Configure ray interactor for teleportation
            m_avatar.LeftHand.OnModeToggled(true);
            // Set isTeleporting
            m_isTeleporting = true;
            */
        }

        public void AttemptTeleportation()
        {
            // Make sure this only occurs when in teleportation
            if (m_currentMovementMode != MovementMode.Teleport || !m_isTeleporting) return;
            // Check if there is a valid teleportation target
            if (m_currentTeleportTarget != null)
            {
                ActivateEventArgs args = new ActivateEventArgs();
                args.interactor = m_avatar.LeftHand.RayInteractor;
                args.interactable = m_currentTeleportTarget;
                m_currentTeleportTarget.Activate(args);
            }
            // Disable teleportation
            DisableTeleportation();
        }
        public virtual void DisableTeleportation()
        {
            //UnityEngine.Debug.Log($"Arrived in DisableTeleportation || Time: {Time.time}");
            // Make sure this only occurs when in teleportation
            if (m_currentMovementMode != MovementMode.Teleport || !m_isTeleporting) return;
            // Clear teleport target
            ClearTeleportTarget();
            // Configure ray interactor for normal interaction
            m_avatar.LeftHand.OnModeToggled(false);
            // Set isTeleporting
            m_isTeleporting = false;
            m_hasTeleportInput = false;
        }

        public void EnableTeleport()
        {
            // Make sure this only occurs when in teleportation
            if (m_currentMovementMode != MovementMode.Teleport) return;
            //UnityEngine.Debug.Log($"{gameObject.name} - enabled teleportation || Time: {Time.time}");
            // Configure ray interactor for teleportation
            m_avatar.LeftHand.OnModeToggled(true);
            // Set isTeleporting
            m_isTeleporting = true;
        }

        public void DisableTeleportationInput()
        {
            //UnityEngine.Debug.Log($"Arrived in DisableTeleportationInput || Time: {Time.time}");
            m_hasTeleportInput = false;
        }

        void CheckForTeleport()
        {
            // Make sure player is in Teleport movement mode
            if (m_currentMovementMode != MovementMode.Teleport) return;
            // Get the input action value
            Vector2 value = m_teleportValueAction.action.ReadValue<Vector2>();
            //m_teleportActionValue = value;
            // Check if this is within the range for a teleport queue request
            //bool valueWithinRange = (value.y < m_teleportInputRange.y && Mathf.Abs(value.x) < m_teleportInputRange.x);
            bool valueWithinRange = (value.y > m_teleportInputRange.y && Mathf.Abs(value.x) < m_teleportInputRange.x);
            m_teleportInputValid = valueWithinRange;
            if (valueWithinRange)
            {
                if (!m_isTeleporting) EnableTeleport();
            }
            /*
            else
            {
                if (m_isTeleporting) AttemptTeleportation();
            }
            */
        }
        public virtual void SetTeleportTarget(L58TeleportationArea _area)
        {
            if (!_area.teleportationProvider) _area.teleportationProvider = m_avatar.TeleportProvider;
            m_currentTeleportTarget = _area;
        }

        public virtual void ClearTeleportTarget()
        {
            m_currentTeleportTarget = null;
        }

        public void SetHeight(float _value)
        {
            if (m_xrRig == null || CameraYOffset == _value) return;
            //m_xrRig.cameraYOffset = Mathf.Clamp(_value, m_cameraYOffsetRange.x, m_cameraYOffsetRange.y);
            m_playerHeight = _value;
            //m_xrRig.cameraYOffset = _value;
            m_xrRig.CameraYOffset = _value;
            // Scale the player's avatar
            m_avatar.ScaleAvatarToHeight(_value);
            // Scale the character controller if available
            ScaleCharacterController(_value);
            // Clear the camera's local Y offset
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, 0.0f, Camera.main.transform.localPosition.z);
            /*
            if (VRUserManager.Instance.SaveCalibratedHeight)
            {
                PlayerPrefs.SetFloat(VRUserManager.HeightKey, CameraYOffset);
                PlayerPrefs.Save();
            }
            */
        }

        void UpdateCharacterHeight()
        {
            float height = AdjustedCameraHeight;
            // Scale the player's avatar
            m_avatar.ScaleAvatarToHeight(height);
            // Scale
            ScaleCharacterController(height);
        }
        void ScaleCharacterController(float _height)
        {
            if (!m_characterController) return;
            m_characterController.height = _height;
            m_characterController.center = new Vector3(m_characterController.center.x, _height * 0.5f, m_characterController.center.z);
        }

        protected float GetPlayerScaleFactor(float _desiredHeight)
        {
            // Compare this desired height to the default one
            return (VRUserManager.Instance.DefaultCameraYOffset / _desiredHeight);
        }
        #endregion

    

        public string GetHeightDisplay()
        {
            return GetHeightDisplay(CameraYOffset);
        }

        string GetHeightDisplay(float _value)
        {
            //float displayAdjustedHeight = _value + VRUserManager.Instance.DefaultHeightCalibrateOffset;
            float displayAdjustedHeight = _value;
            string text = $"{(MathHelper.ConvertMetersToFeet(displayAdjustedHeight)).ToString("0.000")} ft ({displayAdjustedHeight.ToString("0.000")} m)";
            return text;
        }

        protected void Reset()
        {
            m_waitingForCameraMovement = m_initialHMDAdjustment;
        }
    }

    public enum MovementMode { Continuous, Teleport }
    public enum SitStandingMode { Sitting, Standing }
}

