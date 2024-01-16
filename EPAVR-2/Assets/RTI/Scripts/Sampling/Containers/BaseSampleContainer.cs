using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class BaseSampleContainer : MonoBehaviour, ISampleContainer
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected ChemicalModel m_solidFillMesh;
        [SerializeField] protected ChemicalModel m_liquidFillMesh;

        [SerializeField] protected MeshRenderer m_fillMesh;
        [Header("Default Configuration")]
        [SerializeField] protected ContainerProperties m_containerProperties;
        #endregion
        #region Protected Variables
        protected Sample m_sample;

        protected ChemicalModel m_activeModel;

        protected Action<Sample> m_onSetSample;
        protected Action m_onClearSample;
        #endregion
        #region Public Properties
        public Sample CurrentSample { get => m_sample; }

        public ContainerProperties Properties { get => m_containerProperties; }

        public Action<Sample> OnSetSample { get => m_onSetSample; set => m_onSetSample = value; }
        public Action OnClearSample { get => m_onClearSample; set => m_onClearSample = value; }

        public bool CanTransferSample => false;
        #endregion

        #region Sample-Related Functionality
        public void SetSample(Sample _sample)
        {
            if (m_sample != null) return;

            // Copy sample
            m_sample = new Sample(_sample);
            UnityEngine.Debug.Log($"{gameObject.name} SetSample: {_sample.Chemical} | Type: {_sample.Type} || Time: {Time.time}");
            // Get the mesh to enable
            m_activeModel = (_sample.Type == MatterType.Liquid) ? m_liquidFillMesh : m_solidFillMesh;
            
            m_activeModel.Mesh.gameObject.SetActive(true);
            if (m_sample.Chemical != null) m_activeModel.ConfigureModel(m_sample.Chemical);
            m_activeModel.ConfigureModel(_sample.Chemical);
        }

        public void ClearSample()
        {
            // Disable any current active model
            m_activeModel.Mesh.gameObject.SetActive(false);
            // Invoke any necessary events
            m_onClearSample?.Invoke();
            // Clear reference
            m_sample = null;
        }
        #endregion
    }

    [System.Serializable]
    public class ChemicalModel
    {
        #region Inspector Assigned Variables
        [SerializeField] protected MatterType m_matterType;
        [SerializeField] protected MeshRenderer m_mesh;
        #endregion
        #region Public Properties
        public MatterType Type { get => m_matterType; }
        public MeshRenderer Mesh { get => m_mesh; }
        #endregion

        #region Helper Methods
        public void ConfigureModel(ChemicalAgent _chemical)
        {
            // Set the color to match chemical
            m_mesh.material.color = _chemical.SpawnColor;
        }
        #endregion
    }
}

