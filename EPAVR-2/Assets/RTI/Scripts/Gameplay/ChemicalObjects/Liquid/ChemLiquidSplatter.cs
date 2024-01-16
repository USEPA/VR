using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ChemLiquidSplatter : ChemLiquidObject
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected Collider m_collider;
        [SerializeField] protected MeshRenderer m_mesh;
        [Header("Spawn Configuration")]
        [SerializeField] ChemicalAgent m_defaultChemical;
        [SerializeField] protected Vector2 m_offsetRange;
        #endregion
        #region Private Variables
        #endregion
        #region Public Properties
        public Collider Collider { get => m_collider; }
        public MeshRenderer Mesh { get => m_mesh; }
        #endregion

        #region Initialization
        public override void Init(ChemicalAgent _agent, float _amount = 100.0f)
        {
            // Call base functionality
            base.Init(_agent, _amount);
            // Colorize the mesh according to the chemical
            Color targetColor = _agent.SpawnColor;
            //targetColor.a = m_mesh.material.color.a;
            targetColor.a = _agent.SpawnColor.a * Mathf.Clamp(m_amount / 100.0f, 0.0f, 1.0f);
            //targetColor.a = 0.1f;
            UnityEngine.Debug.Log($"{gameObject.name} = Chemical: {m_agent.Name} | Amount: {m_amount} | Spawn Color Alpha: {_agent.SpawnColor.a} | Target: {_agent.SpawnColor.a * Mathf.Clamp(m_amount / 100.0f, 0.0f, 1.0f)} || Time: {Time.time}");
            m_mesh.material.color = targetColor;
            // Get a random offset
            //Vector2 textureOffset = new Vector2(Random.Range(m_offsetRange.x, m_offsetRange.y), Random.Range(m_offsetRange.x, m_offsetRange.y));
            //m_mesh.material.mainTextureOffset = textureOffset;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            //if (m_defaultChemical != null) Init(m_defaultChemical);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

