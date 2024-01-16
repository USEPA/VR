using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class ParentTextDisplay : DoubleTextDisplay
    {
        #region Inspector Assigned Variables
        [SerializeField] protected RectTransform m_childContainer;
        [SerializeField] protected DoubleTextDisplay m_childPrefab;
        #endregion
        #region Protected Variables
        protected List<DoubleTextDisplay> m_children;
        #endregion

        #region Display-Related Functionality
        public void AddChild(string _mainText, string _secondaryText)
        {
            if (m_children == null) m_children = new List<DoubleTextDisplay>();
            DoubleTextDisplay child = Instantiate(m_childPrefab.gameObject, m_childContainer).GetComponent<DoubleTextDisplay>();
            child.SetText(_mainText, _secondaryText);
            m_children.Add(child);
        }
        #endregion
    }
}

