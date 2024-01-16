using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class ScenarioObjective
    {
        #region Protected Variables
        protected bool m_isCompleted = false;

        protected ScenarioInstance m_parent;
        #endregion
        #region Public Properties
        public abstract ObjectiveType Type { get; }
        public bool IsCompleted 
        { 
            get => m_isCompleted; 
            protected set
            {
                m_isCompleted = value;
            }
        }
        
        public ScenarioInstance Parent { get => m_parent; }

        public abstract string ClearMessage { get; }
        public abstract ScoreType ScoreType { get; }
        public abstract int ScorePoints { get; }

        #endregion

        #region Initialization
        public ScenarioObjective(ScenarioInstance _parent)
        {
            // Cache parent reference
            m_parent = _parent;
            // Set isCompleted default
            m_isCompleted = false;

            
        }
        #endregion

        #region Clear-Functionality
        public virtual void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            if (m_parent != null) m_parent.CompleteObjective(this);
        }
        #endregion
    }

    public enum ObjectiveType {ClearSite, TutorialStep}
}

