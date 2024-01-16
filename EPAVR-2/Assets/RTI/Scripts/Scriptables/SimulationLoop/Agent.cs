using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class Agent : ScriptableObject
    {
        #region Inspector Assigned Variables
        #endregion
        #region Public Properties
        public abstract Gamemode Mode { get; }

        public abstract string Name { get; }
        public virtual string Abbreviation { get; } = "";
        #endregion
    }
}

