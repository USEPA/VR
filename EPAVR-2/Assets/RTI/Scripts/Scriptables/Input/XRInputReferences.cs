using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "XR Action Map References")]
    public class XRInputReferences : ScriptableObject
    {
        #region Inspector Assigned Variables
        [SerializeField] InputActionAsset m_inputActionAsset;
        [SerializeField] List<XRActionMapReference> m_actionMaps;
        #endregion
        #region Public Properties
        public InputActionAsset ActionAsset { get => m_inputActionAsset; }
        #endregion

        #region Input-Reading Functionality
        /// <summary>
        /// Retrieves a specified XRActionMapReference from this asset's stored list
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public XRActionMapReference GetActionMap(XRActionMap map)
        {
            return m_actionMaps.Find(x => x.ActionMap == map);
        }
        /// <summary>
        /// Retrieves an InputActionReference from a specified action map
        /// This can be used to bind input events at runtime, provided that the InputActionAsset referenced in this class is enabled in-game
        /// </summary>
        /// <param name="map"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public InputActionReference GetAction(XRActionMap map, XRAction action)
        {
            // Find the action map and make sure it is valid
            var mapResult = GetActionMap(map);
            if (mapResult != null)
            {
                // Try to find an actual valid action reference that matches input
                var actionContainer = mapResult.Actions.Find(y => y.Action == action);
                if (actionContainer != null) return actionContainer.Reference;
            }
            return null;
        }
        #endregion
    }

    public enum XRActionMap { RightHand, LeftHand, UI, Advisor}

    public enum XRAction
    {
        Position, Rotation,
        GripPressed, Grip,
        TriggerPressed, Trigger,
        PrimaryButton, SecondaryButton,
        JoystickClick, Turn, Move, 
        AimPosition, AimRotation,
        UIClick
    }
}

