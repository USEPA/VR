using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SampleWipeTemplate : XRGrabInteractable, IGrabbable
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] BoxCollider m_innerCollider;
        [SerializeField] SampleUnitObject m_sampleUnitPrefab = default;
        [SerializeField] GameObject originMarker;
        [Header("Snap Configuration")]
        [SerializeField] LayerMask m_snapObjectLayerMask;
        [SerializeField] float m_snapCheckRadius = 0.05f;
        [Header("Sample Unit Configuration")]
        [SerializeField] int m_totalSamples = 900;
        #endregion
        #region Private Variables
        private bool m_active = false;

        private float m_snapOffset;

        private Collider m_collider;

        private CollisionInfo m_currentCollision;


        private Bounds m_bounds;
        public Vector3 boundSize;
        private int m_size;
        private Vector3 m_itemSize;
        public Vector3 m_origin;
        private float m_individualUnitAmount;

        private SampleUnit[,] m_sampleGrid;
        private SampleSurfaceArea m_currentSampleSurface;

        public Collider m_currentContactedCollider;
        private Vector3 m_rayDirection;
        private Vector3 m_closestPointOnTarget;
        private Contamination m_currentTarget;

        private XRToolbeltItem m_socketItem;
        #endregion

        #region Public Properties
        public Vector3 LocalCenter { get => m_innerCollider.center; }
        public Vector3 WorldCenter { get => m_innerCollider.transform.TransformPoint(m_innerCollider.center); }

        public Bounds Bounds { get => m_bounds; }
        public Vector3 LocalBoundSize { get => m_innerCollider.transform.InverseTransformVector(m_innerCollider.bounds.size); }

        public SampleSurfaceArea CurrentSampleSurface { get => m_currentSampleSurface; }
        public Contamination CurrentTarget { get => m_currentTarget; }

        public XRToolbeltItem SocketItem
        {
            get
            {
                if (!m_socketItem) m_socketItem = GetComponent<XRToolbeltItem>();
                return m_socketItem;
            }
        }
        #endregion

        #region Initialization
        public void Init()
        {
            // Cache references/default values
            m_collider = GetComponent<Collider>();
            //m_bounds = m_innerCollider.GetComponent<MeshRenderer>().bounds;
            m_bounds = m_innerCollider.bounds;
            //m_snapOffset = m_innerCollider.bounds.extents.z;
            m_snapOffset = LocalBoundSize.z * 0.5f;
            // Create the grid
            InitGrid();
        }

        void InitGrid()
        {
            // Init grid
            m_size = (int)Mathf.Sqrt(m_totalSamples);
            //m_itemSize = new Vector3(Bounds.size.x / m_size, Bounds.size.y / m_size, m_innerCollider.size.z);
            m_itemSize = new Vector3(m_innerCollider.size.x / m_size, m_innerCollider.size.y / m_size, m_innerCollider.size.z);
            boundSize = Bounds.size;
            //m_origin = new Vector3((Bounds.min.x + Bounds.size.x) - (m_itemSize.x / 2), Bounds.min.y + (m_itemSize.y / 2), Bounds.center.z + 0.00001f);
            Vector3 localCenter = m_innerCollider.transform.InverseTransformPoint(Bounds.center);
            Vector3 localMin = m_innerCollider.transform.InverseTransformPoint(Bounds.min);
            localMin.z = localCenter.z;
            Vector3 localMax = m_innerCollider.transform.InverseTransformPoint(Bounds.max);
            localMax.z = localCenter.z;
            m_origin = new Vector3(localMin.x - (m_itemSize.x / 2), localMin.y + (m_itemSize.y / 2), localCenter.z);
            /*
            originMarker = Instantiate(m_sampleUnitPrefab, m_innerCollider.transform).gameObject;
            originMarker.transform.localPosition = m_localOrigin;
            originMarker.transform.localScale = m_localItemSize;
            originMarker.gameObject.name = "Origin";
            */
            //m_origin = new Vector3((Bounds.min.x + Bounds.size.x) - (m_itemSize.x / 2), Bounds.min.y + (m_itemSize.y / 2), localCenter.z);
            m_sampleGrid = new SampleUnit[m_size, m_size];
            m_individualUnitAmount = (1.0f / m_totalSamples) * 100.0f;
   
            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    Vector2 uvPosition = new Vector2((float)x / (float)(m_size - 1), (float)y / (float)(m_size - 1));
                    Vector3 localPosition = new Vector3(m_origin.x - (x * (m_itemSize.x)), m_origin.y + (y * (m_itemSize.y)), m_origin.z);
                    //Vector3 localPosition = new Vector3(localOrigin.x - (x * (m_itemSize.x)), localOrigin.y + (y * (m_itemSize.y)), 0.0f);
                    //Vector3 position = transform.InverseTransformPoint(worldPosition);
                    Vector3 position = localPosition;
                    m_sampleGrid[x, y] = new SampleUnit(index, uvPosition, position, m_individualUnitAmount);
                    /*
                    // Check if it is within the sample bounds
                    bool isSample = UnitInSampleBounds(index);
                    float distance = MathHelper.QuickDistance((Vector2)index, (Vector2)sampleCenter);
                    m_sampleGrid[x, y].SetIsSample(isSample, distance);
                    */
                }
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            //Init();
        }

        // Update is called once per frame
        void Update()
        {
            //if (!m_active) return;

            if (m_currentContactedCollider != null)
            {
                UnityEngine.Debug.DrawRay(transform.position, m_rayDirection, Color.cyan);
                //UnityEngine.Debug.DrawLine(transform.position, m_closestPointOnTarget, Color.green);
                //UnityEngine.Debug.DrawLine(transform.position, m_currentContactedCollider.transform.position, Color.green);
            }
            /*
            // Check for a snappable surface
            if (m_currentCollision != null)
            {
                UnityEngine.Debug.DrawRay(m_currentCollision.Point, m_currentCollision.Normal, Color.red);
            }
            */
        }

        #region Surface-Related Functionality
        public void AttemptSnapToSurface(bool _toolbelt = true)
        {
            // Check for any snappable surface
            if (CheckForSnappableSurface())
                SnapToSurface();
            else if (_toolbelt)
                SocketItem.ForceAttachToSocket();
        }
        public bool CheckForSnappableSurface()
        {
            Collider[] potentialSurfaces = Physics.OverlapSphere(transform.position, m_snapCheckRadius, m_snapObjectLayerMask);
            if (potentialSurfaces.Length > 0)
            {
                UnityEngine.Debug.Log($"CheckForSnappableSurfaces - Found Potential Surfaces: {potentialSurfaces.Length} || Time: {Time.time}");
                // Set up looping variables
                Collider closestCollider = null;
                Vector3 closestPoint = transform.position;
                float closestPointDistance = float.MaxValue;
                // Get the nearest object
                for (int i = 0; i < potentialSurfaces.Length; i++)
                {
                    if (potentialSurfaces[i] == m_collider || potentialSurfaces[i] == m_innerCollider) continue;
                    // Get the closest point on the object bounds to the template's center
                    Vector3 boundsPoint = potentialSurfaces[i].ClosestPointOnBounds(transform.position);
                    
                    // Get the distance to this point
                    float distance = MathHelper.QuickDistance(transform.position, boundsPoint);
                    UnityEngine.Debug.Log($"CheckForSnappableSurfaces - Potential Surface[{i}]: {potentialSurfaces[i]} | Distance: {distance} || Time: {Time.time}");
                    if (distance < closestPointDistance)
                    {
                        closestCollider = potentialSurfaces[i];
                        closestPoint = boundsPoint;
                        closestPointDistance = distance;
                    }
                }
                // Check for closest collider
                if (closestCollider != null)
                {
                    // Get the direction
                    //Vector3 rayDirection = (closestPoint - transform.position).normalized;
                    Vector3 rayDirection = (closestCollider.transform.position - transform.position).normalized;
                    UnityEngine.Debug.Log($"Found closest collider: {closestCollider.gameObject.name} || Time: {Time.time}");
                    m_currentContactedCollider = closestCollider;
                    m_rayDirection = rayDirection;
                    m_closestPointOnTarget = closestPoint;
                    // Raycast to the point
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, rayDirection, out hit, m_snapCheckRadius, m_snapObjectLayerMask))
                    {
                        closestPoint = hit.point;
                        // Create collision info
                        CollisionInfo collision = new CollisionInfo(closestCollider, closestPoint, hit.normal);
                        m_currentCollision = collision;

                        return true;
                    }
                    else
                    {
                        string errorMessage = (hit.collider != null) ? $"Hit: {hit.collider.gameObject.name}" : "No Contact";
                        UnityEngine.Debug.Log($"Failed raycast | {errorMessage} || Time: {Time.time}");
                    }
                }
            }
            return false;
            /*
            // Raycast from center of template and check for potential snappable surfaces
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -m_innerCollider.transform.forward, out hit, m_snapOffset * 2.0f, m_snapObjectLayerMask))
            {
                // Get the distance from this object
                float surfaceDistance = MathHelper.QuickDistance(transform.position, hit.point);
                // Create collision info
                CollisionInfo collision = new CollisionInfo(hit.collider, hit.point, hit.normal);

                m_currentCollision = collision;
            }
            */
        }

        public void SnapToSurface()
        {
            // Make sure there is valid collision info to perform snap logic with
            if (m_currentCollision == null) return;
            // Check if there is a sample surface on this object
            if (m_currentCollision.Collider.gameObject.TryGetComponent<Contamination>(out Contamination contamination))
            {
                UnityEngine.Debug.Log($"{gameObject.name} - Set Contaminant: {contamination.Agent} | Source: {contamination.gameObject} || Time: {Time.time}");
                // Cache contamination reference
                m_currentTarget = contamination;
            }
            SnapToSurface(m_currentCollision);
            transform.parent = null;
            //transform.parent = m_currentCollision.Collider.transform;
            //UnityEngine.Debug.Log($"Successfully snapped to surface: {m_currentCollision.Collider.gameObject.name} || Time: {Time.time}");
     
            /*
            // Check if there is a sample surface on this object
            if (m_currentCollision.Collider.gameObject.TryGetComponent<SampleSurfaceArea>(out SampleSurfaceArea sampleSurface))
            {
                // Cache sample surface reference
                m_currentSampleSurface = sampleSurface;
            }
            */

            // Clear collision info
            m_currentCollision = null;
            // Create the grid
            ShowGrid();
        }

        public void SnapToSurface(CollisionInfo _collision)
        {
            SnapToSurface(_collision.Point, _collision.Normal);
        }

        public void SnapToSurface(Vector3 _position, Vector3 _normal)
        {
            // Snap rotation
            //transform.rotation = Quaternion.FromToRotation(transform.forward, _normal);
            transform.rotation = Quaternion.LookRotation(_normal);
            // Snap position
            transform.position = _position + (_normal * m_snapOffset);
        }

        public void ClearSurface()
        {
            // Check if there are any sample unit objects
            if (m_innerCollider.transform.childCount > 0) HideGrid();
            transform.parent = null;
            m_currentTarget = null;
            m_currentSampleSurface = null;
        }
        #endregion

        #region Interaction-Related Functionality
        public void OnGrab()
        {
            ClearSurface();
        }

        public void OnDrop()
        {
            // Attempt to snap to a surface
            AttemptSnapToSurface();
        }


        protected override void Grab()
        {
            // Call base functionality
            base.Grab();
            // Invoke any specific grab functionality
            OnGrab();
        }

        protected override void Drop()
        {
            // Call base functionality
            base.Drop();
            // Invoke any specific drop functionality
            OnDrop();
        }
        #endregion

        #region Sample-Related Functionality
        public void ShowGrid()
        {
            // Loop through grid and create objects
            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    SampleUnit unit = m_sampleGrid[x, y];
                    //if (m_currentSampleSurface != null) unit.SourceArea = m_currentSampleSurface;
                    if (m_currentTarget != null) unit.Source = m_currentTarget;
                    unit.Object = CreateSampleUnitObject(unit);
                }
            }
        }

        public void HideGrid()
        {
            // Loop through grid and delete objects
            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    SampleUnit unit = m_sampleGrid[x, y];
                    unit.SetCleared(false);
                    Destroy(unit.Object.gameObject);
                    unit.Object = null;
                }
            }
        }

        public SampleUnitObject CreateSampleUnitObject(SampleUnit unit)
        {
            SampleUnitObject unitObject = Instantiate(m_sampleUnitPrefab);
            unitObject.transform.localScale = m_itemSize;
            unitObject.transform.parent = m_innerCollider.transform;
            unitObject.transform.localEulerAngles = Vector3.zero;
            unitObject.transform.localPosition = unit.Position;
            unitObject.gameObject.name = $"SampleUnit[{unit.Index.x}, {unit.Index.y}]";
            unitObject.Init(unit);
            return unitObject;
        }
        #endregion

        #region Editor Helper Methods
        #if UNITY_EDITOR
        [ContextMenu("Force Attach to Surface")]
        public void ForceAttemptSnapToSurface()
        {
            m_active = true;
            AttemptSnapToSurface(false);
        }
        #endif
        #endregion
    }

    public class CollisionInfo
    {
        #region Public Properties
        public Collider Collider { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        #endregion

        public CollisionInfo(Collider _collider, Vector3 _point, Vector3 _normal)
        {
            Collider = _collider;
            Point = _point;
            Normal = _normal;
        }
    }
}

