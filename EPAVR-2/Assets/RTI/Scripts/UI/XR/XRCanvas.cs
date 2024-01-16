using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class XRCanvas : MonoBehaviour
    {
        #region Protected Variables
        protected Canvas m_canvas;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache canvas component
            m_canvas = GetComponent<Canvas>();
            if (!VRUserManager.Instance) return;
            // Assign the event camera of the canvas
            m_canvas.worldCamera = VRUserManager.Instance.Avatar.VRCamera;
        }
    }
}

