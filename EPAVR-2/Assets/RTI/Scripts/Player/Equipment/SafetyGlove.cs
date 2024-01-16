using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class SafetyGlove : MonoBehaviour, IDisposable
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected Material m_gloveMat;
        #endregion
        #region Protected Variables
        protected XRGrabInteractable m_interactable;
        protected MeshRenderer m_interactableMesh;

        protected XRHand m_targetHand;
        protected SkinnedMeshRenderer m_handMesh;
        protected List<ChemicalAgent> m_contaminants;
        #endregion
        #region Public Properties
        public XRGrabInteractable Interactable => m_interactable;
        public System.Action OnDisposed { get; set; }

        public List<ChemicalAgent> Contaminants { get => m_contaminants; }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Try to get the interactable components
            if (TryGetComponent<XRGrabInteractable>(out m_interactable))
            {
                // Get the interactable mesh
                m_interactableMesh = m_interactable.GetComponentInChildren<MeshRenderer>();
                // Initialize the socket item
                if (m_interactable.TryGetComponent<XRToolItem>(out XRToolItem socketItem))
                {
                    socketItem.Init(m_interactable);
                }
            }
        }
        public void Init(XRHand _target)
        {
            // Cache references
            m_targetHand = _target;
            // Check for hand mesh
            if (m_targetHand.Mesh != null)
            {
                // Cache the mesh reference and apply the material
                m_handMesh = m_targetHand.Mesh;
                // Apply the material
                m_targetHand.SetMaterial(m_gloveMat);
            }
            // Check for the interactable
            if (m_interactable != null)
            {
                // Disable the interactable's mesh
                m_interactableMesh.enabled = false;
            }
        }
        #endregion

        #region Contaminant-Related Functionality
        public void AddContaminant(Sample _sample)
        {
            if (_sample.Chemical == null) return;

            if (m_contaminants == null) m_contaminants = new List<ChemicalAgent>();

            if (!m_contaminants.Contains(_sample.Chemical))
            {
                // Add the contaminant
                m_contaminants.Add(_sample.Chemical);
                UnityEngine.Debug.Log($"{gameObject.name} was contaminated: {_sample.Chemical} || Time: {Time.time}");
            }
        }
        #endregion

        #region Helper Functionality
        public void ClearRefs()
        {
            // Clear hand references
            m_targetHand = null;
            m_handMesh = null;
            // Check for the interactable
            if (m_interactable != null)
            {
                // Re-enable the interactable's mesh
                m_interactableMesh.enabled = true;
            }
        }
        #endregion

        public void Dispose()
        {
            OnDisposed?.Invoke();
        }

        void OnDestroy()
        {
            //OnDisposed?.Invoke();
            if (m_contaminants != null) m_contaminants = null;
        }
    }

}
