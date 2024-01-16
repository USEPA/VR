using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Scenarios/Tutorial Scenario")]
    public class TutorialScenarioAsset : ScenarioAsset
    {
        #region Public Properties
        public override Gamemode Mode => Gamemode.Tutorial;
        #endregion
    }
}

