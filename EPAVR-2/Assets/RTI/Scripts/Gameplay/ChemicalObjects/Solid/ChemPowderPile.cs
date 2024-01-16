using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemPowderPile : ChemSolidObject
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private MeshRenderer m_mesh;
        #endregion
        #region Public Properties
        public override MeshRenderer Mesh => m_mesh;
        #endregion

        #region Initialization
        public override void Init(ChemicalAgent _agent, float _amount = 100.0f)
        {
            // Call base functionality
            base.Init(_agent, _amount);
            // Colorize the mesh according to the chemical
            ColorMeshFromAmount();
            // Get a random offset
            //Vector2 textureOffset = new Vector2(Random.Range(m_offsetRange.x, m_offsetRange.y), Random.Range(m_offsetRange.x, m_offsetRange.y));
            //m_mesh.material.mainTextureOffset = textureOffset;
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

