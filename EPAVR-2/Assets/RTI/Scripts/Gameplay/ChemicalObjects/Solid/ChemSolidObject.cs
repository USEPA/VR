using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class ChemSolidObject : ChemicalObject
    {
        #region Public Properties
        public override MatterType Type => MatterType.Solid;
        #endregion
    }
}

