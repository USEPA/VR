using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class VaporCloudMapMarker : MapMarkerObject
    {
        #region Private Variables
        private VaporCloud m_vaporCloud;
        #endregion

        #region Initialization
        public override void Init(Transform _parent, Color _color, bool _isDynamic = false)
        {
            // Call base functionality
            base.Init(_parent, _color, _isDynamic);
            // Try to get a vapor cloud component from the parent
            if (m_parent.TryGetComponent<VaporCloud>(out m_vaporCloud))
            {
                // Scale the object according to the size of the vapor cloud
                float scale = m_vaporCloud.Radius * 2.0f;
                transform.localScale = Vector3.one * scale;

                if (m_vaporCloud.Parent != null)
                {
                    m_vaporCloud.Parent.OnSiteCleared += (i, j) => MapManager.Instance.RemoveMarker(this);
                }
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

