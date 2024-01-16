using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace L58.EPAVR
{
    public class ContaminationAmountTest : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] GameObject m_epicenterMarker;
        [SerializeField] GameObject m_testPositionMarker;
        #endregion
        #region Public Variables
        public Contamination m_currentObject;
        public float m_currentContaminationResult = 0.0f;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CheckContaminationAmount()
        {
            // Raycast to find a contaminated object
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out hit, 30.0f))
            {
                if (hit.collider.gameObject.TryGetComponent<Contamination>(out Contamination contamination))
                {
                    m_currentObject = contamination;
                    SetMarkersEnabled(true);
                    m_epicenterMarker.transform.position = contamination.Epicenter;

                    Vector3 testPoint = hit.collider.ClosestPointOnBounds(hit.point);
                    Vector3 testPointWorld = m_currentObject.transform.TransformPoint(testPoint);
                    m_testPositionMarker.transform.position = testPoint;
                    m_currentContaminationResult = contamination.GetConcentrationFromPoint(testPoint);
                }
            }
            else
            {
                SetMarkersEnabled(false);
                m_currentObject = null;
                m_currentContaminationResult = 0.0f;
            }
        }

        public void SetMarkersEnabled(bool value)
        {
            m_epicenterMarker.SetActive(value);
            m_testPositionMarker.SetActive(value);
        }
    }
}

