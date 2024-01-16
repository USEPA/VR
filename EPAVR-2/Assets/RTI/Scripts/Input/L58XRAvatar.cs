using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace L58.EPAVR 
{
    public class L58XRAvatar : Singleton<L58XRAvatar>
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private XRHand leftHand  = default;
        [SerializeField] private XRHand rightHand = default;
        [SerializeField] private XRToolbelt toolbelt = default;
        [SerializeField] private AvatarIK ikAvatar = default;
        [SerializeField] private XRSocket m_backAttachPoint;
        [SerializeField] private Transform m_cameraScale;
        [SerializeField] private Transform m_watchAttachPoint;
        [Header("Debug Values")]
        [SerializeField] private float m_debugHeadOffset = 1.9f;
        [SerializeField] private float m_debugToolbeltOffset = -0.7f;
        #endregion
        #region Private Variables
        private bool _teleportMode;

        private PlayerController m_parent;
        private XROrigin m_rig;

        private ContinuousMoveProviderBase _moveProvider;
        private SnapTurnProviderBase       _turnProvider;
        private TeleportationProvider      _teleportProvider;

        private Camera m_vrCamera;
        
        #endregion
        #region Public Properties
        public XRToolbelt Toolbelt { get => toolbelt; }
        public XROrigin Rig { get => m_rig; }
        public AvatarIK IK { get => ikAvatar; }

        public XRHand LeftHand { get => leftHand; }
        public XRHand RightHand { get => rightHand; }

        public XRSocket BackAttachPoint { get => m_backAttachPoint; }
        public ContinuousMoveProviderBase MoveProvider { get => _moveProvider; }
        public TeleportationProvider TeleportProvider { get => _teleportProvider; }
        public SnapTurnProviderBase TurnProvider { get => _turnProvider; }
        public Camera VRCamera { get => m_vrCamera; }

        public Transform CameraScale { get => m_cameraScale; }

        public Transform WatchAttachPoint { get => m_watchAttachPoint; }
        #endregion

        #region Initialization
        public void Init(PlayerController _parent)
        {
            // Cache references
            m_parent = _parent;
            m_rig = GetComponent<XROrigin>();
            m_vrCamera = m_rig.Camera;

            _moveProvider = GetComponent<ContinuousMoveProviderBase>();
            _turnProvider = GetComponent<SnapTurnProviderBase>();
            _teleportProvider = GetComponent<TeleportationProvider>();

            _teleportProvider.enabled = false;

            // Initialize hands
            leftHand.Init(this, true);
            rightHand.Init(this, false);

            //leftHand.Controller.modeToggleAction.action.performed += i => VRUserManager.Instance?.Player.ToggleMovementMode();
            
            // Check if there is a debug manager active
            if (DebugManager.Instance && VRUserManager.Instance.IsNonVRMode)
            {
                transform.GetChild(0).transform.localPosition = new Vector3(transform.GetChild(0).transform.localPosition.x, m_debugHeadOffset, transform.GetChild(0).transform.localPosition.z);
                //toolbelt.transform.localPosition = new Vector3(toolbelt.transform.localPosition.x, m_debugToolbeltOffset, toolbelt.transform.localPosition.z);
            }
            
            /*
            // Unparent the toolbelt
            toolbelt.transform.parent = m_parent.transform;
            toolbelt.transform.localPosition = Vector3.zero;
            toolbelt.transform.localEulerAngles = Vector3.zero;
            */
            /*
            if (ikAvatar != null)
            {
                ikAvatar?.Init();
                toolbelt.FollowTarget = ikAvatar.Hip;
                //ikAvatar?.InitMesh();
                //toolbelt.transform.parent = ikAvatar.Hip;
                //toolbelt.transform.localPosition = Vector3.zero;
            }
            */
        }
        

        
        void Start() 
        {
            //Init();
        }
        #endregion

        public void ScaleAvatarToHeight(float _height)
        {
            // Make sure the IK avatar exists
            if (!ikAvatar) return;
            // Tell the IK avatar to scale according to this height
            ikAvatar.ScaleAvatarToHeight(_height);
        }

        public void OnModeToggled(bool teleportMode, bool left) {
            _teleportMode = !_teleportMode;

            _moveProvider.enabled     = !_teleportMode;
            _turnProvider.enabled     = !_teleportMode;
            _teleportProvider.enabled = _teleportMode;

            if (left) {
                rightHand.OnModeToggled(teleportMode);
            } else {
                leftHand.OnModeToggled(teleportMode);
            }
        }

        public void OnGrabAction(XRBaseInteractable interactable, bool left) {
            
        }

        public void OnToolAction(bool left) {
            Debug.Log($"{Time.time} Tool Action Pressed {(left ? "Left" : "Right")}");
        }

        public void OnUserDisplayAction(bool left) {
            Debug.Log($"{Time.time} User Display Pressed {(left ? "Left" : "Right")}");
        }

        public void SetDesktopDebuggingPositions()
        {

        }

        #region Helper Methods
        public void SendHapticImpulse(bool _isLeftHand, float _amplitude, float _duration = 0.1f)
        {
            XRHand targetHand = (_isLeftHand) ? leftHand : rightHand;
            targetHand.SendHapticImpulse(_amplitude, _duration);
        }
        #endregion

        #region Reset Functionality
        public void ClearGloves()
        {
            // Clear gloves if necessary
            ClearGlove(leftHand);
            ClearGlove(rightHand);
        }
        
        void ClearGlove(XRHand _hand)
        {
            if (_hand.EquippedGlove != null)
            {
                SafetyGlove glove = _hand.EquippedGlove;
                _hand.RemoveGlove();
                Destroy(glove.gameObject);
            }
        }
        #endregion
    }
}
