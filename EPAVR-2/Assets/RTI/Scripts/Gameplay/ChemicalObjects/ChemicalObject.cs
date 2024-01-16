using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class ChemicalObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected ChemicalAgent m_presetAgent;
        #endregion
        #region Protected Variables
        protected ChemicalAgent m_agent;
        protected float m_amount;
        #endregion
        #region Public Properties
        public abstract MatterType Type { get; }
        public ChemicalAgent PresetAgent { get => m_presetAgent; }
        public ChemicalAgent Agent { get => m_agent; set => m_agent = value; }
        public float Amount { get => m_amount; }

        public virtual MeshRenderer Mesh { get; }
        #endregion

        #region Initialization
        public virtual void Init(ChemicalAgent _agent, float _amount = 100.0f)
        {
            // Cache references
            m_agent = _agent;
            m_amount = _amount;
        }
        #endregion

        #region Helper Methods
        public virtual void ColorMeshFromAmount()
        {
            if (!Mesh || m_agent.SpawnColor == null) return;
            // Get the alpha value based on the spill amount
            float alpha = m_agent.SpawnColor.a * Mathf.Clamp(m_amount / 100.0f, 0.0f, 1.0f);
            Color targetColor = m_agent.SpawnColor;
            targetColor.a = alpha;
            Mesh.material.color = targetColor;
            Mesh.material.SetColor("_Tint", targetColor);
            Mesh.material.SetColor("_TopColor", targetColor);
        }
        #endregion
    }

    public enum MatterType {Solid, Liquid, Gas}
}

