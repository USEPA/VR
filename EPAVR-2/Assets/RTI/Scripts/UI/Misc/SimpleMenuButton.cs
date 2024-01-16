using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    [RequireComponent(typeof(Button))]
    public class SimpleMenuButton : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] TextMeshProUGUI m_buttonLabel;
        #endregion
        #region Protected Variables
        protected Button m_button;
        #endregion
        #region Public Properties
        public Button Button
        {
            get
            {
                if (!m_button) m_button = GetComponent<Button>();
                return m_button;
            }
        }
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
        public void SetText(string _text)
        {
            m_buttonLabel.text = _text;
        }
        #endregion
    }
}

