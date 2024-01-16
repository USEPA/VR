using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemicalContaminantSite : ContaminationSite
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected VaporCloud m_vaporCloudPrefab;
        [SerializeField] protected Contamination m_presetOrigin;
        [SerializeField] protected ChemicalObject m_presetObject;
        [Header("Default Configuration")]
        [SerializeField] protected float m_spillAmount = 100.0f;
        [SerializeField] protected Vector2 m_spreadSizeRange = new Vector2(0.5f, 2.0f);
        [SerializeField] protected LayerMask m_potentialObjectMask = 1;
        [SerializeField] protected ChemicalAgent m_defaultAgent;
        [SerializeField] protected List<ChemicalAgent> m_spawnableAgents;
        #endregion
        #region Protected Variables
        protected ChemicalAgent m_agent;
        protected MatterType m_spillType;

        public float m_spillRadius;
        protected Vector3 m_pointOfOrigin;
        protected VaporCloud m_vaporCloud;

        public List<Paintable> m_paintedObjects;
        public List<Collider> m_affectedColliders;

        public Dictionary<Collider, Contamination> m_contaminatedObjects;

        public Paintable ClosestPaintable;
        #endregion
        #region Public Properties
        public override Gamemode Gamemode => Gamemode.ChemicalHunt;
        public ChemicalAgent Agent { get => m_agent; }
        public MatterType SpillType { get => m_spillType; }

        public Vector2 SpreadSizeRange { get => m_spreadSizeRange; }
        public Vector3 PointOfOrigin { get => m_pointOfOrigin; }

        public float SpillRadius { get => m_spillRadius; }

        public VaporCloud VaporCloud { get => m_vaporCloud; }

        public Action<Contamination, Sample> OnSiteCleared { get => m_onSiteCleared; set => m_onSiteCleared = value; }

        public List<Paintable> PaintedObjects
        {
            get
            {
                if (m_paintedObjects == null) m_paintedObjects = new List<Paintable>();
                return m_paintedObjects;
            }
        }
        #endregion

        #region Initialization
        public virtual void Init(ChemicalAgent _agent)
        {
            // Make sure agent has valid spawn options
            if (_agent.SpawnableTypes == null || _agent.SpawnableTypes.Count < 1) return;
            // Assign agent reference
            m_agent = (m_presetOrigin != null) ? m_presetOrigin.Agent : _agent;
            // Determine what kind of spill this is
            m_spillType = GetSpillType();

            // Get spill radius
            m_spillRadius = UnityEngine.Random.Range(m_spreadSizeRange.x, m_spreadSizeRange.y);

            // Set default origin
            SetOrigin(transform.position);
            // Initialize contaminated objects
            //InitContaminatedObjects();
            UnityEngine.Debug.Log($"{gameObject.name} initialized: {m_agent.Name} || Time: {Time.time}");
        }

        public virtual void InitContaminatedObjects()
        {
            // Initialize the contaminated object registry
            m_contaminatedObjects = new Dictionary<Collider, Contamination>();
            // Get affected objects
            CacheAffectedColliders();

            // Create the vapor cloud
            m_vaporCloud = Instantiate(m_vaporCloudPrefab, transform).GetComponent<VaporCloud>();
            m_vaporCloud.transform.position = m_pointOfOrigin;
            m_vaporCloud.transform.rotation = Quaternion.identity;
            m_vaporCloud.Init(this, m_spillRadius);
            m_presetObject?.Init(m_agent, m_spillAmount);

            // Loop through each contaminated object and hook up events
            foreach (Contamination contamination in m_contaminatedObjects.Values)
            {
                // Hook up event
                contamination.OnIdentified += i => OnContaminantIdentified(contamination, i);
            }
            UnityEngine.Debug.Log($"{gameObject.name} initialized: {m_agent.Name} || Time: {Time.time}");
        }

        protected virtual void SetOrigin()
        {
            if (m_presetOrigin != null)
            {
                // Set point of origin
                m_pointOfOrigin = transform.position;
            }
            else
            {
                m_pointOfOrigin = transform.position;
            }
        }

        public virtual void SetOrigin(Vector3 _origin)
        {
            // Set point of origin
            m_pointOfOrigin = _origin;
            //transform.position = m_pointOfOrigin;
        }

        protected virtual MatterType GetSpillType()
        {
            return (m_presetOrigin != null) ? m_presetOrigin.Type : m_agent.SpawnableTypes[UnityEngine.Random.Range(0, m_agent.SpawnableTypes.Count)];
        }

        protected virtual void CacheAffectedColliders()
        {
            // Check if this is a preset leak
            if (m_presetOrigin != null)
            {
                // Set point of origin
                m_pointOfOrigin = transform.position;
                // Initialize collider list
                m_affectedColliders = new List<Collider>();
                Collider originCollider = m_presetOrigin.GetComponent<Collider>();
                m_affectedColliders.Add(originCollider);
                m_contaminatedObjects.Add(originCollider, m_presetOrigin);
            }
            else
            {
                // Cache affected colliders normally
                m_affectedColliders = GetAffectedObjects();
            }
        }
        #endregion

        #region Spawn-Related Functionality
        public virtual bool CanSpawnChemical(ChemicalAgent _agent)
        {
            return true;
        }

        public virtual ChemicalAgent SelectChemicalFromDistribution(ChemicalDistribution _distribution)
        {
            // Check if there is a preset chemical here
            if (m_defaultAgent != null) return m_defaultAgent;
            // Copy the distribution
            ChemicalDistribution currentDistribution = new ChemicalDistribution(_distribution);
            // Loop through each chemical item in the distribution and figure out if they can be spawned
            for(int i = 0; i < currentDistribution.Items.Count; i++)
            {
                // Get the chemical
                ChemicalAgent chemical = currentDistribution.Items[i].Value;
                // Check if this chemical can be spawned there
                if (!(CanSpawnChemical(chemical))) currentDistribution.RemoveAt(i);
            }
            // Check if there are any possible chemicals left
            if (currentDistribution.Items.Count > 0)
            {
                // Draw a random chemical
                ChemicalAgent selectedChemical = currentDistribution.Draw();
                UnityEngine.Debug.Log($"{gameObject.name} selected chemical: {selectedChemical.Name} || Time: {Time.time}");
                return selectedChemical;
            }
            // None of the chemicals specified in the distribution can be spawned
            return null;
        }
        #endregion

        #region Contaminant-Related Functionality
        public List<Collider> GetAffectedObjects()
        {
            return GetAffectedObjects(transform.position);
        }
        public List<Collider> GetAffectedObjects(Vector3 _originOverride)
        {
            // Check if there are any overlapping objects
            Collider[] potentialObjects = Physics.OverlapSphere(_originOverride, m_spillRadius, m_potentialObjectMask);
            // Make sure there are potential objects
            if (potentialObjects.Length > 0)
            {
                //UnityEngine.Debug.Log($"{gameObject.name} arrived in GetAffectedObjects: {potentialObjects.Length} || Time: {Time.time}");
                // Set up variables
                List<Collider> affectedObjects = new List<Collider>();
                Collider originCollider = null;
                Vector3 pointOfOrigin = transform.position;
                float closestPotentialOriginDistance = float.MaxValue;

                for (int i = 0; i < potentialObjects.Length; i++)
                {
                    // Cache reference
                    Collider col = potentialObjects[i];
                    // Get the closest point from the current non-specified origin
                    //Vector3 closestColliderPoint = col.ClosestPoint(transform.position);
                    Vector3 closestColliderPoint = col.ClosestPointOnBounds(_originOverride);
                    // Get/compare distance
                    float distance = MathHelper.QuickDistance(_originOverride, closestColliderPoint);
                    if (distance < closestPotentialOriginDistance)
                    {
                        closestPotentialOriginDistance = distance;
                        originCollider = col;
                        pointOfOrigin = closestColliderPoint;
                    }

                    

                    affectedObjects.Add(col);
                }

                if (originCollider != null)
                {
                    // Order the affected object list by distance to point of Origin
                    List<Collider> finalList = affectedObjects.OrderBy(x => MathHelper.QuickDistance(pointOfOrigin, x.transform.position)).ToList();
                    for (int i = 0; i < finalList.Count; i++)
                    {
                        Collider col = finalList[i];
                        Vector3 closestPointToOrigin = col.ClosestPointOnBounds(pointOfOrigin);
                        float originDistance = MathHelper.QuickDistance(pointOfOrigin, closestPointToOrigin);
                        GameObject closestPointMarker = new GameObject();
                        closestPointMarker.name = $"[{i}] Closest Point: {originDistance}";
                        closestPointMarker.transform.parent = col.transform;
                        closestPointMarker.transform.position = closestPointToOrigin;
                        // Add the contamination value
                        float contaminantAmount = m_spillAmount * ((m_spillRadius - originDistance) / m_spillRadius);
                        Contamination contamination = col.gameObject.AddComponent<Contamination>();
                        contamination.Init(m_agent, m_spillType, contaminantAmount);
                        float maxLocalDistance = m_spillRadius - originDistance;
                        contamination.SetEpicenter(closestPointToOrigin, originDistance, maxLocalDistance);
                        m_contaminatedObjects.Add(col, contamination);

                        if (PaintManager.Instance && PaintManager.Instance.HasKey(col))
                        {
                            Paintable p = PaintManager.Instance.GetPaintable(col);
                            p.Contaminants.Add(contamination);
                        }
                    }
                    // Set point of origin reference
                    m_pointOfOrigin = pointOfOrigin;

                    return finalList;
                    /*
                    // Make this the origin
                    affectedObjects.Add(originCollider);
                    // Loop through each object
                    for (int i = 0; i < potentialObjects.Length; i++)
                    {
                        Collider col = potentialObjects[i];
                        if (col != originCollider) affectedObjects.Add(col);
                    }
                    */
                }
            }
            UnityEngine.Debug.Log($"ERROR: No potential objects found || Time: {Time.time}");
            return null;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            //Init(m_defaultAgent);
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region State-Related Functionality
        public void SetState(SiteStatus _status)
        {
            // Set state
            m_status = _status;
            switch (_status)
            {
                case SiteStatus.Contacted:
                    if (!m_firstContacted)
                    {
                        m_firstContacted = true;
                    }
                    break;
                case SiteStatus.Completed:
                    if (!m_isCleared)
                    {
                        m_isCleared = true;
                    }
                    break;
                default:
                    break;
            }
            // Invoke any necessary events
            m_onStateChange?.Invoke(_status);
        }

        public void FirstContact()
        {
            if (m_firstContacted) return;

            SetState(SiteStatus.Contacted);

        }
        #endregion

        #region Clear-Related Functionality
        public virtual void OnContaminantIdentified(Contamination _contamination, Sample _sample)
        {
            TryClearSite(_contamination, _sample);
        }

        public virtual void TryClearSite(Contamination _contamination, Sample _sample)
        {
            if (m_isCleared) return;
            ClearSite(_contamination, _sample);
        }

        public virtual void TryClearSite()
        {
            if (m_isCleared) return;
            ClearSite();
        }

        public virtual void ClearSite()
        {
            // Set clear state
            SetState(SiteStatus.Completed);
            // Invoke any attached events
            OnCleared?.Invoke();
        }
        public virtual void ClearSite(Contamination _contamination, Sample _sample)
        {
            ClearSite();
            // Disable the vapor cloud
            if (m_vaporCloud) m_vaporCloud.gameObject.SetActive(false);
            // Invoke necessary events
            m_onSiteCleared?.Invoke(_contamination, _sample);
        }
        #endregion
    }

    public enum SiteStatus { Idle, Contacted, Completed}
}