using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace L58.EPAVR
{
    public class MapTextureCapture : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private MapCamera m_mapCamera;
        [Header("Default Configuration")]
        [SerializeField] private string m_filePath = "Assets/RTI/Textures/UI/Screenshots/MapImages";
        [SerializeField] private Vector2 m_clippingPlane = new Vector2(1, 500);
        #endregion

        #region Public Properties
        public Camera Camera
        {
            get
            {
                if (m_mapCamera) return m_mapCamera.Camera;
                return null;
            }
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (!m_mapCamera) return;
            m_mapCamera.FillCameraWithLevel();
            MapManagerInjector mapInjector = FindObjectOfType<MapManagerInjector>();
            if (mapInjector) m_clippingPlane = mapInjector.ClippingPlaneOverride;
            m_mapCamera.Camera.nearClipPlane = m_clippingPlane.x;
            m_mapCamera.Camera.farClipPlane = m_clippingPlane.y;
        }

        // Update is called once per frame
        void Update()
        {

        }


        #if UNITY_EDITOR
        [ContextMenu("Render Camera to File")]
        public void RenderCameraToFile()
        {
            RenderTexture oldRT = Camera.targetTexture;
            RenderTexture rt = new RenderTexture(oldRT.width, oldRT.height, oldRT.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Camera.targetTexture = rt;
            Camera.Render();
            Camera.targetTexture = oldRT;

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string sceneName = SceneManager.GetActiveScene().name;
            string path = $"{m_filePath}/{sceneName}-Map.png";
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            Debug.Log($"Saved screenshot to {path} || Time: {Time.time}");
        }
        #endif

    }
}

