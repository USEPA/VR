using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Scoopula : MonoBehaviour, ISampleContainer
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private MeshRenderer m_fillMesh;
        [Header("Default Configuration")]
        [SerializeField] private ContainerProperties m_containerProperties;
        #endregion
        #region Private Variables
        private Sample m_sample;
        #endregion
        #region Public Properties
        public Sample CurrentSample { get => m_sample; }

        public ContainerProperties Properties { get => m_containerProperties; }

        public bool CanTransferSample => false;
        #endregion


        #region Sample-Related Functionality
        public void SetSample(Sample _sample)
        {
            if (m_sample != null) return;

            // Copy sample
            m_sample = new Sample(_sample);
            m_fillMesh.gameObject.SetActive(true);
            if (m_sample.Chemical != null) m_fillMesh.material.color = m_sample.Chemical.SpawnColor;
        }

        public void ClearSample()
        {
            if (m_sample == null) return;
            // Clear sample reference
            m_sample = null;
            // Disable the fill object
            m_fillMesh.gameObject.SetActive(false);
        }
        #endregion

        #region Collision-Related Functionality
        public void OnTriggerStay(Collider other)
        {
            // Check if a sample has not yet been collected
            if (m_sample == null)
            {
                // Try to get a sample from this object
                if (other.TryGetComponent<Contamination>(out Contamination contamination))
                {
                    UnityEngine.Debug.Log($"{gameObject.name} contacted contaminated object: {contamination.gameObject.name} || Time: {Time.time}");
                    if (contamination.Type == MatterType.Solid)
                    {
                        // Take a sample from this object
                        SetSample(new Sample(contamination));
                    }
 
                }
            }
            // Try to get a sample container component
            else if (other.TryGetComponent<ISampleContainer>(out ISampleContainer sampleContainer) && sampleContainer.Properties.CanHoldType(MatterType.Solid))
            {
                // Make sure this sample container does not already have a sample
                if (sampleContainer.CurrentSample != null) return;
                // Transfer the sample to this container
                sampleContainer.SetSample(m_sample);
                ClearSample();
            }
        }

        public void OnTriggerExit(Collider other)
        {
            // Process collision
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

