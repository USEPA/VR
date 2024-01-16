using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public abstract class ContaminationSite : MonoBehaviour
    {
        #region Protected Variables
        protected SiteStatus m_status = SiteStatus.Idle;

        protected bool m_firstContacted = false;
        protected bool m_isCleared = false;
        protected Action<SiteStatus> m_onStateChange;
        protected Action<Contamination, Sample> m_onSiteCleared;
        protected UnityEvent m_onCleared;
        #endregion
        #region Public Properties
        public abstract Gamemode Gamemode { get; }

        public SiteStatus Status { get => m_status; }

        public bool FirstContacted { get => m_firstContacted; }
        public bool IsCleared { get => m_isCleared; }

        public Action<SiteStatus> OnStateChange { get => m_onStateChange; set => m_onStateChange = value; }
        public UnityEvent OnCleared 
        { 
            get 
            {
                if (m_onCleared == null) m_onCleared = new UnityEvent();
                return m_onCleared;
            }
            set => m_onCleared = value; 
        }

        #endregion

        #region Initialization
        public virtual void Init(Agent _agent)
        {
            // Do the thing
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion


        #region State-Related Functionality
        public virtual void SetState(SiteStatus _status)
        {
            // Set state
            m_status = _status;
            switch (_status)
            {
                case SiteStatus.Contacted:
                    if (!m_firstContacted)
                    {
                        m_firstContacted = true;
                    }
                    break;
                case SiteStatus.Completed:
                    if (!m_isCleared)
                    {
                        m_isCleared = true;
                    }
                    break;
                default:
                    break;
            }
            // Invoke any necessary events
            m_onStateChange?.Invoke(_status);
        }

        public virtual void FirstContact()
        {
            if (m_firstContacted) return;

            SetState(SiteStatus.Contacted);

        }
        #endregion

        #region Clear-Related Functionality
        #endregion
    }
}

