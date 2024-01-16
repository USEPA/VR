using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class ChemLiquidObject : ChemicalObject
    {
        #region Public Properties
        public override MatterType Type => MatterType.Liquid;
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

