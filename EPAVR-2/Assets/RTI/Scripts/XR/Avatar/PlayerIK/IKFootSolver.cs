using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class IKFootSolver : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Transform m_body;
        [SerializeField] private IKFootSolver m_otherFoot;
        [Header("Default Configuration")]
        [SerializeField] private LayerMask m_groundLayer;
        [SerializeField] private float m_speed = 5.0f;
        [SerializeField] private float m_stepDistance = 0.3f;
        [SerializeField] private float m_stepLength = 0.3f;
        [SerializeField] private float m_stepHeight = 0.3f;
        [Header("Default Offsets")]
        [SerializeField] Vector3 m_footPosOffset;
        [SerializeField] Vector3 m_footRotOffset;
        #endregion
        #region Private Variables
        private float m_footSpacing;
        private float m_lerpValue;

        private Vector3 m_previousPosition;
        private Vector3 m_currentPosition;
        private Vector3 m_futurePosition;

        private Vector3 m_previousNormal;
        private Vector3 m_currentNormal;
        private Vector3 m_futureNormal;
        #endregion
        #region Public Properties
        public bool Active { get; set; } = false;
        #endregion

        #region Initialization
        public void Init()
        {
            // Initialize values
            m_footSpacing = transform.localPosition.x;
            m_currentPosition = m_futurePosition = m_previousPosition = transform.position;
            m_currentNormal = m_futureNormal = m_previousNormal = transform.up;
            m_lerpValue = 1.0f;
            // Perform initial grounded check
            CheckGrounded();
            // Set active
            Active = true;

        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Make sure solver is active
            if (!Active) return;

            // Update the solver
            UpdateSolver();
        }

        public void UpdateSolver()
        {
            // Set position/rotation according to current values
            transform.position = m_currentPosition + m_footPosOffset;
            transform.rotation = Quaternion.LookRotation(m_currentNormal) * Quaternion.Euler(m_footRotOffset);
            //transform.up = m_currentNormal;

            // Perform ground raycast
            CheckGrounded();

            // Check if the foot needs to be moved
            if (m_lerpValue < 1)
            {
                Vector3 tempPos = Vector3.Lerp(m_previousPosition, m_futurePosition, m_lerpValue);
                tempPos.y += Mathf.Sin(m_lerpValue * Mathf.PI) * m_stepHeight;

                m_currentPosition = tempPos;
                m_currentNormal = Vector3.Lerp(m_previousNormal, m_futureNormal, m_lerpValue);

                m_lerpValue += Time.deltaTime * m_speed;
            }
            else
            {
                m_previousPosition = m_futurePosition;
                m_previousNormal = m_futureNormal;
            }
        }

        public bool CheckGrounded()
        {
            Ray ray = new Ray(m_body.position + (m_body.right * m_footSpacing) + (Vector3.up * 2), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 10.0f, m_groundLayer))
            {
                // Check distance
                if (MathHelper.QuickDistance(m_futurePosition, hit.point) > m_stepDistance && !m_otherFoot.IsMoving() && !IsMoving())
                {
                    // Reset lerp
                    m_lerpValue = 0.0f;
                    int direction = (m_body.InverseTransformPoint(hit.point).z > m_body.InverseTransformPoint(m_futurePosition).z) ? 1 : -1;
                    m_futurePosition = hit.point + (m_body.forward * (direction * m_stepLength));
                    m_futureNormal = hit.normal;
                }

                return true;
            }
            return false;
        }

        public bool IsMoving()
        {
            return m_lerpValue < 1;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_futurePosition, 0.1f);
        }
    }
}

