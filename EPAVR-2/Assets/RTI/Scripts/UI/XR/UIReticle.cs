using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    [RequireComponent(typeof(XRRayInteractor))]
    public class UIReticle : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected GameObject m_reticlePrefab;
        #endregion
        #region Protected Variables
        protected GameObject m_reticle;
        protected XRRayInteractor m_rayInteractor;
        protected XRInteractorLineVisual m_lineVisual;
        protected RaycastResult m_currentRayResult;
        protected bool m_isActive = false;

        public GameObject m_currentObject;
        #endregion
        #region Public Properties
        public GameObject Reticle
        {
            get
            {
                if (!m_reticle)
                {
                    m_reticle = Instantiate(m_reticlePrefab, transform);
                    m_reticle.transform.localPosition = Vector3.zero;
                    m_reticle.transform.localEulerAngles = Vector3.zero;
                }
                return m_reticle;
            }
        }

        public bool Active
        {
            get => m_isActive;
            set
            {
                m_isActive = value;
                Reticle.SetActive(m_isActive);
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache references
            m_rayInteractor = GetComponent<XRRayInteractor>();
            m_lineVisual = GetComponent<XRInteractorLineVisual>();
            // Create the reticle
            m_reticle = Instantiate(m_reticlePrefab, transform);
            m_reticle.transform.localPosition = Vector3.zero;
            m_reticle.transform.localEulerAngles = Vector3.zero;
            // Attach the reticle
            m_lineVisual.AttachCustomReticle(m_reticle);
            m_reticle.gameObject.SetActive(false);
        }

        /*
        // Update is called once per frame
        void LateUpdate()
        {
            // Check if the ray interactor is enabled
            if (m_rayInteractor.TryGetCurrentUIRaycastResult(out m_currentRayResult))
            {
                Active = true;
                
                //var position = m_currentRayResult.worldPosition;
                //var normal = m_currentRayResult.worldNormal;
                //var forward = m_currentRayResult.gameObject.transform.forward;
                //UpdateReticle(position, normal);
                //m_currentObject = m_currentRayResult.gameObject;
               
            }
            else
            {
                if (Active) Active = false;
                m_currentObject = null;
            }
        }
        */

        #region Helper Methods
        public void UpdateReticle(Vector3 _position, Vector3 _normal)
        {
            Reticle.transform.position = _position;
            Reticle.transform.rotation = Quaternion.LookRotation(_normal);
            //Reticle.transform.rotation = Quaternion.FromToRotation(Reticle.transform.up, _normal);
        }
        #endregion

        public void OnHoverEntered(HoverEnterEventArgs args)
        {
        }

        public void OnHoverExited(HoverExitEventArgs args)
        {

        }
    }
}

