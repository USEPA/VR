using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadSiteMapMarker : MapMarkerObject
    {
        #region Inspector Assigned Variables
        [SerializeField] protected SpriteRenderer m_markerSprite;
        #endregion
        #region Protected Variables
        private RadCloud m_radCloud;
        #endregion
        #region Public Properties
        #endregion

        public override void Init(Transform _parent, Color _color, bool _isDynamic = false)
        {
            // Call base functionality
            base.Init(_parent, _color, _isDynamic);
            // Try to get a vapor cloud component from the parent
            if (m_parent.TryGetComponent<RadCloud>(out m_radCloud))
            {
                /*
                // Scale the object according to the size of the vapor cloud
                float scale = m_radCloud.Radius * 2.0f;
                transform.localScale = Vector3.one * scale;
                */
                m_radCloud.MapMarker = this;
                if (m_radCloud.Parent != null)
                {
                    m_radCloud.Parent.OnCleared.AddListener(SetActive);
                    //m_radCloud.Parent.OnSiteCleared += (i, j) => MapManager.Instance.RemoveMarker(this);
                }
            }
        }

        protected override void SetColor(Color _color)
        {
            m_markerSprite.color = _color;
        }

        public void SetActive()
        {
            m_radCloud.SetMapMarkerStatus(true);
        }
        protected void RemoveMarker()
        {
            MapManager.Instance.RemoveMarker(this);
        }
    }
}

