using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MeshComponentHelper : MonoBehaviour
    {
        #region Public Variables
        [Header("Mesh Output")]
        public Vector3 m_meshBounds;
        public Vector3 m_meshCenter;
        [Header("Collider Output")]
        public Vector3 m_colliderBounds;
        public Vector3 m_colliderCenter;

        [Header("Configuration")]
        public PhysicMaterial m_desiredMaterial;
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        #region Helper Methods
        #if UNITY_EDITOR
        [ContextMenu("Get Mesh Bound Size")]
        public void GetMeshBounds()
        {
            if (TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
            {
                m_meshBounds = mesh.bounds.size;
                m_meshCenter = mesh.bounds.center;
                UnityEngine.Debug.Log($"{gameObject.name} bounds area: {MeshHelper.GetMeshBoundsArea(mesh)}");
            }
        }
        [ContextMenu("Get Collider Bound Size")]
        public void GetColliderBounds()
        {
            if (TryGetComponent<Collider>(out Collider col))
            {
                m_colliderBounds = col.bounds.size;
                m_colliderCenter = col.bounds.center;
            }
        }

        [ContextMenu("Name Children by ID")]
        public void AssignChildrenIDs()
        {
            if (transform.childCount < 1) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                string name = child.gameObject.name;
                if (name.Contains("_"))
                {
                    string[] substrings = name.Split('_');
                    name = substrings[1];
                }
                string id = i.ToString("00");
                child.gameObject.name = $"{id}_{name}";
            }
        }

        [ContextMenu("Assign Physics Material")]
        public void AssignPhysicsMaterial()
        {
            // Make sure physics material is set
            if (!m_desiredMaterial) return;
            // Get collider count
            Collider[] colliders = GetComponents<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                foreach(Collider col in colliders)
                {
                    col.material = m_desiredMaterial;
                }
            }
        }
#endif
        #endregion
    }
}

