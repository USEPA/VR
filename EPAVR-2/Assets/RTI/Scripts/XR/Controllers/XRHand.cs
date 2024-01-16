using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using HurricaneVR.Framework.Components;

namespace L58.EPAVR
{
    public class XRHand : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected XRDirectInteractor m_directInteractor;
        [SerializeField] protected XRRayInteractor m_rayInteractor;
        [SerializeField] protected GameObject m_handModel;
        [SerializeField] protected Transform m_tooltipSpawnPoint;
        [SerializeField] protected HVRGrabbableHoverBase m_grabIndicator;
        [SerializeField] protected UnityEngine.InputSystem.InputActionReference m_hapticAction;
        [Header("Default Configuration")]
        [SerializeField] protected Gradient m_defaultRayGradient;
        [SerializeField] protected Gradient m_teleportRayGradient;
        [SerializeField] protected bool m_grabIndicatorEnabled = false;


   
        #endregion
        #region Protected Variables
        protected L58XRAvatar m_avatar;
        protected L58XRController m_controller;

        protected bool m_isLeft = false;
        protected bool m_toggleTeleportMode;

        protected int m_defaultLayerMask;
        protected int m_teleportLayerMask;

        protected Animator m_handAnimator;
        protected SkinnedMeshRenderer m_handMesh;
        protected Material m_defaultMaterial;
        protected XRInteractorLineVisual m_lineVisual;

        protected SafetyGlove m_equippedGlove;

        protected XRTooltip m_currentTooltip;
        protected UnityEngine.InputSystem.XR.XRController m_inputController;

        protected XRBaseInteractable m_currentHoveringInteractable;
        protected Transform m_playerCameraTransform;
        protected bool m_isHovering = false;
        #endregion
        #region Public Properties
        public bool IsLeft { get => m_isLeft; }

        public L58XRController Controller { get => m_controller; }
        public XRDirectInteractor DirectInteractor { get => m_directInteractor; }
        public XRRayInteractor RayInteractor { get => m_rayInteractor; }

        public GameObject Model { get => m_handModel; }
        public SkinnedMeshRenderer Mesh { get => m_handMesh; }
        public Material DefaultMaterial { get => m_defaultMaterial; }

        public SafetyGlove EquippedGlove { get => m_equippedGlove; }
        public XRTooltip CurrentTooltip { get => m_currentTooltip; }
        public Transform TooltipSpawnPoint { get => m_tooltipSpawnPoint; }

        public bool IsHovering { get => m_isHovering; }
        #endregion

        #region Initialization
        public void Init(L58XRAvatar _avatar, bool _isLeft)
        {
            // Cache references
            m_avatar = _avatar;
            m_playerCameraTransform = _avatar.VRCamera.transform;
            m_controller = GetComponent<L58XRController>();
            m_lineVisual = m_rayInteractor.GetComponent<XRInteractorLineVisual>();
            m_handAnimator = m_handModel.GetComponent<Animator>();
            SkinnedMeshRenderer handMesh = m_handModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (handMesh != null) 
            {
                m_handMesh = handMesh;
                m_defaultMaterial = m_handMesh.material;
            }
     
            // Configure handedness
            m_isLeft = _isLeft;
            m_inputController = ((m_isLeft) ? UnityEngine.InputSystem.XR.XRController.leftHand : UnityEngine.InputSystem.XR.XRController.rightHand);
            /*
            var device = UnityEngine.InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(((!m_isLeft) ? CommonUsages.RightHand : CommonUsages.LeftHand)));
            UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse()
            */
            // Get teleport layer masks
            m_defaultLayerMask = LayerMask.GetMask("UI");
            m_teleportLayerMask = LayerMask.GetMask("UI", "Teleport");
            // Hook up events
            m_controller.selectAction.action.started += i => SetHandGripAnimation(true);
            m_controller.selectAction.action.canceled += i => SetHandGripAnimation(false);
            m_directInteractor.selectEntered.AddListener(i => HideHand(i.interactable));
            m_directInteractor.selectExited.AddListener(i => ShowHand(i.interactable));
            // By default, disable teleport mode
            OnModeToggled(false);
        }
        #endregion

        #region Update
        // Update is called once per frame
        void Update()
        {
            //if (!m_directInteractor) return;
            if (!m_isHovering || !m_grabIndicatorEnabled) return;
            UpdateGrabIndicator();
        }
        #endregion

        #region Interaction-Related Functionality
        public void OnSelectEnter(SelectEnterEventArgs args)
        {
            // Get the interactable
            if (args.interactable is XRGrabInteractable interactable)
            {
                // Check if the object is contaminated
                if (interactable.TryGetComponent<ISampleContainer>(out ISampleContainer sampleContainer))
                {
                    // Check if there is a contaminant that could be applied to the glove
                    if (m_equippedGlove != null)
                    {
                        // Check if there is a sample that can contaminate the hand
                        if (sampleContainer.Properties.CanContaminateHand && sampleContainer.CurrentSample != null) m_equippedGlove.AddContaminant(sampleContainer.CurrentSample);
                        // Check if cross-contamination should be applied
                        if (m_equippedGlove.Contaminants != null && m_equippedGlove.Contaminants.Count > 0)
                        {
                            if (sampleContainer.CurrentSample != null)
                            {
                                foreach(ChemicalAgent chemical in m_equippedGlove.Contaminants)
                                {
                                    if (!sampleContainer.CurrentSample.IsCrossContaminated && sampleContainer.CurrentSample.Chemical != null && sampleContainer.CurrentSample.Chemical != chemical)
                                    {
                                        sampleContainer.CurrentSample.IsCrossContaminated = true;
                                        UnityEngine.Debug.Log($"{interactable.gameObject.name} was cross-contaminated by {gameObject.name}: {chemical} || Time: {Time.time}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // There is no current glove, so add a penalty
                        if (sampleContainer.Properties.CanContaminateHand && sampleContainer.CurrentSample != null) 
                            ScoreManager.Instance.AddUnsafeChemicalContactPenalty();
                    }
                }
            }
        }

        public void OnHoverEnter(HoverEnterEventArgs args)
        {
            
            if (!m_controller || args.interactable == null) return;
            m_isHovering = true;
            m_currentHoveringInteractable = args.interactable;
            EnableGrabIndicator();
            //var command = UnityEngine.InputSystem.XR.Haptics.SendHapticImpulseCommand.Create(0, 0.2f, 0.2f);
            XRBaseInteractable interactable = args.interactable;
            // Get distance from hand to origin
            float distance = MathHelper.QuickDistance(transform.position, interactable.transform.position);
            if (distance < 0.1f)
            {
                SendHapticImpulse(0.025f, 0.1f);
                //UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(m_hapticAction, 0.025f, 0.1f, m_inputController);
            }

            //m_controller.SendHapticImpulse(0.2f, 0.2f);
        }

        public void OnHoverExit(HoverExitEventArgs args)
        {
            m_isHovering = false;
            m_currentHoveringInteractable = null;
            DisableGrabIndicator();
        }
        #endregion
        #region Teleport-Related Functionality
        public void OnModeToggled(bool teleportMode)
        {
            m_toggleTeleportMode = teleportMode;
            m_rayInteractor.interactionLayerMask = (teleportMode) ? m_teleportLayerMask : m_defaultLayerMask;
            m_rayInteractor.raycastMask = (teleportMode) ? m_teleportLayerMask : m_defaultLayerMask;
            m_rayInteractor.lineType = (teleportMode) ? XRRayInteractor.LineType.ProjectileCurve : XRRayInteractor.LineType.StraightLine;
            m_lineVisual.invalidColorGradient = (teleportMode) ? m_teleportRayGradient : m_defaultRayGradient;

            /*
            if (teleportMode)
            {
               
            }
            else
            {

            }
            */
            //if (!_toggleTeleportMode) OnTeleportInteractionStateChanged(false);
        }

        public void GetTeleportTarget()
        {

        }

        public void AttemptTeleport()
        {
            // Get any current interactables from the ray interactor
            //XRBaseInteractable interactable = m_rayInteractor.
        }
        #endregion
        #region Hand-Related Functionality
        public void HideHand(XRBaseInteractable _interactable)
        {
            m_handModel.SetActive(false);
            m_rayInteractor.gameObject.SetActive(false);
        }

        public void ShowHand(XRBaseInteractable _interactable)
        {
            m_handModel.SetActive(true);
            m_rayInteractor.gameObject.SetActive(true);
        }

        public void SetHandGripAnimation(bool _gripped)
        {
            m_handAnimator?.SetBool("isGripped", _gripped);
        }

        public void SetMaterial(Material _material)
        {
            // Make sure the mesh exists
            if (!m_handMesh) return;
            m_handMesh.material = _material;
            UnityEngine.Debug.Log($"{gameObject.name} arrived in SetMaterial: {_material.name} || Time: {Time.time}");
        }

        public void ResetMaterial()
        {
            // Make sure the mesh exists
            if (!m_handMesh) return;
            m_handMesh.material = m_defaultMaterial;
        }
        #endregion

        #region Glove-Related Functionality
        public void ApplyGlove(SafetyGlove _glove)
        {
            // Make sure no glove is already applied
            if (m_equippedGlove != null) return;
            UnityEngine.Debug.Log($"{gameObject.name} applied glove || Time: {Time.time}");
            // Set reference
            m_equippedGlove = _glove;
            // Initialize the glove
            m_equippedGlove.Init(this);
        }

        public void RemoveGlove()
        {
            // Make sure there is a glove applied
            if (!m_equippedGlove) return;
            UnityEngine.Debug.Log($"{gameObject.name} removed glove || Time: {Time.time}");
            // Clear references for the glove
            m_equippedGlove.ClearRefs();
            m_equippedGlove = null;
            // Reset the hand's material
            ResetMaterial();
        }

        public void TryAttachGlove(XRToolbeltItem _potentialGlove)
        {
            if (_potentialGlove.TryGetComponent<SafetyGlove>(out SafetyGlove glove))
            {
                // Try to attach the glove
                ApplyGlove(glove);
            }
        }
        #endregion

        #region Grab-Related Functionality
        private void UpdateGrabIndicator()
        {
            if (!m_grabIndicator || !m_currentHoveringInteractable) return;
            if (m_grabIndicator.LookAtCamera) m_grabIndicator.transform.LookAt(m_playerCameraTransform);

            Vector3 grabPoint = transform.position;
            switch (m_grabIndicator.HoverPosition)
            {
                case HVRHoverPosition.Self:
                    return;
                case HVRHoverPosition.GrabPoint:
                    grabPoint = m_currentHoveringInteractable.transform.position;
                    break;
                default:
                    break;
            }
            
            m_grabIndicator.transform.position = grabPoint;

        }
        private void EnableGrabIndicator()
        {
            if (m_grabIndicatorEnabled) return;
            if (m_grabIndicator)
            {
                m_grabIndicatorEnabled = true;
                m_grabIndicator.Enable();
                m_grabIndicator.Hover();
            }
        }

        private void DisableGrabIndicator()
        {
            if (!m_grabIndicatorEnabled) return;
            if (m_grabIndicator)
            {
                m_grabIndicatorEnabled = false;
                m_grabIndicator.Unhover();
                m_grabIndicator.Disable();
            }
        }
        #endregion

        #region Tooltip-Related Functionality
        public void SetTooltip(XRTooltip _newTooltip, string _message)
        {
            // Cache reference
            m_currentTooltip = _newTooltip;
            m_currentTooltip.SetText(_message);

            Destroy(m_currentTooltip.gameObject, 2.0f);
        }
        #endregion

        #region Helper Methods
        public void SendHapticImpulse(float _amplitude, float _duration = 0.1f)
        {
            UnityEngine.XR.OpenXR.Input.OpenXRInput.SendHapticImpulse(m_hapticAction, _amplitude, _duration, m_inputController);
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }


    }
}

