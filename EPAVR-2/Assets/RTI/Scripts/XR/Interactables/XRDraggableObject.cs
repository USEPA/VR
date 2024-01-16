using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRDraggableObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected MeshRenderer m_parentMesh;
        #endregion
        #region Protected Variables
        protected Collider m_collider;
        protected Bounds m_bounds;

        protected bool m_pointerInBounds = false;
        protected bool m_moveEnabled = false;

        protected L58XRHand m_currentInteractor;
        protected XRActionMap m_currentInteractorActionMap;
        protected InputActionReference m_currentInteractorPositionRef;
        protected GameObject hitObject;

        protected System.Action<bool> m_onSetMovable;
        #endregion
        #region Public Properties
        public bool MoveEnabled { get => m_moveEnabled; }
        public System.Action<bool> OnSetMovable { get => m_onSetMovable; set => m_onSetMovable = value; }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (TryGetComponent<Collider>(out m_collider))
            {
                m_bounds = m_collider.bounds;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_bounds == null ||!m_moveEnabled || !m_pointerInBounds || m_currentInteractor == null) return;

            RaycastHit hit;
            if (m_currentInteractor.TryGetCurrent3DRaycastHit(out hit))
            {
                hitObject = hit.collider.gameObject;
                Vector3 hitPosition = new Vector3(hit.point.x, hit.point.y, transform.position.z);
                transform.position = (m_parentMesh != null) ? GetClampedPositionWithinParentBounds(hitPosition, hit) : hitPosition;
            }
            else
            {
                hitObject = null;
            }
        }

        #region Movement-Related Functionality
        public void ToggleMove()
        {
            if (!m_moveEnabled)
                EnableMove();
            else
                DisableMove();
        }

        public void EnableMove()
        {
            // Make sure pointer was in sample bounds
            if (m_moveEnabled || !m_pointerInBounds) return;
            m_moveEnabled = true;
            m_onSetMovable?.Invoke(true);
            UnityEngine.Debug.Log($"Enabled sample area movement || Time: {Time.time}");
        }
        public void EnableMove(ActivateEventArgs args)
        {
            EnableMove();
            if (args.interactor is L58XRHand hand)
            {
                m_currentInteractor = hand;
                m_currentInteractorActionMap = SimulationManager.Instance.GetHandMap(hand);
            }
        }

        public void DisableMove()
        {
            if (!m_moveEnabled) return;
            m_moveEnabled = false;
            if (m_currentInteractor != null)
            {
                m_currentInteractor = null;
            }
            m_onSetMovable?.Invoke(false);
            UnityEngine.Debug.Log($"Disabled sample area movement || Time: {Time.time}");
        }
        #endregion

        #region Helper Methods
        public Vector3 GetClampedPositionWithinParentBounds(Vector3 _hitPosition, RaycastHit _hit)
        {
            Bounds futureBounds = new Bounds(new Vector3(_hitPosition.x, _hitPosition.y, m_parentMesh.transform.position.z), m_bounds.size);
            float x = Mathf.Clamp(_hit.point.x, (m_parentMesh.bounds.min.x + m_bounds.extents.x), (m_parentMesh.bounds.max.x - m_bounds.extents.x));
            float y = Mathf.Clamp(_hit.point.y, (m_parentMesh.bounds.min.y + m_bounds.extents.y), (m_parentMesh.bounds.max.y - m_bounds.extents.y));
            //Vector3 clampedPosition = m_parentMesh.transform.InverseTransformPoint(KeepInBounds(futureBounds));
            return new Vector3(x, y, _hitPosition.z);
        }
        #endregion
    }
}

