using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemLiquidContainer : ChemLiquidObject
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected MeshRenderer m_fillMesh;

        #endregion
        #region Protected Variables
        protected MeshRenderer m_containerMesh;
        #endregion
        #region Public Properties
        public float AlphaColor { get => m_containerMesh.material.color.a; }
        public override MeshRenderer Mesh => m_fillMesh;
        #endregion

        #region Initialization
        public override void Init(ChemicalAgent _agent, float _amount = 100.0f)
        {
            base.Init(_agent, _amount);

            if (Mesh != null) ColorMeshFromAmount();
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            m_containerMesh = GetComponent<MeshRenderer>();

            if (m_presetAgent) Init(m_presetAgent);
        }
    }
}

