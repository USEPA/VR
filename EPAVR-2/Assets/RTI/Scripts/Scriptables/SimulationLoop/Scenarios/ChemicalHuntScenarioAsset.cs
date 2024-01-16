using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Scenarios/Chemical Hunt Scenario")]
    public class ChemicalHuntScenarioAsset : ScenarioAsset
    {
        #region Public Properties
        public override Gamemode Mode => Gamemode.ChemicalHunt;
        #endregion

        public override ScenarioInstance CreateInstance()
        {
            return new ChemHuntScenarioInstance(this);
        }
    }
}

