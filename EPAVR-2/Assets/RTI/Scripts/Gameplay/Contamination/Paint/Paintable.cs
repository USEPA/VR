using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Paintable : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected List<Collider> m_colliders;
        [SerializeField] protected MeshCollider m_meshCollider;
        [Header("Default Configuration")]
        [SerializeField] protected float m_extendsIslandOffset = 1;
        public Transform m_repaintTransform;
        public TestPainter m_sourcePainter;
        #endregion
        #region Protected Variables
        protected Renderer m_renderer;
        protected bool m_isInitialized = false;
        protected RenderTexture m_extendIslandsRenderTexture;
        protected RenderTexture m_uvIslandsRenderTexture;
        protected RenderTexture m_maskRenderTexture;
        protected RenderTexture m_supportTexture;

        protected int m_maskTextureID = Shader.PropertyToID("_MaskTexture");
        protected int m_mainTextureID = Shader.PropertyToID("_MainTexture");
        protected const int DEFAULT_TEXTURE_SIZE = 1024;
        protected int m_textureSize;

        public List<Contamination> m_contaminants;
        #endregion
        #region Public Properties
        public bool IsInitialized { get => m_isInitialized; }
        public List<Collider> Colliders { get => m_colliders; }
        public MeshCollider MeshCollider { get => m_meshCollider; }
        public RenderTexture Mask { get => m_maskRenderTexture; }
        public RenderTexture UVIslands { get => m_uvIslandsRenderTexture; }
        public RenderTexture Extend { get => m_extendIslandsRenderTexture; }
        public RenderTexture Support { get => m_supportTexture; }

        public int TextureSize
        {
            get => m_textureSize;
        }

        public Renderer Renderer
        {
            get
            {
                if (!m_renderer) m_renderer = GetComponent<Renderer>();
                return m_renderer;
            }
        }

        public float ExtendIslandsOffset { get => m_extendsIslandOffset; }

        public List<Contamination> Contaminants
        {
            get
            {
                if (m_contaminants == null) m_contaminants = new List<Contamination>();
                return m_contaminants;
            }
        }
        #endregion

        #region Initialization
        // Start is called before the first frame update
        void Start()
        {

        }

        public void Setup()
        {
            if (m_meshCollider && PaintManager.Instance && !PaintManager.Instance.Enabled) m_meshCollider.enabled = false;
        }
        public void Init()
        {
            // Cache texture size
            m_textureSize = DEFAULT_TEXTURE_SIZE;
            // Try to get the main texture of the renderer
            if (Renderer.material.GetTexture(m_mainTextureID) != null)
            {
                m_textureSize = Renderer.material.GetTexture(m_mainTextureID).width;
            }
            // Create the render textures
            m_maskRenderTexture = new RenderTexture(m_textureSize, m_textureSize, 0);
            m_maskRenderTexture.filterMode = FilterMode.Bilinear;

            m_extendIslandsRenderTexture = new RenderTexture(m_textureSize, m_textureSize, 0);
            m_extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            m_uvIslandsRenderTexture = new RenderTexture(m_textureSize, m_textureSize, 0);
            m_uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            m_supportTexture = new RenderTexture(m_textureSize, m_textureSize, 0);
            m_supportTexture.filterMode = FilterMode.Bilinear;
            // Set the render texture
            Renderer.material.SetTexture(m_maskTextureID, m_extendIslandsRenderTexture);
            // Set initialized
            m_isInitialized = true;

        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            if (m_maskRenderTexture) m_maskRenderTexture = null;
            if (m_extendIslandsRenderTexture) m_extendIslandsRenderTexture = null;
            if (m_uvIslandsRenderTexture) m_uvIslandsRenderTexture = null;
            if (m_supportTexture) m_supportTexture = null;
        }

#if UNITY_EDITOR
        [ContextMenu("Set Up Collider References")]
        public void SetUpColliderRefs()
        {
            List<Collider> returnedColliders = new List<Collider>();
            // First off, get any collider component on this object
            Component[] sourceColliderComponents = GetComponents(typeof(Collider));

            if (sourceColliderComponents.Length > 0)
            {
                foreach (Collider sourceCollider in sourceColliderComponents)
                {
                    if (sourceCollider is MeshCollider meshCollider && !meshCollider.convex) continue;
                    returnedColliders.Add(sourceCollider);
                } 
            }
            // Now get any child collider components
            Component[] childColliderComponents = GetComponentsInChildren(typeof(Collider));

            if (childColliderComponents.Length > 0)
            {
                foreach (Collider childCollider in sourceColliderComponents)
                {
                    if (!childCollider.enabled || (childCollider is MeshCollider meshCollider && !meshCollider.convex)) continue;
                    returnedColliders.Add(childCollider);
                }
            }

            if (returnedColliders.Count > 0)
            {
                if (m_colliders == null) m_colliders = new List<Collider>();
                foreach(Collider collider in returnedColliders)
                {
                    if (!collider.enabled || (m_colliders.Count > 0 && m_colliders.Contains(collider))) continue;
                    m_colliders.Add(collider);
                }
            }

            // Now handle the mesh collider reference
            if (m_meshCollider == null && gameObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
            {
                Mesh sourceMesh = meshFilter.sharedMesh;
                // Check children
                if (transform.childCount > 0)
                {
                    for(int i = 0; i < transform.childCount; i++)
                    {
                        GameObject childObject = transform.GetChild(i).gameObject;
                        if (childObject.name.Contains("_MeshCollider") && childObject.TryGetComponent<MeshCollider>(out MeshCollider meshCol) && !meshCol.convex)
                        {
                            m_meshCollider = meshCol;
                            return;
                        }
                    }
                }

                // Create a new mesh collider object
                GameObject meshColliderObject = new GameObject();
                meshColliderObject.name = $"{gameObject.name}_MeshCollider";
                meshColliderObject.transform.parent = transform;
                meshColliderObject.transform.localPosition = Vector3.zero;
                meshColliderObject.transform.localEulerAngles = Vector3.zero;
                meshColliderObject.transform.localScale = Vector3.one;
                // Add the mesh collider component
                m_meshCollider = meshColliderObject.AddComponent<MeshCollider>();
                m_meshCollider.sharedMesh = sourceMesh;
                // Now set the layer to Paintable
                m_meshCollider.gameObject.layer = LayerMask.NameToLayer("Paintable");
            }
        }

        [ContextMenu("Attempt Repaint At Transform")]
        public void AttemptRepaintAtTransform()
        {
            if (!m_repaintTransform || !m_sourcePainter) return;
            m_sourcePainter.Repaint(this, m_repaintTransform.position, m_sourcePainter.SpillColor, m_sourcePainter.Radius, m_sourcePainter.Strength, m_sourcePainter.Hardness);
        }

        public void CacheColliders()
        {
            // First, check if the list needs to be initialized
            if (m_colliders == null) m_colliders = new List<Collider>();
            if (gameObject.TryGetComponent<MeshRenderer>(out MeshRenderer sourceMesh))
            {
                // Get any contaminant components
                Component[] colliderComponents = gameObject.GetComponents(typeof(Collider));

                if (colliderComponents != null && colliderComponents.Length > 0)
                {
                    // Loop through any of the colliders on this object and add them if necessary
                    foreach (Collider sourceCollider in colliderComponents)
                    {
                        // Make sure the collider is enabled, not a trigger, and that the list does not already contain it
                        if (!sourceCollider.enabled || sourceCollider.isTrigger || m_colliders.Contains(sourceCollider)) continue;
                        // Add this to the list
                        m_colliders.Add(sourceCollider);
                    }
                }


                Component[] childColliderComponents = gameObject.GetComponentsInChildren<Collider>();

                if (childColliderComponents != null && childColliderComponents.Length > 0)
                {
                    // Loop through any of the  childcolliders and add them if necessary
                    foreach (Collider childCollider in childColliderComponents)
                    {
                        // Make sure the collider is enabled, not a trigger, and that the list does not already contain it
                        if (!childCollider.enabled || childCollider.isTrigger || m_colliders.Contains(childCollider)) continue;

                        if (childCollider.TryGetComponent<MeshRenderer>(out MeshRenderer childMesh)) continue;
                        // Add this to the list
                        m_colliders.Add(childCollider);
                    }
                }
            }
        }

        /*
        public void FixMeshColliderReference()
        {
            if (m_colliders == null || m_colliders.Count < 1) return;

            if (m_colliders.Count == 1 && m_colliders[0] is MeshCollider meshCollider)
            {
                m_meshCollider = meshCollider;
                return;
            }


            List<Collider> newColliderList = new List<Collider>();
            foreach(Collider col in m_colliders)
            {
                if (col is MeshCollider meshCol)
                {
                    m_meshCollider = meshCol;
                    continue;
                }
                newColliderList.Add(col);
            }

            m_colliders = newColliderList;
        }
        */

        public void BringCollidersBack()
        {
            if (m_colliders != null && m_colliders.Count < 2) return;
            List<Collider> returnedColliders = new List<Collider>();

            foreach(Collider sourceCol in m_colliders)
            {
                if (sourceCol.gameObject == gameObject || sourceCol is MeshCollider meshCollider) continue;

                Collider newCol = null;
                if (sourceCol is BoxCollider boxCollider)
                {
                    newCol = gameObject.AddComponent<BoxCollider>();
                    ((BoxCollider)newCol).center = boxCollider.center;
                    ((BoxCollider)newCol).size = boxCollider.size;
                }
                else if (sourceCol is SphereCollider sphereCollider)
                {
                    newCol = gameObject.AddComponent<SphereCollider>();
                    ((SphereCollider)newCol).center = sphereCollider.center;
                    ((SphereCollider)newCol).radius = sphereCollider.radius;
                }

                returnedColliders.Add(newCol);
            }

            m_colliders = returnedColliders;
            
        }
#endif
    }
}

