using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Scenarios/Radiation Survey")]
    public class RadiationSurveyScenarioAsset : ScenarioAsset
    {
        #region Public Properties
        public override Gamemode Mode => Gamemode.RadiationSurvey;
        #endregion

        public override ScenarioInstance CreateInstance()
        {
            return new RadSurveyScenarioInstance(this);
        }
    }

    [System.Flags]
    public enum RadType { Alpha = 0, Beta = 1, Gamma = 2 }
}

