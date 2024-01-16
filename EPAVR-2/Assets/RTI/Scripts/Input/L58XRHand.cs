using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace L58.EPAVR {
    [DisallowMultipleComponent]
    public class L58XRHand : XRRayInteractor {
        #region Inspector Assigned Variables
        [SerializeField] GameObject m_handModel;
        #endregion
        private bool _isLeftHand;
        private bool _toggleTeleportMode;
        private bool _teleportSelected;
        private int  _teleportOctant;

        private LineRenderer           _lineRenderer;
        private XRInteractorLineVisual _lineVisual;
        private L58TeleportationArea   _teleportTarget;

        private L58XRAvatar _avatar;

        private readonly ActivateEventArgs _teleportActivateArgs = new ActivateEventArgs();

        private const float TELEPORT_SELECT_THRESHOLD  = 0.925f;
        private const float TELEPORT_TRIGGER_THRESHOLD = 0.125f;

        protected override void Start() {
            base.Start();

            xrController = GetComponent<L58XRController>();
            selectEntered.AddListener(i => HideHand(i.interactable));
            selectExited.AddListener(i => ShowHand(i.interactable));
        }

        public void InitializeHand(L58XRAvatar avatar, bool left) {
            _avatar = avatar;
            _isLeftHand = left;
        }

        public void OnModeToggled(bool teleportMode) {
            _toggleTeleportMode = teleportMode;

            if (!_toggleTeleportMode) OnTeleportInteractionStateChanged(false);
        }

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic) {
                var l58Controller = xrController as L58XRController;
                if (l58Controller == null)
                    return;

                if (l58Controller.selectInteractionState.activatedThisFrame) {
                    _avatar.OnGrabAction(selectTarget, _isLeftHand);
                }

                if (l58Controller.modeToggleInteractionState.activatedThisFrame) {
                    _toggleTeleportMode = !_toggleTeleportMode;
                    _avatar.OnModeToggled(_toggleTeleportMode, _isLeftHand);
                }

                if (l58Controller.toolActionInteractionState.activatedThisFrame) {
                    _avatar.OnToolAction(_isLeftHand);
                }

                if (l58Controller.userDisplayInteractionState.activatedThisFrame) {
                    _avatar.OnUserDisplayAction(_isLeftHand);
                }

                if (_toggleTeleportMode && !_teleportSelected && l58Controller.thumbstickPosition.y >= TELEPORT_SELECT_THRESHOLD) {
                    OnTeleportInteractionStateChanged(true);
                } else if (_toggleTeleportMode && _teleportSelected && _teleportTarget != null && l58Controller.thumbstickPosition.sqrMagnitude < TELEPORT_TRIGGER_THRESHOLD) {
                    OnTeleport();
                } else if (!_toggleTeleportMode) {
                    OnTeleportInteractionStateChanged(false);
                }

                if (_toggleTeleportMode && _teleportSelected) {
                    var thumbstick = l58Controller.thumbstickPosition;
                    var thumbstickAngle = Mathf.Atan2(thumbstick.y, thumbstick.x);
                    
                    var avatarOctant     = Mathf.RoundToInt(8 * _avatar.transform.eulerAngles.y / 360f + 8) % 8;
                    var thumbstickOctant = 8 - (Mathf.RoundToInt(8 * thumbstickAngle / (2 * Mathf.PI) + 8) % 8 + 6) % 8;

                    _teleportOctant = (avatarOctant + thumbstickOctant) % 8;
                }
            }
        }

        private void OnTeleport() {
            _teleportActivateArgs.interactor   = this;
            _teleportActivateArgs.interactable = _teleportTarget;
            _teleportTarget.Activate(_teleportActivateArgs, _teleportOctant);
            OnTeleportInteractionStateChanged(false);
        }
        
        private void OnTeleportInteractionStateChanged(bool enabled) {
            _teleportSelected = enabled;
            
            lineType = enabled ? LineType.ProjectileCurve : LineType.StraightLine;
        }

        #region Merged from XRDirectInteractor and XRRayInteractor
        protected override void Awake() {
            base.Awake();

            _lineRenderer = GetComponent<LineRenderer>();
            _lineVisual   = GetComponent<XRInteractorLineVisual>();

            OnTeleportInteractionStateChanged(false);

            //XRDirectInteractor
            m_InteractableSortComparison = InteractableSortComparison;
            if (!GetComponents<Collider>().Any(x => x.isTrigger)) Debug.LogWarning("XR Hand does not have required Collider set as a trigger.", this);
        }
        
        public override void GetValidTargets(List<XRBaseInteractable> targets) {
            base.GetValidTargets(targets);

            if (targets.Count > 0) _teleportTarget = targets[0] as L58TeleportationArea;

            //XRDirectInteractor
            m_InteractableDistanceSqrMap.Clear();
            
            foreach (var interactable in m_ValidGrabTargets) {
                m_InteractableDistanceSqrMap[interactable] = interactable.GetDistanceSqrToInteractor(this);
                targets.Add(interactable);
            }

            //targets.Sort(m_InteractableSortComparison);
        }
        #endregion

        #region From XRDirectInteractor
        
        readonly  List<XRBaseInteractable> m_ValidGrabTargets = new List<XRBaseInteractable>();

        readonly Dictionary<XRBaseInteractable, float> m_InteractableDistanceSqrMap = new Dictionary<XRBaseInteractable, float>();
        Comparison<XRBaseInteractable> m_InteractableSortComparison;

        protected void OnTriggerEnter(Collider other) {
            if (interactionManager == null) return;

            var interactable = interactionManager.GetInteractableForCollider(other);
            if (interactable != null && !m_ValidGrabTargets.Contains(interactable)) m_ValidGrabTargets.Add(interactable);
        }

        protected void OnTriggerExit(Collider other) {
            if (interactionManager == null) return;

            var interactable = interactionManager.GetInteractableForCollider(other);
            if (interactable != null) m_ValidGrabTargets.Remove(interactable);
        }

        protected override void OnRegistered(InteractorRegisteredEventArgs args) {
            base.OnRegistered(args);
            args.manager.interactableUnregistered += OnInteractableUnregistered;
        }
        
        protected override void OnUnregistered(InteractorUnregisteredEventArgs args) {
            base.OnUnregistered(args);
            args.manager.interactableUnregistered -= OnInteractableUnregistered;
        }

        void OnInteractableUnregistered(InteractableUnregisteredEventArgs args) {
            m_ValidGrabTargets.Remove(args.interactable);
        }

        int InteractableSortComparison(XRBaseInteractable x, XRBaseInteractable y) {
            var xDistance = m_InteractableDistanceSqrMap[x];
            var yDistance = m_InteractableDistanceSqrMap[y];
            if (xDistance > yDistance)
                return 1;
            if (xDistance < yDistance)
                return -1;

            return 0;
        }

        #endregion

        #region Hand Functionality
        public void HideHand(XRBaseInteractable _interactable)
        {
            m_handModel.SetActive(false);
        }

        public void ShowHand(XRBaseInteractable _interactable)
        {
            m_handModel.SetActive(true);
        }
        #endregion
    }
}

