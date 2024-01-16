using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ContaminantContainerSite : ChemicalContaminantSite
    {
        #region Inspector Assigned Variables
        [Header("Container References")]
        [SerializeField] List<ChemicalObject> m_possibleChemObjects;
        #endregion
        #region Protected Variables
        protected ChemicalObject m_chemObject;
        #endregion
        #region Public Properties
        public ChemicalObject ChemObject { get => m_chemObject; }
        #endregion

        #region Initialization
        public override void InitContaminatedObjects()
        {
            if (!m_chemObject) return;
            m_chemObject.gameObject.SetActive(true);
            m_chemObject.Init(m_agent, m_spillAmount);
            base.InitContaminatedObjects();
        }
        protected override MatterType GetSpillType()
        {
            return m_chemObject.Type;
        }

        protected override void CacheAffectedColliders()
        {
            if (!m_chemObject) return;

            // Set point of origin
            SetOrigin(m_chemObject.transform.position);
            // Initialize collider list
            m_affectedColliders = new List<Collider>();
            Collider originCollider = m_chemObject.GetComponent<Collider>();
            Contamination originContamination = originCollider.gameObject.GetComponent<Contamination>();
            if (!originContamination) originContamination = originCollider.gameObject.AddComponent<Contamination>();
            originContamination.Init(m_agent, GetSpillType(), m_spillAmount);
            m_affectedColliders.Add(originCollider);
            m_contaminatedObjects.Add(originCollider, originContamination);
        }
        #endregion

        #region Spawn-Related Functionality
        public ChemicalObject GetRandomContainer()
        {
            if (m_possibleChemObjects == null || m_possibleChemObjects.Count < 1) return null;
            // Select a chemical object
            ChemicalObject source = m_possibleChemObjects[Random.Range(0, m_possibleChemObjects.Count)];
            m_chemObject = source;
            if (!source.gameObject.activeInHierarchy) source.gameObject.SetActive(true);
            return source;
        }
        public override ChemicalAgent SelectChemicalFromDistribution(ChemicalDistribution _distribution)
        {
            // Get a random container
            if (GetRandomContainer() == null) return null;
            if (m_chemObject.Agent == null && m_chemObject.PresetAgent != null) m_chemObject.Agent = m_chemObject.PresetAgent;
            UnityEngine.Debug.Log($"{gameObject.name} selected chemical container object: {m_chemObject.gameObject.name} | Agent: {m_chemObject.Agent} || Time: {Time.time}");
            // Return the chemical agent at this spot
            return m_chemObject.Agent;
        }
        #endregion
    }

}
