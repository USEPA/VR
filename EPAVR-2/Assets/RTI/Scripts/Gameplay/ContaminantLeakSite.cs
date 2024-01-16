using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ContaminantLeakSite : ChemicalContaminantSite
    {
        #region Inspector Assigned Variables
        [Header("Object References")]
        [SerializeField] protected List<ChemicalObject> m_possibleLeakSources;
        #endregion
        #region Protected Variables
        public ChemicalObject m_chosenLeakSource;
        protected GameObject m_leakMarker;
        #endregion
        #region Public Properties
        public GameObject LeakMarker
        {
            get
            {
                if (!m_leakMarker)
                {
                    m_leakMarker = new GameObject();
                    m_leakMarker.name = $"{m_chosenLeakSource.gameObject.name} Spill - {m_agent.Name}";
                    m_leakMarker.transform.parent = transform;
                    m_leakMarker.transform.localEulerAngles = Vector3.zero;
                    m_leakMarker.transform.position = Vector3.zero;
                }
                return m_leakMarker;
            }
        }
        #endregion

        #region Initialization
        protected override MatterType GetSpillType()
        {
            return m_chosenLeakSource.Type;
        }
        protected override void CacheAffectedColliders()
        {
            // Make sure leak source is chosen
            if (m_chosenLeakSource.TryGetComponent<Collider>(out Collider col))
            {
                // Get the center
                Vector3 center = col.bounds.center;
                // Get the closest point to this in the collider
                Vector3 origin = col.ClosestPointOnBounds(center);
                // Get colliders based on this origin
                GetAffectedObjects(origin);
            }
        }

        public Vector3 GetRandomLeakPoint()
        {
            Vector3 origin = transform.position;
            if (m_chosenLeakSource != null && m_chosenLeakSource.TryGetComponent<Collider>(out Collider col))
            {
                origin = GameObjectHelper.GetRandomPointOnMesh(col);
            }
            return origin;
        }

        public override void SetOrigin(Vector3 _origin)
        {
            base.SetOrigin(_origin);
            LeakMarker.transform.position = _origin;
        }
        #endregion
        #region Spawn-Related Functionality
        public ChemicalObject GetRandomLeakSource()
        {
            if (m_possibleLeakSources == null || m_possibleLeakSources.Count < 1) return null;
            // Select a chemical object
            ChemicalObject source = m_possibleLeakSources[Random.Range(0, m_possibleLeakSources.Count)];
            m_chosenLeakSource = source;
            return source;
        }
        public override ChemicalAgent SelectChemicalFromDistribution(ChemicalDistribution _distribution)
        {
            // Get a random leak source
            if (GetRandomLeakSource() == null) return null;
            if (m_chosenLeakSource.Agent == null && m_chosenLeakSource.PresetAgent != null) m_chosenLeakSource.Agent = m_chosenLeakSource.PresetAgent;
            UnityEngine.Debug.Log($"{gameObject.name} selected chemical leak object: {m_chosenLeakSource.gameObject.name} | Agent: {m_chosenLeakSource.Agent} || Time: {Time.time}");
            // Return the chemical agent at this spot
            return m_chosenLeakSource.Agent;
        }
        #endregion
    }
}

