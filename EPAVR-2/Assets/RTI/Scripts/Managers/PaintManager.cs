using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace L58.EPAVR
{
    public class PaintManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Shader texturePaint;
        [SerializeField] private Shader extendIslands;
        [Header("Default Configuration")]
        [SerializeField] private bool m_enabled = true;
        [Header("Testing")]
        [SerializeField] private List<TestPainter> m_testPainters;
        [SerializeField] private int m_seed = -1;
        [SerializeField] private bool m_useSeed = false;
        #endregion
        #region Private Variables
        ManagerStatus m_status;

        int prepareUVID = Shader.PropertyToID("_PrepareUV");
        int positionID = Shader.PropertyToID("_PainterPosition");
        int hardnessID = Shader.PropertyToID("_Hardness");
        int strengthID = Shader.PropertyToID("_Strength");
        int radiusID = Shader.PropertyToID("_Radius");
        int blendOpID = Shader.PropertyToID("_BlendOp");
        int colorID = Shader.PropertyToID("_PainterColor");
        int textureID = Shader.PropertyToID("_MainTex");
        int uvOffsetID = Shader.PropertyToID("_OffsetUV");
        int uvIslandsID = Shader.PropertyToID("_UVIslands");

        Material paintMaterial;
        Material extendMaterial;

        CommandBuffer command;

        Dictionary<Collider, Paintable> m_paintables;
        #endregion
        #region Public Properties
        public static PaintManager Instance { get; set; }
        public ManagerStatus Status => m_status;
        public bool Enabled { get => m_enabled; set => m_enabled = value; }
        public Dictionary<Collider, Paintable> Paintables { get => m_paintables; }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Set singleton
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            /*
            if (m_useSeed && m_seed != -1)
            {
                UnityEngine.Random.InitState(m_seed);
            }
            else
            {
                m_seed = UnityEngine.Random.seed;
            }*/

            /*
            // Create materials
            paintMaterial = new Material(texturePaint);
            extendMaterial = new Material(extendIslands);
            // Create command buffer
            command = new CommandBuffer();
            command.name = "CommmandBuffer - " + gameObject.name;
            //Startup();
            */
        }

        void Start()
        {
            //Startup();
        }


        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            // Create materials
            if (!paintMaterial) paintMaterial = new Material(texturePaint);
            if (!extendMaterial) extendMaterial = new Material(extendIslands);
            // Create a command buffer
            command = new CommandBuffer();
            command.name = $"CommandBuffer - {gameObject.name}";
            // Create paintable master registry
            m_paintables = new Dictionary<Collider, Paintable>();
            // Find all game objects with paintable components
            Paintable[] paintables = FindObjectsOfType<Paintable>();
            if (paintables != null && paintables.Length > 0)
            {
                foreach(Paintable paintable in paintables)
                {
                    RegisterPaintable(paintable);
                    //InitPaintable(paintable);
                }
            }

            /*
            if (m_testPainters != null && m_testPainters.Count > 0)
            {
                foreach (TestPainter painter in m_testPainters) painter.Init();
            }
            */
            
            // Finish initialization
            UnityEngine.Debug.Log($"PaintManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }
        #endregion

        #region Paint-Related Functionality
        public void Paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
        {
            // Make sure the paintable is initialized
            if (!paintable.IsInitialized) return;

            // Cache references
            RenderTexture mask = paintable.Mask;
            RenderTexture uvIslands = paintable.UVIslands;
            RenderTexture extend = paintable.Extend;
            RenderTexture support = paintable.Support;
            Renderer rend = paintable.Renderer;

            // Set all the paint parameters
            paintMaterial.SetFloat(prepareUVID, 0);
            paintMaterial.SetVector(positionID, pos);
            paintMaterial.SetFloat(hardnessID, hardness);
            paintMaterial.SetFloat(strengthID, strength);
            paintMaterial.SetFloat(radiusID, radius);
            paintMaterial.SetTexture(textureID, support);
            paintMaterial.SetColor(colorID, color ?? Color.red);
            extendMaterial.SetFloat(uvOffsetID, paintable.ExtendIslandsOffset);
            extendMaterial.SetTexture(uvIslandsID, uvIslands);

            // Set up the command buffer
            command.SetRenderTarget(mask);
            command.DrawRenderer(rend, paintMaterial, 0);

            command.SetRenderTarget(support);
            command.Blit(mask, support);

            command.SetRenderTarget(extend);
            command.Blit(mask, extend, extendMaterial);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }

        public Color Sample(RaycastHit hit)
        {
            // Try to get a paintable from this
            if (hit.collider)
            {
                if (hit.collider is MeshCollider)
                {
                    Paintable p = GetPaintable(hit.collider);
                    if (p != null)
                    {
                        UnityEngine.Debug.Log($"PaintManager - Attempting to Collect Sample: {p.gameObject.name} || Time: {Time.time}");
                        // Sample based on the raycast's texture position
                        return Sample(hit.textureCoord, p);
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"PaintManager - ERROR: Failed to retrieve sample from {hit.collider.gameObject.name} | No Mesh Collider Found! || Time: {Time.time}");
                }
            }
            return Color.clear;
        }

        public Color Sample(Vector2 textureCoord, Paintable p)
        {
            // Get the render texture of the paintable and make a temporary Texture2D for sampling from it
            RenderTexture tex = p.Mask;
            Texture2D texture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
            // Convert the texture coordinates
            int x = Mathf.FloorToInt(textureCoord.x * texture.width);
            int y = Mathf.FloorToInt(textureCoord.y * texture.height);
            Vector2 texturePos = new Vector2(x, y);
            // Set this as the active render texture and read all pixels
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            RenderTexture.active = tex;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply(false);
            // Return the color value of the paintable at this position
            return texture.GetPixel(x, y);
        }
        #endregion

        #region Registry-Related Functionality
        public void InitPaintable(Paintable _paintable)
        {
            // Make sure the paintable has not already been registered
            //if (HasKey(_paintable.gameObject.GetInstanceID())) return;
            // Initialize the paintable if necessary
            if (!_paintable.IsInitialized) _paintable.Init();

            // Cache references
            RenderTexture mask = _paintable.Mask;
            RenderTexture uvIslands = _paintable.UVIslands;
            RenderTexture extend = _paintable.Extend;
            RenderTexture support = _paintable.Support;
            Renderer rend = _paintable.Renderer;

            // Set the render targets
            command.SetRenderTarget(mask);
            command.SetRenderTarget(extend);
            command.SetRenderTarget(support);

            paintMaterial.SetFloat(prepareUVID, 1);
            command.SetRenderTarget(uvIslands);
            command.DrawRenderer(rend, paintMaterial, 0);

            // Execute the command
            Graphics.ExecuteCommandBuffer(command);
            command.Clear();

            // Register the paintable
            //RegisterPaintable(_paintable);
        }

        
        public Paintable GetPaintable(Collider _col)
        {
            if (!HasKey(_col)) return null;

            return m_paintables[_col];
        }

        protected void RegisterPaintable(Paintable _paintable)
        {
            if (m_paintables == null) m_paintables = new Dictionary<Collider, Paintable>();
            //UnityEngine.Debug.Log($"Arrived in RegisterPaintable: {_paintable.gameObject.name} || Time: {Time.time}");
            _paintable.Setup();
            if (_paintable.Colliders != null && _paintable.Colliders.Count > 0)
            {
                //UnityEngine.Debug.Log($"RegisterPaintable: {_paintable.gameObject.name}  passed collider check: {_paintable.Colliders.Count} || Time: {Time.time}");
                foreach (Collider col in _paintable.Colliders)
                {
                    // Add a key value pair for this collider and the paintable
                    m_paintables.Add(col, _paintable);
                }
            }
            //m_paintables.Add(_paintable.gameObject.GetInstanceID(), _paintable);
        }

        public bool HasKey(Collider _col)
        {
            return (m_paintables != null && m_paintables.ContainsKey(_col));
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin resetting
            m_status = ManagerStatus.Resetting;
            // Clear the dictionary
            if (m_paintables != null)
            {
                m_paintables.Clear();
                m_paintables = null;
            }
            // Clear the command buffer
            if (command != null) command = null;
            // Finish reset
            UnityEngine.Debug.Log($"PaintManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }
        #endregion
    }
}

