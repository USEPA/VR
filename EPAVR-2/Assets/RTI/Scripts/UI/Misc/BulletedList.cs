using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class BulletedList : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected BulletedListItem m_listItemPrefab;
        [Header("Default Configuration")]
        [SerializeField] protected bool m_forceChildExpand = true;
        [SerializeField] protected int m_indentation = 1;
        #endregion
        #region Protected Variables
        protected List<BulletedListItem> m_items;
        protected RectTransform m_rectTransform;
        protected VerticalLayoutGroup m_verticalLayoutGroup;
        #endregion
        #region Public Properties
        public RectTransform RectTransform
        {
            get
            {
                if (!m_rectTransform) m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public VerticalLayoutGroup VerticalLayoutGroup
        {
            get
            {
                if (!m_verticalLayoutGroup) m_verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
                return m_verticalLayoutGroup;
            }
        }
        #endregion


        #region List-Related Functionality
        public void AddItem(string _text)
        {
            // Initialize list if necessary
            if (m_items == null) m_items = new List<BulletedListItem>();
            // Instantiate a new bulleted list item
            BulletedListItem item = Instantiate(m_listItemPrefab, RectTransform);
            // Check if the child should be force expanded
            if (m_forceChildExpand) ForceChildExpandWidth(item);
            item.Init(_text, m_indentation);
        }
        #endregion

        #region Helper Methods
        void ForceChildExpandWidth(BulletedListItem _item)
        {
            if (_item.TryGetComponent<RectTransform>(out RectTransform childRectTransform))
            {
                childRectTransform.pivot = new Vector2(0, 1);
                childRectTransform.sizeDelta = new Vector2(RectTransform.rect.size.x - (VerticalLayoutGroup.padding.left + VerticalLayoutGroup.padding.right), childRectTransform.sizeDelta.y);
            }
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
    }
}

