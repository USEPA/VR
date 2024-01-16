using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    public class TestDraggableItem : MonoBehaviour, IDebugDraggable
    {
        #region Inspector Assigned Variables
        [Header("Events")]
        [SerializeField] UnityEvent m_onDragStart;
        [SerializeField] UnityEvent m_onDragEnd;
        #endregion
        #region Protected Variables
        protected Rigidbody m_rigidbody;
        #endregion
        #region Public Properties
        public Rigidbody Rigidbody { get => m_rigidbody; }

        public virtual bool ParentToHand { get => true; }
        #endregion

        void Awake()
        {
            TryGetComponent<Rigidbody>(out m_rigidbody);
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void OnDragStart()
        {
            m_onDragStart?.Invoke();
        }

        public virtual void OnDragEnd()
        {
            m_onDragEnd?.Invoke();
        }
    }
}

