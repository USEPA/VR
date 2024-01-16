using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class AvatarIK : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("IK Solver References")]
        [SerializeField] private IKHead m_headIK;
        [SerializeField] private IKHand m_rightHandIK;
        [SerializeField] private IKHand m_leftHandIK;
        [SerializeField] private IKFootSolver m_rightLegIK;
        [SerializeField] private IKFootSolver m_leftLegIK;
        [Header("Avatar References")]
        [SerializeField] private List<SkinnedMeshRenderer> m_mesh;
        [SerializeField] private Transform m_hipBone;
        [SerializeField] private Transform m_backAttachPoint;
        [Header("Default Configuration")]
        [SerializeField] private bool m_showMeshByDefault = true;
        [SerializeField] private Material m_hideMaterial;
        #endregion
        #region Private Variables
        private List<Material> m_defaultMaterials;
        public const float m_defaultAvatarHeight = 1.728814f;
        #endregion
        #region Public Properties
        public Transform Hip { get => m_hipBone; }
        public Transform BackAttachPoint { get => m_backAttachPoint; }
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize the head IK solver
            m_headIK?.Init();
            // Initialize both hand IK solvers
            m_rightHandIK.Init();
            m_leftHandIK.Init();
            // Initialize both leg IK solvers
            m_rightLegIK.Init();
            m_leftLegIK.Init();

            // Initialize the mesh
            InitMesh();
        }

        public void InitMesh()
        {
            // Initialize mesh materials
            m_defaultMaterials = new List<Material>();
            // Check if there is a valid mesh to show/hide
            if (m_mesh != null && m_mesh.Count > 0)
            {
                for (int i = 0; i < m_mesh.Count; i++)
                {
                    // Add the material of this mesh
                    m_defaultMaterials.Add(m_mesh[i].material);
                }

                // Set whether or not the player mesh should be visible
                SetMeshVisibility(m_showMeshByDefault);
            }
        }
        #endregion

        private void Start()
        {
            //Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Helper Methods
        public void ScaleAvatarToHeight(float _height)
        {
            // Get the current height according to the scale
            //float currentHeight = m_defaultAvatarHeight * transform.localScale.y;
            // Compare the height to the desired one
            float scaleFactor = _height / m_defaultAvatarHeight;
            // Assign the scale
            transform.localScale = Vector3.one * scaleFactor;
        }
        public void SetMeshVisibility(bool value)
        {
            if (m_mesh == null || m_mesh.Count < 1) return;

            for (int i = 0; i < m_mesh.Count; i++) 
            {
                SkinnedMeshRenderer mesh = m_mesh[i];
                //mesh.enabled = value;
                Material targetMaterial = (value) ? m_defaultMaterials[i] : m_hideMaterial;
                mesh.material = targetMaterial;
            }
        }


        public void SetMeshLayer(int value = 0)
        {
            if (m_mesh == null || m_mesh.Count < 1) return;

            for (int i = 0; i < m_mesh.Count; i++)
            {
                m_mesh[i].gameObject.layer = value;
            }
        }


        #if UNITY_EDITOR
        [ContextMenu("Force Show Mesh")]
        public void ForceShowMesh()
        {
            SetMeshVisibility(true);
        }

        [ContextMenu("Force Hide Mesh")]
        public void ForceHideMesh()
        {
            SetMeshVisibility(false);
        }
        #endif
        #endregion
    }
}

