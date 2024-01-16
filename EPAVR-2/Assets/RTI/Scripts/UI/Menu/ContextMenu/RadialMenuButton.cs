using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class RadialMenuButton : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] protected Button m_button;
        [SerializeField] protected Image m_icon;
        #endregion
        #region Protected Variables
        protected RadialMenu m_parent;
        protected int m_index;

        protected RectTransform m_rectTransform;

        protected float m_angleOffset;

        protected Action m_onClick;
        #endregion
        #region Public Properties
        public RadialMenu Parent { get => m_parent; }
        public int Index { get => m_index; }

        public Button Button { get => m_button; }
        public Image Icon { get => m_icon; }

        public Action OnClick { get => m_onClick; set => m_onClick = value; }
        #endregion

        #region Initialization
        public void Init(RadialMenu _parent, int _index, float _offset)
        {
            // Cache components
            m_rectTransform = GetComponent<RectTransform>();
            m_button.gameObject.name = $"{gameObject.name}_Button";
            // Set parent and index
            m_parent = _parent;
            m_index = _index;
            // Set angle offset
            m_angleOffset = _offset;

            // Ensure rotation is correct
            m_rectTransform.rotation = Quaternion.Euler(0, 0, -m_angleOffset);
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Event-Related Functionality
        public void OnSelected()
        {
            UnityEngine.Debug.Log($"{gameObject.name} clicked || Time: {Time.time}");
            m_onClick?.Invoke();
        }
        #endregion
    }
}

