using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class MenuState : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] protected UnityEvent m_onStateEnter;
        [SerializeField] protected UnityEvent m_onStateExit;
        #endregion
        #region Public Properties
        #endregion

        #region Enter/Exit-Related Functionality
        public virtual void OnStateEnter()
        {
            gameObject.SetActive(true);
            m_onStateEnter?.Invoke();
        }

        public virtual void OnStateUpdate(float _deltaTime)
        {
            // Do nothing
        }

        public virtual void OnStateExit()
        {
            m_onStateExit?.Invoke();
            gameObject.SetActive(false);
        }
        #endregion
    }
}

