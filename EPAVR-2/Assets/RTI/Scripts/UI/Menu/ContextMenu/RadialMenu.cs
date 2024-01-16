using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;


namespace L58.EPAVR
{
    public class RadialMenu : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected TextMeshProUGUI m_title;
        [SerializeField] protected List<RadialMenuButton> m_options;
        [SerializeField] protected RectTransform m_pointerContainer;
        [Header("Default Configuration")]
        [SerializeField] protected float m_globalOffset = 0.0f;
        #endregion
        #region Protected Variables
        public GameObject eventSystemCurrentSelected;
        protected RectTransform m_rectTransform;

        public RadialMenuButton m_currentSelectedItem;

        protected float m_baseAngleOffset = 0.0f;
        protected float m_currentAngle = 0.0f;

        #endregion
        #region Public Properties
        public List<RadialMenuButton> Options { get => m_options; }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            // Cache components
            m_rectTransform = GetComponent<RectTransform>();

            // Set up offset
            m_baseAngleOffset = (360f / (float)m_options.Count);
            // Initialize each option element
            for(int i = 0; i < m_options.Count; i++) m_options[i].Init(this, i, (m_baseAngleOffset * i) + m_globalOffset);
        }

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject != null) eventSystemCurrentSelected = EventSystem.current.currentSelectedGameObject;
            // Set up values
            float rawAngle = 0.0f;
            // Get mouse position
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            // Get raw angle from mouse position
            rawAngle = Mathf.Atan2(mousePosition.y - m_rectTransform.position.y, mousePosition.x - m_rectTransform.position.x) * Mathf.Rad2Deg;
            // Normalize the angle
            m_currentAngle = MathHelper.ClampAngle(-rawAngle + 90.0f - m_globalOffset + (m_baseAngleOffset / 2f));
            /*
            if (m_baseAngleOffset != 0)
            {
                // Get the index closest to the current angle
                int selectedIndex = (int)(m_currentAngle / m_baseAngleOffset);
                // Make sure this index is valid
                if (m_options[selectedIndex] != null && m_currentSelectedItem != m_options[selectedIndex])
                {
                    SelectItem(m_options[selectedIndex]);
                }
            }
            */

            // Rotate pointer in direction of cursor
            m_pointerContainer.rotation = Quaternion.Euler(0, 0, rawAngle + 270.0f);
        }

        public void SetTitle(string _text)
        {
            m_title.text = _text;
        }

        #region Selection-Related Functionality
        public void SelectItem(RadialMenuButton _item)
        {
            // Check if there is a previous selected item
            if (m_currentSelectedItem != null)
            {
                // Do the thing
                EventSystem.current.SetSelectedGameObject(null);
            }
            // Set references
            m_currentSelectedItem = _item;
            StartCoroutine(SelectButton(m_currentSelectedItem.Button));
        }

        IEnumerator SelectButton(Button _button)
        {
            yield return null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_button.gameObject);
        }
        #endregion
    }
}

