using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace L58.EPAVR
{
    public class DebugConsole : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        public TextMeshProUGUI m_textDisplay;
        [SerializeField] private Canvas m_parentCanvas;
        #endregion
        #region Private Variables
        static string myLog = "";
        private string output;
        private string stack;
        private bool m_isActive = false;


        #endregion

        #region Public Properties
        public static DebugConsole Instance { get; set; }
        public bool Active 
        { 
            get => m_isActive;
            private set
            {
                m_isActive = value;
                ParentCanvas.gameObject.SetActive(m_isActive);
                gameObject.SetActive(m_isActive);
            }
        }

        public Canvas ParentCanvas
        {
            get
            {
                if (!m_parentCanvas) 
                { 
                    if (!TryGetComponent<Canvas>(out m_parentCanvas))
                    {
                        Canvas parentCanvas = GetComponentInParent<Canvas>();
                        if (parentCanvas != null) m_parentCanvas = parentCanvas;
                    }
                }
                return m_parentCanvas;
            }
        }
        public System.Action<string> OnLog { get; set; }
        #endregion

        private void Awake()
        {
            // Set singleton
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetActive(false);
        }
        void OnEnable()
        {
            if (m_textDisplay) OnLog += i => UpdateText(i);
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            if (m_textDisplay) OnLog -= i => UpdateText(i);
            Application.logMessageReceived -= Log;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            output = logString;
            stack = stackTrace;
            myLog = output + "\n" + myLog;
            if (myLog.Length > 5000)
            {
                myLog = myLog.Substring(0, 4000);
            }
            OnLog?.Invoke(myLog);
        }

        public void UpdateText(string text)
        {
            if (!m_textDisplay) return;
            m_textDisplay.text = text;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Helper Methods
        public void SetActive(bool value)
        {
            Active = value;

            if (value == true && ScreenFader.Instance)
            {
                ScreenFader.Instance.ForceComplete();
            }
        }

        public void ToggleActive()
        {
            SetActive(!Active);
        }
        #endregion
    }
}

