using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace L58.EPAVR {
    [DisallowMultipleComponent]
    public class L58XRController : ActionBasedController {
        
        [SerializeField]
        InputActionProperty m_thumbstickAction;
        public InputActionProperty thumbstickAction
        {
            get => m_thumbstickAction;
            set => SetInputActionProperty(ref m_thumbstickAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_modeToggleAction;
        public InputActionProperty modeToggleAction {
            get => m_modeToggleAction;
            set => SetInputActionProperty(ref m_modeToggleAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_toolAction;
        public InputActionProperty toolAction {
            get => m_toolAction;
            set => SetInputActionProperty(ref m_toolAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_userDisplayAction;
        public InputActionProperty userDisplayAction {
            get => m_userDisplayAction;
            set => SetInputActionProperty(ref m_userDisplayAction, value);
        }

        InteractionState m_modeToggleInteractionState;
        public InteractionState modeToggleInteractionState => m_modeToggleInteractionState;
        
        InteractionState m_toolActionInteractionState;
        public InteractionState toolActionInteractionState => m_toolActionInteractionState;
        
        InteractionState        m_userDisplayInteractionState;
        public InteractionState userDisplayInteractionState => m_userDisplayInteractionState;

        Vector2 m_thumbstickPosition;
        public Vector2 thumbstickPosition => m_thumbstickPosition;

        protected override void Awake() {
            base.Awake();
            
            SetControllerState(new L58XRControllerState());
        }
        
        protected override void OnEnable() {
            base.OnEnable();
            m_modeToggleAction.EnableDirectAction();
        }
        
        protected override void OnDisable() {
            base.OnDisable();
            m_modeToggleAction.DisableDirectAction();
        }

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState) {
            base.ApplyControllerState(updatePhase, controllerState);

            var l58ControllerState = controllerState as L58XRControllerState;
            if (l58ControllerState == null)
                return;

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic) {
                m_thumbstickPosition          = l58ControllerState.thumbstick;
                m_modeToggleInteractionState  = l58ControllerState.modeToggleInteractionState;
                m_toolActionInteractionState  = l58ControllerState.toolActionInteractionState;
                m_userDisplayInteractionState = l58ControllerState.userDisplayInteractionState;
            }
        }

        protected override void UpdateInput(XRControllerState controllerState) {
            base.UpdateInput(controllerState);
            if (controllerState == null)
                return;

            var l58ControllerState = controllerState as L58XRControllerState;
            if (l58ControllerState == null)
                return;
            
            l58ControllerState.ResetL58FrameDependentStates();

            if (m_thumbstickAction.action != null) l58ControllerState.thumbstick = m_thumbstickAction.action.ReadValue<Vector2>();
            
            ComputeInteractionActionStates(IsPressed(m_modeToggleAction), ref l58ControllerState.modeToggleInteractionState);
            ComputeInteractionActionStates(IsPressed(m_toolAction), ref l58ControllerState.toolActionInteractionState);
            ComputeInteractionActionStates(IsPressed(m_userDisplayAction), ref l58ControllerState.userDisplayInteractionState);

            bool IsPressed(InputActionProperty property)
            {
                var action = property.action;
                if (action == null)
                    return false;

                return action.triggered || action.phase == InputActionPhase.Performed;
            }
        }
        
        #region Taken from ActionBasedController
        
        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value) {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }
        
        static void ComputeInteractionActionStates(bool pressed, ref InteractionState interactionState) {
            if (pressed) {
                if (!interactionState.active) {
                    interactionState.activatedThisFrame = true;
                    interactionState.active             = true;
                }
            } else {
                if (interactionState.active) {
                    interactionState.deactivatedThisFrame = true;
                    interactionState.active               = false;
                }
            }
        }

        static bool IsDisabledReferenceAction(InputActionProperty property) =>
            property.reference != null && property.reference.action != null && !property.reference.action.enabled;

        #endregion
    }
}