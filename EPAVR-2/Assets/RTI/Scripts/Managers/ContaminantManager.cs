using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ContaminantManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] private LayerMask m_liquidSpillCastMask;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;
        private bool m_paintEnabled = true;
        #endregion
        #region Public Properties
        public static ContaminantManager Instance { get; set; }
        public ManagerStatus Status => m_status;
        #endregion

        #region Initialization
        void Awake()
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
        }

        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            if (CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt)
            {
                m_paintEnabled = (PaintManager.Instance != null) ? PaintManager.Instance.Enabled : false;
                // Check for contaminant sites
                List<ContaminationSite> contaminantSites = ScenarioManager.Instance.CurrentScenarioInstance.ActiveSites;
                
                if (contaminantSites != null && contaminantSites.Count > 0)
                {
                    foreach (ChemicalContaminantSite site in contaminantSites)
                    {
                        InitSite(site);
                    }
                }
            }
            // Finish initialization
            UnityEngine.Debug.Log($"ContaminantManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion

        #region Site-Related Functionality
        void InitSite(ChemicalContaminantSite _site)
        {
            // Check the gamemode type
            switch (_site.Gamemode)
            {
                case Gamemode.ChemicalHunt:
                    // Get the spill type
                    MatterType spillType = _site.SpillType;
                    switch (spillType)
                    {
                        case MatterType.Liquid:
                            GenerateLiquidSpill(_site);
                            break;
                        case MatterType.Solid:
                            break;
                    }
                    // Get/initialize all contaminated objects contained within this site
                    _site.InitContaminatedObjects();
                    break;
                case Gamemode.RadiationSurvey:
                    ((RadSite)_site).InitRadCloud(50.0f);
                    break;
            }
        }
        #endregion

        #region Spill Generation-Related Functionality
        void GenerateLiquidSpill(ChemicalContaminantSite _site, float _strength = 1.0f, float _hardness = 0.15f)
        {
            if (!CoreGameManager.Instance.UsePaintSystem) return;
            // Get the radius
            float radius = _site.SpillRadius;
            // Generate a random point
            Vector3 randomOrigin = _site.transform.position;
            bool overrideOrigin = true;
            //Vector3 randomOrigin = _site.transform.position + (Random.insideUnitSphere * radius);
            if (_site is ContaminantLeakSite leakSite)
            {
                randomOrigin = leakSite.GetRandomLeakPoint();
                overrideOrigin = false;
            }
            else
            {
                randomOrigin = _site.transform.position + (Random.insideUnitSphere * radius);
            }

            Vector3 origin = randomOrigin;
            Paintable closestPaintable = null;
            Collider closestCollider = null;
            float closestDistance = float.MaxValue;
            Collider[] potentialColliders = Physics.OverlapSphere(origin, radius, m_liquidSpillCastMask);
            UnityEngine.Debug.Log($"ContaminantManager - GenerateLiquidSpill: {_site.gameObject.name} | Agent: {_site.Agent.Name} | Radius: {radius} | Found Colliders: {((potentialColliders != null) ? potentialColliders.Length : 0)} || Time: {Time.time}");
            if (potentialColliders == null || potentialColliders.Length < 1) return;
            foreach(Collider col in potentialColliders)
            {
                Paintable p = null;
                // Get the closest point to the random origin
                Vector3 closestColPoint = col.ClosestPoint(randomOrigin);
               
                Vector3 closestPoint = closestColPoint;

                if (m_paintEnabled)
                {
                    // First off, try to get a paintable component from this
                    p = PaintManager.Instance.GetPaintable(col);
                    if (p == null) continue;

                    if (col is MeshCollider meshCollider && !meshCollider.convex && p.Renderer != null)
                    {
                        SampleableMesh sampleMesh = p.gameObject.GetComponent<SampleableMesh>();
                        if (sampleMesh == null) sampleMesh = p.gameObject.AddComponent<SampleableMesh>();
                        UnityEngine.Debug.Log($"{_site.gameObject.name} getting nearest point on mesh: {p.gameObject.name} || Time: {Time.time}");
                        closestPoint = sampleMesh.GetNearestPoint(randomOrigin);
                    }

                }

                // Get the distance to this point
                float distance = MathHelper.QuickDistance(randomOrigin, closestPoint);
                if (distance < closestDistance)
                {
                    closestPaintable = p;
                    closestCollider = col;
                    closestDistance = distance;
                    UnityEngine.Debug.Log($"{_site.gameObject.name} found new closest point on: {closestCollider.gameObject.name} | Point: {closestPoint} | Distance: {closestDistance} || Time: {Time.time}");

                    if (overrideOrigin) origin = closestPoint;
                }
            }

            if (closestPaintable) _site.ClosestPaintable = closestPaintable;

            UnityEngine.Debug.Log($"ContaminantManager - GenerateLiquidSpill: {_site.gameObject.name} | Closest Paintable: {((closestPaintable != null) ? closestPaintable.gameObject.name : "N/A")} | Origin: {origin} | Agent: {_site.Agent.Name} || Time: {Time.time}");
            GameObject testOriginMarker = new GameObject();
            testOriginMarker.name = $"{_site.gameObject.name} Closest Point Marker";
            testOriginMarker.transform.parent = _site.transform;
            testOriginMarker.transform.localEulerAngles = Vector3.zero;
            testOriginMarker.transform.position = origin;

            GameObject randOriginMarker = new GameObject();
            randOriginMarker.name = $"{_site.gameObject.name} Random Point Marker";
            randOriginMarker.transform.parent = _site.transform;
            randOriginMarker.transform.localEulerAngles = Vector3.zero;
            randOriginMarker.transform.position = randomOrigin;
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
                        if (!overrideOrigin) origin = closestPoint;
                    }
                }
            }
            */

            if (m_paintEnabled && closestPaintable == null)
            {
                UnityEngine.Debug.Log($"ContaminantManager - ERROR: Failed to find closest paintable for site: {_site.gameObject.name} || Time: {Time.time}");
                return;
            }
            // Get the agent color
            Color spillColor = _site.Agent.SpawnColor;
            Vector3 rayOrigin = origin + ((_site.transform.position - origin)*10.0f);
            Ray ray = new Ray(rayOrigin, (origin - rayOrigin));
            //Ray ray = new Ray(randomOrigin, (origin - randomOrigin).normalized);
            RaycastHit hit;
            // Override origin for the site
            //_site.SetOrigin(origin);
           
            if (closestCollider.Raycast(ray, out hit, 100.0f))
            {
                if (!overrideOrigin && origin != hit.point)
                {
                    origin = hit.point;
                }
                // Override origin for the site
                _site.SetOrigin(origin);
            
                if (m_paintEnabled)
                {
                    // Paint the source object
                    Paint(closestPaintable, origin, spillColor, radius, _strength, _hardness);
                    _site.PaintedObjects.Add(closestPaintable);
                    float adjustedParentRadius = radius * 0.7783056587f;
                    Collider[] colliders = Physics.OverlapSphere(origin, adjustedParentRadius);
                    //UnityEngine.Debug.Log($"{gameObject.name} found colliders: {colliders.Length} | Site: {m_presetSite.gameObject.name} | Radius: {adjustedParentRadius} || Time: {Time.time}");
                    if (colliders != null && colliders.Length > 1)
                    {
                        foreach (Collider collider in colliders)
                        {
                            if (collider == closestCollider) continue;
                            if (collider.TryGetComponent<Paintable>(out Paintable paintable))
                            {
                                // Get the closest point on the collider to the origin
                                Vector3 closestPoint = collider.ClosestPoint(origin);
                                if (collider is MeshCollider meshCollider && !meshCollider.convex && paintable.Renderer != null)
                                {

                                    SampleableMesh sampleMesh = paintable.gameObject.GetComponent<SampleableMesh>();
                                    if (sampleMesh == null) sampleMesh = paintable.gameObject.AddComponent<SampleableMesh>();
                                    closestPoint = sampleMesh.GetNearestPoint(origin);
                                }
                                // Get the distance to origin
                                float originDistance = MathHelper.QuickDistance(origin, closestPoint);
                                float scale = (1 - (originDistance / radius));
                                // Get the adjusted radius
                                float adjustedRadius = scale * radius;
                                //UnityEngine.Debug.Log($"{gameObject.name} Painting: {paintable.gameObject.name} | Distance: {originDistance} | Parent Radius: {adjustedParentRadius} | Scale: {scale} | Adjusted Radius: {adjustedRadius} || Time: {Time.time}");

                                // Paint the object
                                Paint(paintable, closestPoint, spillColor, adjustedRadius, _strength, _hardness);
                                _site.PaintedObjects.Add(paintable);
                                //m_affectedObjects.Add(paintable);
                            }
                        }
                    }
                }
   
                UnityEngine.Debug.Log($"ContaminantManager initialized liquid spill: {_site.gameObject.name} | Agent: {_site.Agent} | Radius: {radius} || Time: {Time.time}");
            }
            else
            {
                UnityEngine.Debug.Log($"ContaminantManager - ERROR: Failed raycast for liquid spill: {_site.gameObject.name} | Closest Collider: {closestCollider.gameObject.name} | Closest Contact Point: {origin} | Radius: {radius} || Time: {Time.time}");
            }
        }


        public void Paint(Paintable p, Vector3 _pos, Color _color, float _radius, float _strength, float _hardness)
        {
            // Check if the paint system is enabled
            // Initialize the paintable if necessary
            if (!p.IsInitialized) PaintManager.Instance.InitPaintable(p);

            UnityEngine.Debug.Log($"Painting {p.gameObject.name} | Point: {_pos} | Radius: {_radius} | Strength: {_strength} | Color: {_color} || Time: {Time.time}");
            PaintManager.Instance.Paint(p, _pos, _radius, _hardness, _strength, _color);
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin resetting
            m_status = ManagerStatus.Resetting;
            // Finish reset
            UnityEngine.Debug.Log($"ContaminantManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }
        #endregion
    }
}

