using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class VaporCloud : MonoBehaviour
    {
        #region Protected Variables
        protected ChemicalContaminantSite m_parent;
        protected float m_radius;
        protected SphereCollider m_collider;

        protected Vector3 m_epicenter;
        #endregion
        #region Public Properties
        public ChemicalContaminantSite Parent { get => m_parent; }
        public ChemicalAgent Contaminant { get => m_parent.Agent; }

        public Vector3 Epicenter { get => m_epicenter; }
        public float Radius { get => m_radius; }

        #endregion

        #region Initialization
        public virtual void Init(ChemicalContaminantSite _parent, float _radius)
        {
            // Cache parent reference
            m_parent = _parent;
            // Get the collider component
            m_collider = gameObject.GetComponent<SphereCollider>();
            // Cache radius
            m_radius = _radius;
            m_collider.radius = m_radius;

            // Set epicenter
            m_epicenter = m_parent.PointOfOrigin;
        }
        #endregion


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

