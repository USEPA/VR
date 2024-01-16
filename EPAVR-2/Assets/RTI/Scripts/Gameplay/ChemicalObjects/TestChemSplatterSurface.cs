using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class TestChemSplatterSurface : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected ChemLiquidSplatter m_splatterPrefab;
        [Header("Default Configuration")]
        [SerializeField] protected ChemicalAgent m_defaultChemical;
        [SerializeField] protected Vector3 m_defaultPosition;
        #endregion
        #region Protected Variables
        protected BoxCollider m_collider;
        protected ChemLiquidSplatter m_splatter;

        public List<Vector3> validDirections;
        public Vector3 splatterExtents;
        public Vector2 xRange;
        public Vector2 yRange;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache components
            m_collider = GetComponent<BoxCollider>();
            // Instantiate the splatter
            m_splatter = Instantiate(m_splatterPrefab, transform);
            m_splatter.Init(m_defaultChemical);
            PositionSplatter();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SelectFace()
        {
            validDirections = new List<Vector3>();
            /*
            validDirections.Add(transform.forward);
            validDirections.Add(-transform.forward);
            validDirections.Add(transform.right);
            validDirections.Add(-transform.right);
            validDirections.Add(transform.up);
            validDirections.Add(-transform.up);
            */
            
            validDirections.Add(m_collider.center + new Vector3(m_collider.bounds.extents.x, 0.0f, 0.0f));
            validDirections.Add(m_collider.center + new Vector3(-m_collider.bounds.extents.x, 0.0f, 0.0f));
            validDirections.Add(m_collider.center + new Vector3(0.0f, m_collider.bounds.extents.y, 0.0f));
            validDirections.Add(m_collider.center + new Vector3(0.0f, -m_collider.bounds.extents.y, 0.0f));
            validDirections.Add(m_collider.center + new Vector3(0.0f, 0.0f, m_collider.bounds.extents.z));
            validDirections.Add(m_collider.center + new Vector3(0.0f, 0.0f, -m_collider.bounds.extents.z));

            for(int i = 0; i < validDirections.Count; i++)
            {
                // Get orthogonal vector
                Vector3 left = Vector3.Cross(validDirections[i], Vector3.up).normalized;
                // Determine if the splatter can fit within this
            }

        }

        public void PositionSplatter()
        {
            // Get the direction to the default position
            Vector3 faceDirection = transform.TransformDirection(m_defaultPosition);
            // Rotate the splatter to match this direction
            m_splatter.transform.rotation = Quaternion.LookRotation(faceDirection);

            // Get the bounds of the splatter
            splatterExtents = m_splatter.Collider.bounds.extents;

            // Determine random range
            //yRange = new Vector3(m_collider.center.z - m_collider.bounds.extents.z, m_collider.center.z + m_collider.bounds.extents.z);
            yRange = new Vector3(0.0f, m_collider.bounds.size.y);
            yRange.x += splatterExtents.z;
            yRange.y -= splatterExtents.z;

            xRange = new Vector3(-m_collider.bounds.extents.x, m_collider.bounds.extents.x);
            xRange.x += splatterExtents.x;
            xRange.y -= splatterExtents.x;
            //yRange.x = Mathf.Clamp(yRange.x, yRange.x - splatterExtents.y, yRange.x + splatterExtents.y);

            Vector3 targetPosition = m_defaultPosition;
            targetPosition.y = Random.Range(xRange.x, xRange.y);
            targetPosition.z = Random.Range(yRange.x, yRange.y);
            m_splatter.transform.localPosition = targetPosition;
        }
    }
}

