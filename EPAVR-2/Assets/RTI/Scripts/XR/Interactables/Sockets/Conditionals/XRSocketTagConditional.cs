using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class XRSocketTagConditional : XRSocketConditional
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] [TagSelector]
        private string m_tag = "";
        #endregion

        public override bool ItemIsValid(XRToolbeltItem _item)
        {
            return (_item.CompareTag(m_tag));
        }
    }
}

