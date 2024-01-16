using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Gamemodes/Chemical Hunt")]
    public class ChemicalHuntGMConfigAsset : GamemodeConfigAsset
    {
        #region Public Properties
        public override Gamemode Mode => Gamemode.ChemicalHunt;
        #endregion
    }
}
