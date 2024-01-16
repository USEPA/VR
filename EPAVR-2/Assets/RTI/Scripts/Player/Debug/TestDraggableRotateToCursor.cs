using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class TestDraggableRotateToCursor : TestDraggableItem
    {
        #region Private Variables
        private bool m_active = false;
        #endregion
        #region Public Properties
        public override bool ParentToHand => false;
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!m_active || !VRUserManager.Instance) return;

            // Get the current mouse position
            Vector2 mousePosition = VRUserManager.Instance.PointerEventData.position;
            // Get the current position of this object in screen space
            Vector3 objectScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            mousePosition.x = mousePosition.x - objectScreenPosition.x;
            mousePosition.y = mousePosition.y - objectScreenPosition.y;
            // Get the angle
            float angle = (Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg);
            transform.localEulerAngles = new Vector3(0, 0, -angle);
        }

        #region Interaction Functionality
        public override void OnDragStart()
        {
            // Call base functionality
            base.OnDragStart();
            // Set active
            m_active = true;
        }

        public override void OnDragEnd()
        {
            // Call base functionality
            base.OnDragEnd();
            // Set active
            m_active = false;
        }
        #endregion
    }
}

