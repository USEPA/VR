using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [RequireComponent(typeof(Camera))]
    public class MapCamera : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [Tooltip("If set to 0, this scales itself based on objects in the Ground and" +
             "Cover layers. If set >0, this is the OrthographicSize of the Camera.")]
        [SerializeField] private float camSize = 0;

        [Tooltip("If camSize==0, these are the layers to contain within the Camera view.")]
        // Note that we could have pulled the cullingMask from the Camera, but there 
        //  are some Layers that we want to render but don't necessarily want to zoom
        //  to (for instance, the EnemyLightCone layer).
        [SerializeField] private LayerMask layersToZoomTo;

        [Tooltip("What percent of the MiniMap should be left as a frame around the level?")]
        [SerializeField] private float zoomBorder = 0.1f;
        #endregion
        #region Private Variables
        private Camera cam;
        private Vector2 m_dimensions;
        private Vector3 m_minPosition;
        private Vector3 m_maxPosition;
        private Vector3 m_center;

        private Bounds m_bounds;
        #endregion
        #region Public Properties
        public static MapCamera Instance { get; set; }
        public Camera Camera
        {
            get 
            {
                if (!cam) cam = GetComponent<Camera>();
                return cam;
            }
        }

        public Vector3 MinPosition { get => m_minPosition; }
        public Vector3 MaxPosition { get => m_maxPosition; }

        public Vector3 Center { get => m_center; }

        public Bounds Bounds { get => m_bounds; }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            // Cache component
            cam = GetComponent<Camera>();
            //FillCameraWithLevel();
        }
        #endregion

        #region Position-Related Functionality
        public void FillCameraWithLevel()
        {
            // For this to work, the Camera must be orthographic
            if (!Camera.orthographic)
            {
                Debug.LogWarning("For MiniMap Camera scaling to work automatically, "
                                 + "the camera must be orthographic.");
                Camera.orthographic = true;
            }

            // This line is really slow, the rest is pretty fast though
            GameObject[] allGOs = FindObjectsOfType<GameObject>();

            Vector3 xzMin = Vector3.zero, xzMax = Vector3.zero;
            Renderer tRend;
            int binaryZoomLayers = layersToZoomTo.value;
            int binaryLayer;
            bool foundSomethingToZoomTo = false;

            for (int i = 0; i < allGOs.Length; i++)
            {
                // A little bit of binary bit shifting because that's fun…and fast!
                binaryLayer = 1 << allGOs[i].layer;

                // If the binary layer is 0010, and the binaryZoomLayers is 0110, then
                //  binaryLayer & binaryZoomLayers will be 0010, equal to binaryLayer.
                if ((binaryLayer & binaryZoomLayers) == binaryLayer)
                {

                    UnityEngine.Debug.Log($"{gameObject.name} found potential zoom object: {allGOs[i].name} || Time: {Time.time}");
                    // Use the bounds of the Renderer to get a good idea of extents.
                    tRend = allGOs[i].GetComponent<Renderer>();
                    if (tRend != null)
                    {
                        xzMin.x = Mathf.Min(xzMin.x, tRend.bounds.min.x);
                        xzMin.z = Mathf.Min(xzMin.z, tRend.bounds.min.z);
                        xzMax.x = Mathf.Max(xzMax.x, tRend.bounds.max.x);
                        xzMax.z = Mathf.Max(xzMax.z, tRend.bounds.max.z);

                        // We've found a GameObject that we should zoom to.
                        foundSomethingToZoomTo = true;
                    }
                }
            }

            /*
            // If there is nothing in the level that we wish to zoom to, then disable
            //  the MiniMap
            gameObject.SetActive(foundSomethingToZoomTo);
            if (!foundSomethingToZoomTo) return;
            */

            // Center the camera in the scene
            Vector3 center = (xzMin + xzMax) / 2f;

#if DEBUG_MAPCamera_ShowMinMax
            Debug.DrawLine(xzMin, center, Color.magenta, 60);
            Debug.DrawLine(xzMax, center, Color.magenta, 60);
#endif

            center.y = 16; // This number keeps this camera above the action.
            transform.position = center;

            // Scale the camera's orthographic size
            float ratioHW = (cam.rect.height * Screen.height) / (cam.rect.width * Screen.width);
            float width = xzMax.x - xzMin.x;
            float height = xzMax.z - xzMin.z;
            float size = Mathf.Max(width * ratioHW, height) * 0.5f;
            m_minPosition = xzMin;
            m_maxPosition = xzMax;
            m_dimensions = new Vector2(width, height);
            m_center = center;
            m_bounds = MathHelper.GetBounds(m_center, m_minPosition, m_maxPosition);
            UnityEngine.Debug.Log($"{gameObject.name} calculated size: {size} | Width: {width} | Height: {height} || Time: {Time.time}");
            size *= 1 + zoomBorder;
            cam.orthographicSize = size;
        }
        #endregion
    }
}

