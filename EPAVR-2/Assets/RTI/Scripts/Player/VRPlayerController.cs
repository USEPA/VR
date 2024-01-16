using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class VRPlayerController : PlayerController
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] Canvas m_menuCanvas;
        [SerializeField] VRUserToolSelection m_toolSelectionMenu;
        [Header("UI Configuration")]
        [SerializeField] float m_popupMenuDistance = 1.5f;
        #endregion
        #region Private Variables

        #endregion
        #region Public Properties
        public override Canvas MenuCanvas => m_menuCanvas;
        #endregion

        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            if (m_defaultMovementMode != MovementMode.Continuous)
            {
                SetMovementMode(MovementMode.Continuous);
            }
            SetMovementMode(m_defaultMovementMode);
            // Hook up tooltip events
            m_onSetMovementMode += i =>
            {
                string message = (i == MovementMode.Teleport) ? "Movement Mode: Teleport" : "Movement Mode: Continuous";
                VRUserManager.Instance?.SpawnTooltip(m_avatar.LeftHand, message);
            };

            m_avatar.TeleportProvider.endLocomotion += i => DisableTeleportation();
            m_toolSelectionMenu.OnToggleMenu += i => m_menuCanvas.gameObject.SetActive(i);


            /*
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
            }

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
            }

            if (!m_disposalBag && m_disposalBagPrefab)
            {
                m_disposalBag = Instantiate(m_disposalBagPrefab);
                if (m_disposalBag.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    m_avatar.Toolbelt.CreateToolObject(interactable, m_avatar.Toolbelt.BackWaistAttachPoint, true);
                }
                m_disposalBag.Init();
            }*/

            /*
            if (m_scoopulaPrefab)
            {
                GameObject scoopulaObject = Instantiate(m_scoopulaPrefab);
                if (scoopulaObject.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    m_avatar.Toolbelt.CreateToolObject(interactable, m_avatar.Toolbelt.OtherAttachPoints[0], false);
                }
            }

            if (m_pipettePrefab)
            {
                GameObject pipetteObject = Instantiate(m_pipettePrefab);
                if (pipetteObject.TryGetComponent<XRGrabInteractable>(out XRGrabInteractable interactable))
                {
                    m_avatar.Toolbelt.CreateToolObject(interactable, m_avatar.Toolbelt.OtherAttachPoints[1], false);
                }
            }
            */
        }
        #endregion

        #region Update
        // Update is called once per frame
        protected override void Update()
        {
            // Check for user presence
            CheckForUserPresence();

            // Update the camera floor offset object if necessary
            UpdateCameraFloorOffset();

            // Call base functionality
            base.Update();
        }

        #endregion

        #region Input-Related Functionality
        protected void CheckForUserPresence()
        {
            // First, try to get a headset
            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            // Make sure device is valid
            if (!device.isValid)
            {
                m_userIsPresent = false;
                return;
            }

            // Check if this device has a user presence sensor
            bool userPresent = true;
            if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out bool featureValue))
                userPresent = featureValue;

            // Check if the headset was just put on this frame
            if (!m_userIsPresent && userPresent)
            {
                // Perform first frame logic
            }
            // Set user presence
            m_userIsPresent = userPresent;
        }

        protected void UpdateCameraFloorOffset()
        {
            if (m_xrRig && m_xrRig.CameraFloorOffsetObject)
            {
                var pos = m_xrRig.CameraFloorOffsetObject.transform.localPosition;
                var intendedOffset = SittingOffset + CameraYOffset;
                m_xrRig.CameraFloorOffsetObject.transform.localPosition = new Vector3(pos.x, intendedOffset, pos.z);
            }
            // Set the adjusted camera height
            AdjustedCameraHeight = CameraFloorOffset.localPosition.y + Camera.transform.localPosition.y * m_rigScale;
        }
        #endregion

        #region Movement-Related Functionality
        public override void SetMovementMode(MovementMode _newMode)
        {
            /*
            // Check for teleport cancel
            if (m_currentMovementMode == MovementMode.Teleport && m_isTeleporting)
                DisableTeleportation();
            // Set current movement mode
            m_currentMovementMode = _newMode;
            */

            // Call base functionality
            base.SetMovementMode(_newMode);
            // Check if this is teleportation
            bool isTeleport = (_newMode == MovementMode.Teleport);
            // Configure the movement providers
            m_avatar.MoveProvider.enabled = !isTeleport;
            m_avatar.TeleportProvider.enabled = isTeleport;
            // Set the left hand teleport mode
            //m_avatar.LeftHand.OnModeToggled(isTeleport);

        
        }
        #endregion

        #region UI-Related Functionality
        public override void InitToolSelectionMenu()
        {
            if (m_inventory.ToolInstances == null || m_inventory.ToolInstances.Count < 1) return;
            // Get available tools
            List<ToolType> availableToolTypes = new List<ToolType>();
            foreach(KeyValuePair<ToolType, ToolInstance> item in m_inventory.ToolInstances)
            {
                UnityEngine.Debug.Log($"{gameObject.name} InitToolSelectionMenu: {item.Key} || Time: {Time.time}");
                availableToolTypes.Add(item.Key);
            }
            // Initialize the tool selection items
            m_toolSelectionMenu.Init(availableToolTypes);
        }

        public override void ToggleToolSelectionMenu()
        {
            if (m_menuCanvas == null || m_toolSelectionMenu == null || CoreGameManager.Instance.CurrentState != GameState.InGame || m_inventory.ToolInstances.Count < 2) return;

            UnityEngine.Debug.Log($"Arrived in ToggleToolSelectionMenu: {m_menuCanvas.gameObject.activeInHierarchy} || Time: {Time.time}");
            // Open/close the tool selection menu
            m_toolSelectionMenu.ToggleMenu();
            /*
            if (!m_menuCanvas.gameObject.activeInHierarchy)
                m_menuCanvas.gameObject.SetActive(true);
            else
                m_menuCanvas.gameObject.SetActive(false);
            */
        }
        
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }
    }


}

