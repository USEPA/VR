using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class TestPainter : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] ChemicalContaminantSite m_presetSite;
        [SerializeField] ChemicalAgent m_presetAgent;
        [SerializeField] bool m_useCollisionSystem = false;
        [SerializeField] CollisionPainter m_collisionPainterPrefab;
        [SerializeField] Vector2 m_spreadRange = new Vector2(0.1f, 0.75f);
        [SerializeField] float m_spillStrength = 1.0f;
        [SerializeField] float m_spillHardness = 0.15f;
        [SerializeField] Transform m_randomOriginMarker;
        [SerializeField] Transform m_closestPointMarker;
        [SerializeField] Transform m_originMarker;
        [SerializeField] bool m_showPaintDebugLines = false;
        [Header("Debug Values")]
        public Vector3 m_randomSphereValue;
        public float m_radius;
        public Vector3 m_origin;
        public Paintable m_closestPaintable;
        public List<Paintable> m_affectedObjects;
        public Color m_spillColor;
        #endregion
        #region Protected Variables
        protected SphereCollider m_vaporCloud;
        protected CollisionPainter m_collisionPainter;
        public List<PaintableSnapshot> m_paintableSnapshots;
        #endregion
        #region Public Properties
        public float Radius { get => m_radius; }
        public Color SpillColor { get => m_spillColor; }
        public Vector3 Origin { get => m_origin; }
        public float Strength { get => m_spillStrength; }
        public float Hardness { get => m_spillHardness; }

        public List<PaintableSnapshot> Snapshots
        {
            get
            {
                if (m_paintableSnapshots == null) m_paintableSnapshots = new List<PaintableSnapshot>();
                return m_paintableSnapshots;
            }
        }
        #endregion

        #region Initialization
        // Start is called before the first frame update
        void Start()
        {

        }

        public void Init()
        {
            // Get a random radius
            //m_radius = Random.Range(m_presetSite.SpreadSizeRange.x, m_presetSite.SpreadSizeRange.y * 0.5f);
            m_radius = Random.Range(m_spreadRange.x, m_spreadRange.y);
            // Generate a random point
            m_randomSphereValue = Random.insideUnitSphere; 
            Vector3 randomOrigin = m_presetSite.transform.position + (m_randomSphereValue * m_radius);
            m_randomOriginMarker.position = randomOrigin;
            Vector3 origin = randomOrigin;
            Paintable closestPaintable = null;
            Collider closestCollider = null;

            if (m_presetSite is ContaminantLeakSite leakSite)
            {
                // Get a random leak source
                ChemicalObject leakSource = leakSite.GetRandomLeakSource();
                randomOrigin = leakSite.GetRandomLeakPoint();
                UnityEngine.Debug.Log($"{m_presetSite.gameObject.name} selected random leak source: {leakSource.gameObject.name} | Origin: {randomOrigin} || Time: {Time.time}");



                if (leakSource.TryGetComponent<Collider>(out Collider col))
                {
                    Paintable paintable = PaintManager.Instance.GetPaintable(col);
                    if (paintable != null)
                    {
                        closestPaintable = paintable;
                        origin = randomOrigin;
                    }
                }
            }

            Vector3 closestColliderOrigin = m_randomOriginMarker.position;
            float closestDistance = float.MaxValue;

            if (closestPaintable == null)
            {
                Collider[] potentialColliders = Physics.OverlapSphere(origin, m_radius);
                if (potentialColliders == null || potentialColliders.Length < 1) return;

                foreach (Collider col in potentialColliders)
                {
                    // First off, try to get a paintable component from this
                    Paintable p = PaintManager.Instance.GetPaintable(col);
                    if (p == null) continue;

                    // Get the closest point to the random origin
                    Vector3 closestColPoint = col.ClosestPoint(randomOrigin);
                    Vector3 closestPoint = closestColPoint;

                    if (col is MeshCollider meshCollider && !meshCollider.convex && p.Renderer != null)
                    {
                        SampleableMesh sampleMesh = p.gameObject.GetComponent<SampleableMesh>();
                        if (sampleMesh == null) sampleMesh = p.gameObject.AddComponent<SampleableMesh>();
                        closestPoint = sampleMesh.GetNearestPoint(randomOrigin);
                    }

                    // Get the distance to this point
                    float distance = MathHelper.QuickDistance(randomOrigin, closestPoint);
                    if (distance < closestDistance)
                    {
                        closestPaintable = p;
                        closestCollider = col;
                        closestDistance = distance;
                        origin = closestPoint;
                    }
                }
            }


            /*
            foreach (KeyValuePair<int, Paintable> item in PaintManager.Instance.Paintables)
            {
                Paintable p = item.Value;
                if (p.TryGetComponent<Collider>(out Collider col))
                {
                    // Get the closest point to the ranndom origin
                    Vector3 closestColPoint = col.ClosestPoint(randomOrigin);
                    Vector3 closestPoint = closestColPoint;
                    if (col is MeshCollider meshCollider && !meshCollider.convex && p.Renderer != null)
                    {
                        SampleableMesh sampleMesh = p.gameObject.GetComponent<SampleableMesh>();
                        if (sampleMesh == null) sampleMesh = p.gameObject.AddComponent<SampleableMesh>();
                        closestPoint = sampleMesh.GetNearestPoint(randomOrigin);
                    }
                    
                    // Get the distance to this point
                    float distance = MathHelper.QuickDistance(randomOrigin, closestPoint);
                    if (distance < closestDistance)
                    {
                        closestPaintable = p;
                        closestCollider = col;
                        closestDistance = distance;
                        closestColliderOrigin = closestColPoint;
                        origin = closestPoint;
                    }
                }
            }
            */

            if (closestPaintable == null) return;
            m_origin = origin;
            m_originMarker.position = origin;
            m_closestPointMarker.position = m_presetSite.transform.position + m_randomSphereValue;
            m_closestPaintable = closestPaintable;
            m_affectedObjects = new List<Paintable>();
            m_affectedObjects.Add(m_closestPaintable);
            // Get the spill color
            m_spillColor = m_presetAgent.SpawnColor;
            m_spillColor.a = Mathf.Clamp(m_spillColor.a, 0.4f, 1.0f);
            //m_spillColor.a = 1.0f;
            if (m_useCollisionSystem && m_collisionPainterPrefab) 
                PerformColliderPaint(origin);
            else
                PerformRaycastPaint(origin, randomOrigin, closestCollider);
            
        }

        public void PerformRaycastPaint(Vector3 origin, Vector3 randomOrigin, Collider closestCollider)
        {
            Vector3 rayOrigin = origin + ((m_presetSite.transform.position - origin) * 10.0f);
            Ray ray = new Ray(rayOrigin, (origin - rayOrigin));
            //Ray ray = new Ray(randomOrigin, origin - randomOrigin);
            //Ray ray = new Ray(m_presetSite.transform.position, origin - m_presetSite.transform.position);
            RaycastHit hit;
            bool ignoreRaycast = true;

            if (ignoreRaycast) // m_closestPaintable.MeshCollider.Raycast(ray, out hit, 100.0f)
            {
                gameObject.name = $"Spill: {m_presetSite.gameObject.name}";
                
                /*
                if (origin != hit.point)
                {
                    origin = hit.point;
                    m_originMarker.transform.position = origin;
                }*/
                
                // Paint the source object
                Paint(m_closestPaintable, origin, m_spillColor, m_radius, m_spillStrength, m_spillHardness);
                float adjustedParentRadius = m_radius * 0.7783056587f;

                Collider[] colliders = Physics.OverlapSphere(origin, adjustedParentRadius, LayerMask.GetMask("Default", "LevelStructure", "Teleport"));
                UnityEngine.Debug.Log($"{gameObject.name} found colliders: {colliders.Length} | Site: {m_presetSite.gameObject.name} | Radius: {adjustedParentRadius} || Time: {Time.time}");
                if (colliders != null && colliders.Length > 1)
                {
                    foreach (Collider collider in colliders)
                    {
                        if (collider == closestCollider) continue;
                        Paintable paintable = PaintManager.Instance.GetPaintable(collider);
                        if (paintable != null)
                        {
                            // Get the closest point on the collider to the origin
                            Vector3 closestPoint = collider.ClosestPoint(origin);
                            if (collider is MeshCollider meshCollider && !meshCollider.convex && paintable.Renderer != null)
                            {

                                SampleableMesh sampleMesh = paintable.gameObject.GetComponent<SampleableMesh>();
                                if (sampleMesh == null) sampleMesh = paintable.gameObject.AddComponent<SampleableMesh>();
                                closestPoint = sampleMesh.GetNearestPoint(origin);
                            }
                            /*
                            if (paintable.gameObject.name == "Shelf")
                            {
                                UnityEngine.Debug.Log($"{gameObject.name} painting shelf: {collider.gameObject.name} | Point: {closestPoint} || Time: {Time.time}");
                                GameObject closestPointMarker = Instantiate(m_closestPointMarker.gameObject, collider.transform);
                                closestPointMarker.transform.position = closestPoint;
                            }
                            */
                            // Get the distance to origin
                            float originDistance = MathHelper.QuickDistance(origin, closestPoint);
                            float scale = (1 - (originDistance / m_radius));
                            // Get the adjusted radius
                            float adjustedRadius = scale * m_radius;
                            UnityEngine.Debug.Log($"{gameObject.name} Painting: {paintable.gameObject.name} | Distance: {originDistance} | Parent Radius: {adjustedParentRadius} | Scale: {scale} | Adjusted Radius: {adjustedRadius} || Time: {Time.time}");

                            // Paint the object
                            Paint(paintable, closestPoint, m_spillColor, adjustedRadius, m_spillStrength, m_spillHardness);

                            m_affectedObjects.Add(paintable);
                            Snapshots.Add(new PaintableSnapshot(paintable, closestPoint, adjustedRadius));
                        }
                    }
                }
                GameObject vaporCloudObject = new GameObject();
                vaporCloudObject.name = $"{m_presetSite.gameObject.name} Vapor Cloud";
                vaporCloudObject.transform.parent = m_presetSite.transform;
                vaporCloudObject.transform.localPosition = Vector3.zero;
                vaporCloudObject.transform.localEulerAngles = Vector3.zero;
                vaporCloudObject.transform.position = origin;

                m_vaporCloud = vaporCloudObject.AddComponent<SphereCollider>();
                m_vaporCloud.radius = m_radius;
                m_vaporCloud.isTrigger = true;
            }
        }

        public void PerformColliderPaint(Vector3 origin)
        {
            m_collisionPainter = Instantiate(m_collisionPainterPrefab.gameObject, transform).GetComponent<CollisionPainter>();
            m_collisionPainter.transform.position = origin;
            m_collisionPainter.transform.localEulerAngles = Vector3.zero;

            m_collisionPainter.Init(m_radius, m_spillColor, m_spillStrength, m_spillHardness);

        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }

        public void Paint(Paintable p, Vector3 _pos, Color _color, float _radius, float _strength, float _hardness)
        {
            //UnityEngine.Debug.Log($"Painting {p.gameObject.name} | Point: {_pos} | Radius: {_radius} | Strength: {_strength} | Color: {_color} || Time: {Time.time}");
            PaintManager.Instance.Paint(p, _pos, _radius, _hardness, _strength, _color);
        }

        public void Repaint(Paintable p, Vector3 _pos, Color _color, float _radius, float _strength, float _hardness)
        {
            // Get the distance to origin
            float originDistance = MathHelper.QuickDistance(m_origin, _pos);
            float scale = (1 - (originDistance / m_radius));
            // Get the adjusted radius
            float adjustedRadius = scale * m_radius;
            // Paint according to radius
            Paint(p, _pos, _color, adjustedRadius, _strength, _hardness);
        }

    #if UNITY_EDITOR
        [ContextMenu("Attempt Repaint")]
        public void AttemptRepaint()
        {
            if (!m_closestPaintable) return;
            Paint(m_closestPaintable, m_originMarker.transform.position, m_spillColor, m_radius, m_spillStrength, m_spillHardness);
        }

        [ContextMenu("Rename to Spill Site")]
        public void RenameToSpillSite()
        {
            gameObject.name = $"Spill: {m_presetSite.gameObject.name}";
            transform.position = m_presetSite.transform.position;
        }


#endif
        private void OnDrawGizmosSelected()
        {
            if (!m_showPaintDebugLines) return;
            if (m_closestPaintable != null && m_paintableSnapshots != null && m_paintableSnapshots.Count > 0)
            {
                Gizmos.color = Color.cyan;
                foreach (PaintableSnapshot snapshot in m_paintableSnapshots)
                {
                    Gizmos.DrawLine(m_origin, snapshot.m_position);
                }
            }
        }
    }

    [System.Serializable]
    public class PaintableSnapshot
    {
        public Paintable m_paintable;
        public Vector3 m_position;
        public float m_radius;

        public PaintableSnapshot(Paintable _paintable, Vector3 _position, float _radius)
        {
            m_paintable = _paintable;
            m_position = _position;
            m_radius = _radius;
        }
    }
}

