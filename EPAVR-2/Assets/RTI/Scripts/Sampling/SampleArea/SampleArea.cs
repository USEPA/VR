using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SampleArea : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Configuration")]
        [SerializeField] protected ChemicalAgent m_contaminant;
        [SerializeField] protected int m_totalSamples = 900;
        [SerializeField] protected int m_sampleRadius = 10;
        [Header("Important References")]
        [SerializeField] protected SampleUnitObject m_sampleUnitPrefab = default;
        [SerializeField] protected MeshRenderer m_mesh;
        [SerializeField] protected Collider m_collider;
        [SerializeField] protected bool m_activeOnStart = false;
        [SerializeField] protected GameObject m_toolCollisionMarkerPrefab;
        #endregion
        #region Protected Variables
        protected bool m_agentIdentified = false;

        protected int m_size;
        protected SampleUnit[,] m_sampleGrid;
        protected Vector3 m_itemSize;
        protected Vector3 m_origin;

        protected bool m_active = false;

        protected Vector3 m_epicenter;

        public bool m_pointerInBounds = false;
        protected MeshRenderer m_parentMesh;
        protected bool m_moveEnabled = false;
        public L58XRHand m_currentInteractor;
        protected XRActionMap m_currentInteractorActionMap;
        protected InputActionReference m_currentInteractorPositionRef;
        public GameObject hitObject;
        public Vector2Int sampleCenter;

        protected System.Action<bool> m_onSetMovable;
        protected ToolType m_currentToolType;

        protected GameObject m_toolCollisionMarker;
        protected GameObject m_currentTrackedTool;

        protected System.Action m_onIdentifyAgent;
        #endregion
        #region Public Properties
        public bool Active { get => m_active; set => m_active = value; }
        public virtual string TypeID { get; }

        public ChemicalAgent Contaminant { get => m_contaminant; }
        public Vector3 Epicenter { get => m_epicenter; }

        public bool AgentIdentified { get => m_agentIdentified; }

        public bool MoveEnabled { get => m_moveEnabled; }
        public int TotalSamples { get => m_totalSamples; }

        public Collider Collider { get => m_collider; }
        public Bounds Bounds { get => m_mesh.bounds; }

        public SampleTool Tool { get; set; }

        public ToolType CurrentToolType { get; set; }

        public System.Action<bool> OnSetMovable { get => m_onSetMovable; set => m_onSetMovable = value; }

        public System.Action OnIdentifyAgent { get => m_onIdentifyAgent; set => m_onIdentifyAgent = value; }
        #endregion

        #region Initialization
        public virtual void Init()
        {
            // Cache components
            if (!m_collider) TryGetComponent<Collider>(out m_collider);
            /*
            // Initialize the grid
            InitSampleGrid();
            if (m_activeOnStart) ShowSampleGrid();
            */
        }

        public void Init(ChemicalAgent _chemical)
        {
            // Assign contaminant
            m_contaminant = _chemical;
            // Call normal functionality
            Init();
        }
        #endregion

        #region Update
        protected virtual void Update()
        {
            if (m_toolCollisionMarker != null)
            {
                Vector3 collisionPoint = m_collider.ClosestPointOnBounds(m_currentTrackedTool.transform.position);
                m_toolCollisionMarker.transform.localPosition = transform.InverseTransformPoint(collisionPoint);
            }

            if (!m_moveEnabled || !m_pointerInBounds || m_currentInteractor == null) return;

            RaycastHit hit;
            if(m_currentInteractor.TryGetCurrent3DRaycastHit(out hit))
            {
                hitObject = hit.collider.gameObject;
                Vector3 hitPosition = new Vector3(hit.point.x, hit.point.y, transform.position.z);
                if (m_parentMesh != null)
                {
                    Bounds futureBounds = new Bounds(new Vector3(hitPosition.x, hitPosition.y, m_parentMesh.transform.position.z), m_mesh.bounds.size);
                    float x = Mathf.Clamp(hit.point.x, (m_parentMesh.bounds.min.x + Bounds.extents.x), (m_parentMesh.bounds.max.x - Bounds.extents.x));
                    float y = Mathf.Clamp(hit.point.y, (m_parentMesh.bounds.min.y + Bounds.extents.y), (m_parentMesh.bounds.max.y - Bounds.extents.y));
                    //Vector3 clampedPosition = m_parentMesh.transform.InverseTransformPoint(KeepInBounds(futureBounds));
                    hitPosition = new Vector3(x, y, hitPosition.z);
                }
                transform.position = hitPosition;
            }
            else
            {
                hitObject = null;
            }
            
        }
        #endregion

        #region Show/Hide Area Functionality
        public virtual void ConfigureSampleArea(ScenarioStep _newStep)
        {
            if (_newStep.EnableSampleArea && !Active)
                ShowArea();
            else if (!_newStep.EnableSampleArea && Active)
                HideArea();
        }
        /*
        public void ConfigureSampleGrid(ScenarioStep _newStep)
        {
            if (_newStep.EnableSampleArea && !Active)
                ShowSampleGrid();
            else if (!_newStep.EnableSampleArea && Active)
                HideSampleGrid();
        }
        */

        public virtual void ShowArea()
        {
            // Set active
            Active = true;
        }

        public virtual void HideArea()
        {
            // Set inactive
            Active = false;
        }

        /*
        public void ToggleSampleGrid()
        {
            if (!Active)
                ShowSampleGrid();
            else
                HideSampleGrid();
        }
        */
        #endregion

        #region Movement Functionality
        public void ToggleMove()
        {
            if (!m_moveEnabled)
                EnableMove();
            else
                DisableMove();
        }


        public void EnableMove()
        {
            // Make sure pointer was in sample bounds
            if (m_moveEnabled || !m_pointerInBounds) return;
            m_moveEnabled = true;
            m_onSetMovable?.Invoke(true);
            UnityEngine.Debug.Log($"Enabled sample area movement || Time: {Time.time}");
        }
        public void EnableMove(ActivateEventArgs args)
        {
            EnableMove();
            if (args.interactor is L58XRHand hand)
            {
                m_currentInteractor = hand;
                m_currentInteractorActionMap = SimulationManager.Instance.GetHandMap(hand);
            }
                
        }

        public void DisableMove()
        {
            if (!m_moveEnabled) return;
            m_moveEnabled = false;
            if (m_currentInteractor != null)
            {
                m_currentInteractor = null;
            }
            m_onSetMovable?.Invoke(false);
            UnityEngine.Debug.Log($"Disabled sample area movement || Time: {Time.time}");
        }

        public Vector3 KeepInBounds(Bounds _futureBounds)
        {
            return _futureBounds.center;
            /*
            // Set up return values
            Vector3 boundCenter = _futureBounds.center;
            Vector3 adjustment = Vector3.zero;
            // Get all points
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(_futureBounds.min.x, _futureBounds.min.y, _futureBounds.center.z);
            vertices[1] = new Vector3(_futureBounds.min.x, _futureBounds.max.y, _futureBounds.center.z);
            vertices[2] = new Vector3(_futureBounds.max.x, _futureBounds.min.y, _futureBounds.center.z);
            vertices[3] = new Vector3(_futureBounds.max.x, _futureBounds.max.y, _futureBounds.center.z);
            List<Vector3> translationVectors = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                if (!m_parentMesh.bounds.Contains(vertex))
                {
                    Vector3 baseDirection = vertex - boundCenter;
                    Vector3 closestPoint = m_parentMesh.bounds.ClosestPoint(vertex);

                    Vector3 directionVector = boundCenter + adjustment;
                    Vector3 pointAdjustment = closestPoint - vertex;

                    adjustment = pointAdjustment;
                    UnityEngine.Debug.Log($"Vertex [{i}] not within mesh bounds. Position: {vertex} | Closest Point: {closestPoint}| Direction: {baseDirection} || Time: {Time.time}");

                }
            }
            return boundCenter + adjustment;*/
        }
        public void OnPointerEnter(HoverEnterEventArgs args)
        {
            m_pointerInBounds = true;
            //UnityEngine.Debug.Log($"Pointer entered bounds || Time: {Time.time}");
        }

        public void OnPointerExit(HoverExitEventArgs args)
        {
            m_pointerInBounds = false;
            //UnityEngine.Debug.Log($"Pointer exited bounds || Time: {Time.time}");
        }
        #endregion

        #region Event-Related Functionality
        public void IdentifyAgent()
        {
            if (m_agentIdentified) return;
            m_agentIdentified = true;
            m_onIdentifyAgent?.Invoke();
        }
        #endregion

        #region Helper Methods
        public SampleUnitObject CreateSampleUnitObject(SampleUnit unit)
        {
            SampleUnitObject unitObject = Instantiate(m_sampleUnitPrefab);
            unitObject.transform.localScale = m_itemSize;
            unitObject.transform.parent = m_mesh.transform;
            unitObject.transform.localEulerAngles = Vector3.zero;
            unitObject.transform.localPosition = unit.Position;
            unitObject.gameObject.name = $"SampleUnit[{unit.Index.x}, {unit.Index.y}]";
            unitObject.Init(unit);
            return unitObject;
        }
        public bool UnitInSampleBounds(Vector2 _unitIndex)
        {
            return ((_unitIndex.x - sampleCenter.x) * (_unitIndex.x - sampleCenter.x) + (_unitIndex.y - sampleCenter.y) * (_unitIndex.y - sampleCenter.y)) <= m_sampleRadius * m_sampleRadius;
        }
        #endregion
        /*
        private void OnTriggerEnter(Collider other)
        {
            if (m_toolCollisionMarker == null) 
            { 
                m_toolCollisionMarker = Instantiate(m_toolCollisionMarkerPrefab, transform);
                m_currentTrackedTool = other.gameObject;
                Vector3 collisionPoint = m_collider.ClosestPointOnBounds(m_currentTrackedTool.transform.position);
                m_toolCollisionMarker.transform.localPosition = transform.InverseTransformPoint(collisionPoint);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_toolCollisionMarker != null) 
            {
                m_currentTrackedTool = null;
                Destroy(m_toolCollisionMarker);
            }

        }
        */
    }
}