using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MapManagerInjector : MonoBehaviour, IManagerInjector
    {
        #region Inspector Assigned Variables
        [Header("Map Camera Configuration")]
        [SerializeField] private Vector2 m_clippingPlaneOverride = new Vector2(1.0f, 1000.0f);
        #endregion
        #region Public Properties
        public Vector2 ClippingPlaneOverride { get => m_clippingPlaneOverride; }
        #endregion
        #region Initialization
        public void Init()
        {
            if (!MapManager.Instance || MapCamera.Instance == null) return;
            MapCamera.Instance.Camera.nearClipPlane = m_clippingPlaneOverride.x;
            MapCamera.Instance.Camera.farClipPlane = m_clippingPlaneOverride.y;
        }
        #endregion
    }
}

