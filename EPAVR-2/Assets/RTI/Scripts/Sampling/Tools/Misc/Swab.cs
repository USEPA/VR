using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class Swab : SampleCollector, ISampleContainer, IDisposable
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] Collider m_sampleCollider;
        [SerializeField] MeshRenderer m_mesh;
        
        [Header("Default Configuration")]
        [SerializeField] float m_collectAmountPercentage = 100.0f;
        [SerializeField] float m_maxContaminantAmount = 10.0f;
        [SerializeField] Color m_sampleMarkerColor;
        #endregion
        #region Protected Variables
        protected TriggerEventListener m_triggerListener;
        protected XRGrabInteractable m_interactable;
        #endregion
        #region Public Properties
        public ContainerProperties Properties { get => m_containerProperties; }

        public bool CanTransferSample => false;

        public XRGrabInteractable Interactable
        {
            get
            {
                if (!m_interactable) m_interactable = GetComponent<XRGrabInteractable>();
                return m_interactable;
            }
        }

        public System.Action OnDisposed { get; set; }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            XRGrabInteractable interactable = transform.GetComponentInParent<XRGrabInteractable>();
            if (interactable != null)
            {
                // Ignore collisions with the parent
                Physics.IgnoreCollision(GetComponent<Collider>(), interactable.GetComponent<Collider>(), true);
            }

            // Check if the trigger listener needs to be created
            if (!m_triggerListener)
            {
                m_triggerListener = m_sampleCollider.gameObject.AddComponent<TriggerEventListener>();
                m_triggerListener.Init();

                m_triggerListener.OnTriggerEntered += i => ProcessTriggerStay(i);
                m_triggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Sample-Related Functionality
        public void SetSample(Sample _sample)
        {
            if (m_currentSample != null) return;

            // Copy sample
            m_currentSample = new Sample(_sample);

            if (m_mesh)
            {
                m_mesh.gameObject.SetActive(true);
                m_mesh.material.color = m_sampleMarkerColor;
            }
        }

        public void ClearSample()
        {
            if (m_currentSample == null) return;
            // Set mesh
            if (m_mesh != null) m_mesh.gameObject.SetActive(false);
            // Clear sample reference
            m_currentSample = null;
        }
        #endregion

        #region Collision-Related Functionality
        public void ProcessTriggerStay(Collider other)
        {
            // Check if a sample has not yet been collected
            if (m_currentSample == null)
            {
                /*
                Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);
                Contamination contamination = GetClosestContamination(other, closestPoint);
                if (contamination != null)
                {
                    float concentration = contamination.GetConcentrationFromPoint(closestPoint);
                    UnityEngine.Debug.Log($"{gameObject.name} - collected sample: {contamination.Agent} | Source: {contamination.gameObject.name} | Concentration: {concentration} || Time: {Time.time}");
                    // Take a sample from this object
                    SetSample(new Sample(contamination, concentration));
                }
                */
                // Try to get a sample from this object
                if (other.TryGetComponent<Contamination>(out Contamination contamination) && m_containerProperties.CanHoldType(contamination.Type))
                {
                    float concentration = contamination.GetConcentrationFromPoint(other.ClosestPointOnBounds(transform.position));
                    if (concentration > 0.0f)
                    {
                        UnityEngine.Debug.Log($"{gameObject.name} - collected sample: {contamination.Agent} | Source: {contamination.gameObject.name} | Concentration: {concentration} || Time: {Time.time}");
                        // Take a sample from this object
                        SetSample(new Sample(contamination, concentration));
                    }
                }
            }
            // Try to get a sample container component
            else if (other.TryGetComponent<ISampleContainer>(out ISampleContainer sampleContainer) && sampleContainer.CurrentSample == null && m_containerProperties.CanHoldType(m_currentSample.Type))
            {
                UnityEngine.Debug.Log($"{gameObject.name} - found sample container: {other.gameObject.name} || Time: {Time.time}");
                // Copy the sample to this container
                sampleContainer.SetSample(m_currentSample);
                //ClearSample();
            }
        }

        public void ProcessTriggerExit(Collider other)
        {
            // Process collision
        }
        #endregion

        public void Dispose()
        {
            OnDisposed?.Invoke();
        }
    }
}

