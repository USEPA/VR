using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace L58.EPAVR
{
    public class DeviceGUISubstate : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("UI Elements")]
        [SerializeField] List<Text> m_textElements;
        [SerializeField] List<TextMeshProUGUI> m_textMeshElements;
        [SerializeField] List<Image> m_images;
        [SerializeField] List<Slider> m_sliders;
        [SerializeField] AgentInfoDisplay m_agentInfoDisplay;
        [SerializeField] List<AgentInfoDisplayItem> m_agentInfoElements;
        [SerializeField] List<Button> m_buttons;
        [Header("UI Events")]
        [SerializeField] UnityEvent m_onEnterStateUnityEvents;
        [SerializeField] UnityEvent m_onExitStateUnityEvents;

        [SerializeField] ChemicalAgent m_contaminant;
        #endregion
        #region Protected Variables
        protected DeviceGUIState m_parent;

        protected float m_stateTimer;
        protected Action m_onStateEnter;
        protected Action<float> m_onStateUpdate;
        protected Action m_onStateExit;
        #endregion
        #region Public Properties
        public List<Button> Buttons { get => m_buttons; }
        public Action<float> OnStateUpdated { get => m_onStateUpdate; set => m_onStateUpdate = value; }
        #endregion

        #region Initialization
        public virtual void Init(DeviceGUIState _parent)
        {
            // Cache parent reference
            m_parent = _parent;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (m_contaminant != null) SetAgentDisplay(m_contaminant);
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region State Functionality
        public void OnStateEnter(float _time)
        {
            // Activate the game object
            gameObject.SetActive(true);
            // Do the thing
            m_stateTimer = _time;
            // Invoke any necessary events
            m_onStateEnter?.Invoke();
            m_onEnterStateUnityEvents?.Invoke();
            //UnityEngine.Debug.Log($"Entered substate: {gameObject.name} || Time: {Time.time}");
        }

        public virtual void OnStateUpdate(float _delta)
        {
            // Decrement state timer by delta time
            m_stateTimer -= _delta;
            // Invoke any necessary events
            m_onStateUpdate?.Invoke(m_stateTimer);
        }

        public void OnStateExit()
        {
            // Reset timer
            m_stateTimer = 0.0f;
            // Invoke any necessary events
            m_onStateExit?.Invoke();
            m_onExitStateUnityEvents?.Invoke();
            // De-activate the game object
            gameObject.SetActive(false);
            //UnityEngine.Debug.Log($"Exiting substate: {gameObject.name} || Time: {Time.time}");
        }
        #endregion

        #region Helper Methods
        public void SetText(int _index, string _text)
        {
            // Make sure conditions are valid
            if (m_textElements == null || m_textElements.Count < 1 || _index < 0 || _index >= m_textElements.Count) return;
            // Set the text of the specified text element
            m_textElements[_index].text = _text;
        }

        public void SetTextMesh(int _index, string _text)
        {
            // Make sure conditions are valid
            if (m_textMeshElements == null || m_textMeshElements.Count < 1 || _index < 0 || _index >= m_textMeshElements.Count) return;
            // Set the text of the specified text element
            m_textMeshElements[_index].text = _text;
        }

        public void SetImage(int _index, Sprite _sprite)
        {
            // Make sure conditions are valid
            if (m_images == null || m_images.Count < 1 || _index < 0 || _index >= m_images.Count) return;
            // Set the sprite of the specified image
            m_images[_index].sprite = _sprite;
        }

        public void SetSlider(int _index, float _value)
        {
            // Make sure conditions are valid
            if (m_sliders == null || m_sliders.Count < 1 || _index < 0 || _index >= m_sliders.Count) return;
            // Set the value of the specified slider
            m_sliders[_index].value = _value;
        }

        public void SetAgentDisplay(ChemicalAgent _agent)
        {
            if (m_agentInfoDisplay == null) return;
            if (_agent != null) UnityEngine.Debug.Log($"Arrived in SetAgentDisplay for Substate: {gameObject.name} | Agent: {_agent.Name} || Time: {Time.time}");
            m_agentInfoDisplay.SetAgentDisplay(_agent);
            //if (m_agentInfoElements == null || m_agentInfoElements.Count < 1) return;
            //for (int i = 0; i < m_agentInfoElements.Count; i++) m_agentInfoElements[i].ParseAgentInfo(_agent);
        }

    #if UNITY_EDITOR
        [ContextMenu("Update Chemical")]
        public void UpdateChemicalDisplay()
        {
            SetAgentDisplay(m_contaminant);
        }
    #endif
    #endregion
    }
}

