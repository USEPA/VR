using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class L58XRControllerState : XRControllerState {

    public Vector2          thumbstick;
    public InteractionState modeToggleInteractionState;
    public InteractionState toolActionInteractionState;
    public InteractionState userDisplayInteractionState;

    public void ResetL58FrameDependentStates() {
        modeToggleInteractionState.ResetFrameDependent();
        toolActionInteractionState.ResetFrameDependent();
        userDisplayInteractionState.ResetFrameDependent();
    }
}
