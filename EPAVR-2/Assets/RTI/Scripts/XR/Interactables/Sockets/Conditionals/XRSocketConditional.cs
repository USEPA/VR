using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class XRSocketConditional : MonoBehaviour
    {
        #region Protected Variables
        protected XRSocket m_parent;
        #endregion

        public virtual void Init(XRSocket _parent)
        {
            // Cache parent reference
            m_parent = _parent;
        }

        public abstract bool ItemIsValid(XRToolbeltItem _item);


    }
}

