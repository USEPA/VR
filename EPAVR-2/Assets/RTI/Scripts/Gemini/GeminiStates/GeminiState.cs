using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class GeminiState : MonoBehaviour
    {
        #region Protected Variables
        protected Gemini m_parent;
        #endregion
        #region Public Properties
        public abstract GModeType Type { get; }
        public abstract string Title { get; }
        public Gemini Parent { get => m_parent; }
        #endregion

        #region Initialization
        public virtual void Init(Gemini _parent)
        {
            // Cache parent reference
            m_parent = _parent;
        }
        #endregion
        #region State-Related Functionality
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
        #endregion
    }
}

