using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class ChemCloudMapMarkerUIObject : MapMarkerUIObject
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Image m_fillImage;
        [SerializeField] private GameObject m_statusContainer;
        [SerializeField] private List<Image> m_statusImages;
        [Header("Other Configuration")]
        [SerializeField] Color m_missedSiteColor;
        [SerializeField] Color m_clearedSiteColor;
        #endregion
        #region Private Variables
        private RectTransform m_fillRectTransform;
        private Image m_currentStatusImage;

        private bool m_isCleared = false;
        #endregion
        #region Public Properties
        public override Image Image => m_fillImage;

        public RectTransform FillRectTransform
        {
            get
            {
                if (!m_fillRectTransform) m_fillRectTransform = m_fillImage.GetComponent<RectTransform>();
                return m_fillRectTransform;
            }
        }
        #endregion

        #region Initialization
        public override void Init(MapUI _parent, Vector3 _sourcePosition)
        {
            // Call base functionality
            base.Init(_parent, _sourcePosition);
        }

        public void LoadVaporCloudInfo(VaporCloud _cloud)
        {
            // Get radius
            float radius = _cloud.Radius;

            // Edit the image accordingly
            Vector3 defaultScale = FillRectTransform.localScale;
            FillRectTransform.localScale = new Vector3(defaultScale.x * radius, defaultScale.y * radius, defaultScale.z);

            // Disable all status images
            if (m_statusImages != null && m_statusImages.Count > 0)
            {
                foreach (Image statusImage in m_statusImages) statusImage.gameObject.SetActive(false);
                SetClearStatus(false);
            }

            // By default, disable status view
            SetStatusViewActive(false);
        }
        #endregion

        public override void SetScale(float _scaleFactor = 1)
        {
            FillRectTransform.localScale = new Vector3(_scaleFactor, _scaleFactor, RectTransform.localScale.z);
        }

        public override void SetColor(Color _color)
        {
            m_fillImage.color = _color;
        }

        public void SetStatusViewActive(bool _value)
        {
            m_statusContainer.gameObject.SetActive(_value);
        }

        public void SetClearStatus(bool _value)
        {
            m_isCleared = _value;
            if (m_currentStatusImage != null) m_currentStatusImage.gameObject.SetActive(false);

            m_currentStatusImage = (m_isCleared) ? m_statusImages[1] : m_statusImages[0];
            m_currentStatusImage.gameObject.SetActive(true);

            SetColor((m_isCleared) ? m_clearedSiteColor : m_missedSiteColor);
        }
    }
}

