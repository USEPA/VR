using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class EndScenarioDetailsDisplay : MonoBehaviour
    {
        #region Public Properties
        public abstract Gamemode Mode { get; }
        #endregion
        #region Initialization
        public virtual void Init(EndScenarioInfo _info)
        {
            // Do the thing
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

