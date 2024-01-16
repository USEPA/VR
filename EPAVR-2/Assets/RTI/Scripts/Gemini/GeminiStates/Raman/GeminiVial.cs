using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class GeminiVial : XRGrabInteractable, IGrabbable, ISampleContainer
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] GameObject m_lid;
        [SerializeField] MeshRenderer m_sampleFillModel;
        [Header("Default Configuration")]
        [SerializeField] ContainerProperties m_containerProperties;
        [Header("Debug Configuration")]
        [SerializeField] bool m_startOpen = false;
        #endregion
        #region Private Variables
        private GeminiRamanMode m_parent;
        private Sample m_sample;
        private SampleSurfaceArea m_sampleArea;
        private ChemicalAgent m_chemicalAgent;

        private bool m_isOpen;
        public bool m_atAttachPoint = false;
        protected Collider m_attachPointCollider;
        #endregion
        #region Public Properties
        public GeminiRamanMode Parent { get => m_parent; }
        public bool AtAttachPoint { get => m_atAttachPoint; set => m_atAttachPoint = value; }
        public Collider AttachPointCollider { get => m_attachPointCollider; set => m_attachPointCollider = value; }

        public Sample CurrentSample { get => m_sample; }
        public ContainerProperties Properties { get => m_containerProperties; }

        public bool IsOpen { get => m_isOpen; }

        public bool CanTransferSample => m_isOpen;
        #endregion

        #region Initialization
        public void Init(GeminiRamanMode _parent)
        {
            // Cache reference
            m_parent = _parent;
            // Ignore collision with the main Gemini collider
            Physics.IgnoreCollision(GetComponent<Collider>(), _parent.Parent.GetComponent<Collider>(), true);
        }
        #endregion

        void Start()
        {
            if (m_startOpen) SetOpen(true);
        }

        #region Sample-Related Functionality
        public void CollectSample(SampleSurfaceArea _area)
        {
            // Create sample
            SetSample(new Sample(_area, _area.Contaminant));
            // Cache references
            //m_sampleArea = _area;
            //if (m_sampleArea.Contaminant != null) m_chemicalAgent = m_sampleArea.Contaminant;
        }

        public void SetSample(Sample _sample)
        {
     
            // Make sure lid is open
            if (!m_isOpen) return;
            UnityEngine.Debug.Log($"Gemini Vial arrived in SetSample: {_sample.Chemical.Name} || Time: {Time.time}");
            // Copy sample
            m_sample = new Sample(_sample);
            m_sampleFillModel.gameObject.SetActive(true);
            if (m_sample.Chemical != null) m_sampleFillModel.material.color = m_sample.Chemical.SpawnColor;
        }

        public void ClearSample()
        {
            if (m_sample == null) return;
            m_sampleFillModel.gameObject.SetActive(false);
            m_sample = null;
        }
        #endregion

        #region Interaction-Related Functionality
        public void ToggleOpen()
        {
            SetOpen(!m_isOpen);
        }

        public void SetOpen(bool value)
        {
            // Set isOpen
            m_isOpen = value;
            // Enable/disable lid gameobject
            m_lid.SetActive(!value);
        }

        public void OnGrab()
        {
            // Check if this vial has been inserted
            if (m_parent.CurrentInsertedVial == this) m_parent.RemoveVial();
        }

        public void OnDrop()
        {
            // Check if the vial is at the insertion point
            if (m_atAttachPoint && m_attachPointCollider == m_parent.VialSlot.Collider) m_parent.InsertVial(this);
        }


        protected override void Grab()
        {
            // Call base functionality
            base.Grab();
            // Invoke any specific grab functionality
            OnGrab();
        }

        protected override void Drop()
        {
            // Call base functionality
            base.Drop();
            // Invoke any specific drop functionality
            OnDrop();
        }
        #endregion

        #region Collision-Related Functionality
        private void OnTriggerStay(Collider other)
        {
            switch (other.transform.tag)
            {
                case "SampleArea":
                    // Try to get sample area
                    if (m_sample == null && other.TryGetComponent<SampleSurfaceArea>(out SampleSurfaceArea sampleArea))
                    {
                        // Gather a sample
                        CollectSample(sampleArea);
                    }
                    break;
                case "AttachPoint":
                    // Check if this is the vial insert slot
                    if (other == m_parent.VialSlot.Collider)
                    {
                        m_atAttachPoint = true;
                        m_attachPointCollider = m_parent.VialSlot.Collider;
                    }
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            switch (other.transform.tag)
            {
                case "AttachPoint":
                    // Check if this is the vial insert slot
                    if (m_atAttachPoint && other == m_parent.VialSlot.Collider)
                    {
                        m_atAttachPoint = false;
                        m_attachPointCollider = null;
                    }
                    break;
            }
        }
        #endregion
    }
}

