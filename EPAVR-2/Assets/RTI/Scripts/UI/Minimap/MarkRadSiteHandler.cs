using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace L58.EPAVR
{
    public class MarkRadSiteHandler : MonoBehaviour, IPointerClickHandler
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private RadMapUI m_mapUI;
        #endregion
        #region Private Variables
        private bool m_isActive = false;

        private Image m_image;
        private RectTransform m_rectTransform;
        private GraphicRaycaster m_graphicRaycaster;

        public Vector2 m_pointerPosition;
        public Vector2 m_pointerPositionInRectTransform;
        #endregion
        #region Public Properties
        public bool Active
        {
            get => m_isActive;
            set
            {
                m_isActive = value;
            }
        }
        public RectTransform RectTransform
        {
            get
            {
                if (!m_rectTransform) m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (!m_graphicRaycaster) m_graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
                return m_graphicRaycaster;
            }
        }
        #endregion

        void OnEnable()
        {
            Active = true;
        }

        void OnDisable()
        {
            Active = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Active) return;
            // Get point of click
            m_pointerPosition = eventData.position;
            Vector2 rectPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, GraphicRaycaster.eventCamera, out rectPosition))
            {
                m_pointerPositionInRectTransform = rectPosition;

                UnityEngine.Debug.Log($"{gameObject.name} - pointer clicked | Pointer Position in Rect: {rectPosition} || Time: {Time.time}");
                m_mapUI?.MarkSitePosition(rectPosition);
            }
        }
    }

}

