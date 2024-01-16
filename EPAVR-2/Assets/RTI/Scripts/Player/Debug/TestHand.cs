using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace L58.EPAVR
{
    public class TestHand : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] InputActionReference m_mousePositionRef;
        [SerializeField] InputActionReference m_mouseScrollRef;
        [Header("Configuration")]
        [SerializeField] Vector2 m_distanceExtremities = new Vector2(1.0f, 3.0f);
        [SerializeField] float m_defaultZDistance = 0.5f;
        [SerializeField] float m_scrollSpeed = 2.0f;
        #endregion
        #region Private Variables
        public Vector2 m_mousePosition;
        private Vector3 targetPosition;

        public float m_mouseScrollValue;

        public TestDraggableItem m_currentItem;
        #endregion
        #region Public Properties
        public TestDraggableItem CurrentItem { get => m_currentItem; }
        #endregion

        #region Initialization
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!VRUserManager.Instance || VRUserManager.Instance.CurrentState != PlayerState.Focused) return;
            m_mousePosition = m_mousePositionRef.action.ReadValue<Vector2>();
            m_mouseScrollValue = m_mouseScrollRef.action.ReadValue<float>();
            // Get camera distance
            float cameraDistance = Camera.main.WorldToScreenPoint((transform.position)).z;
            // Get screen position and transform that to world position
            Vector3 screenPosition = new Vector3(m_mousePosition.x, m_mousePosition.y, cameraDistance);
            Vector3 desiredPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            Vector3 localPosition = Camera.main.transform.InverseTransformPoint(desiredPosition);
            transform.localPosition = localPosition;
            targetPosition = new Vector3(localPosition.x, localPosition.y, GetScrollValue());
            transform.localPosition = targetPosition;
            /*
            Vector3 childTargetLocalPosition = new Vector3(0, 0, GetScrollValue());
            Vector3 worldPosition = transform.TransformPoint(childTargetLocalPosition);
            if (m_currentItem != null)
            {
                m_currentItem.Rigidbody.MovePosition(worldPosition);
            }
            */




            //transform.position = new Vector3(desiredPosition.x, desiredPosition.y, transform.position.z);

            // Get this in world space
            //Vector3 worldPosition = Camera.main.ScreenToWorldPoint(m_mousePosition);
            //transform.position = worldPosition;
            /*
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            localPosition.z = transform.localPosition.z;
            transform.localPosition = localPosition;
            */

        }

        public void LateUpdate()
        {
            
        }


        public float GetScrollValue()
        {
            if (m_mouseScrollValue == 0.0f) return transform.localPosition.z;
            float zValue = transform.localPosition.z + ((Mathf.Sign(m_mouseScrollValue) * m_scrollSpeed) * Time.deltaTime);
            // Clamp this value
            zValue = Mathf.Clamp(zValue, m_distanceExtremities.x, m_distanceExtremities.y);
            return zValue;
        }

        public void AttemptGrabItem(TestDraggableItem _item)
        {
            //UnityEngine.Debug.Log($"Entered AttemptGrabItem: {_item.gameObject.name} || Time: {Time.time}");
            m_currentItem = _item;
            m_currentItem.OnDragStart();
            if (m_currentItem.ParentToHand)
            {
                _item.transform.parent = transform;
                _item.transform.localPosition = Vector3.zero;
            }
        }

        public void ReleaseItem()
        {
            if (m_currentItem == null) return;
            //UnityEngine.Debug.Log($"Entered ReleaseItem: {m_currentItem.gameObject.name} || Time: {Time.time}");
            if (m_currentItem.ParentToHand) m_currentItem.transform.parent = null;
            m_currentItem.OnDragEnd();
            m_currentItem = null;
        }
    }
}

