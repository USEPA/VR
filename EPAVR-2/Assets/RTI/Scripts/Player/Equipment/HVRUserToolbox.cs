using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [RequireComponent(typeof(HVRGrabbableContainer))]
    public class HVRUserToolbox : UserToolbox
    {
        #region Private Variables
        private HVRGrabbableContainer m_grabbableContainer;
        #endregion

        #region Initialization
        public override void Init()
        {
            // Initialize the grabbable container
            m_grabbableContainer.Init();
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }
    }
}

