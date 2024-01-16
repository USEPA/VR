using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class RadAgent : Agent
    {
        #region Inspector Assigned Variables
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.RadiationSurvey;
        
        public abstract RadType Type { get; }
        public override string Name => Type.ToString();

        public override string Abbreviation
        {
            get => $"{Name[0]}";
        }
        #endregion
    }
}

