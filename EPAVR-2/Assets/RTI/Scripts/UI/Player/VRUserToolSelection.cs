using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace L58.EPAVR
{
    public class VRUserToolSelection : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] Button m_simpleMenuButtonPrefab;
        [SerializeField] List<SimpleMenuButton> m_toolOptions;
        #endregion
        #region Private Variables
        private System.Action<bool> m_onToggleMenu;
        #endregion
        #region Public Properties
        public List<SimpleMenuButton> ToolOptions { get => m_toolOptions; }

        public System.Action<bool> OnToggleMenu { get => m_onToggleMenu; set => m_onToggleMenu = value; }
        #endregion
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(List<ToolType> _availableTypes)
        {
            InitButtonOptions(_availableTypes);
        }

        public void InitButtonOptions(List<ToolType> _availableTypes)
        {
            // Clear the tool options
            // Get how many options currently exist
            //List<SimpleMenuButton> availableButtons = GetComponentsInChildren<SimpleMenuButton>().ToList();
            List<SimpleMenuButton> availableButtons = m_toolOptions;
            foreach (SimpleMenuButton button in availableButtons) button.gameObject.SetActive(false);
            for(int i = 0; i < _availableTypes.Count; i++)
            {
                ToolType _type = _availableTypes[i];
                UnityEngine.Debug.Log($"{gameObject.name} initialized tool option[{i}]: {_type} | Available Buttons: {availableButtons.Count} || Time: {Time.time}");

                SimpleMenuButton item = availableButtons[i];
                item.Button.onClick.RemoveAllListeners();
                item.Button.onClick.AddListener(() => SelectTool((int) _type));
                item.SetText(_type.ToString());
                item.gameObject.SetActive(true);
            }
        }
        public void ToggleMenu()
        {
            m_onToggleMenu?.Invoke(!gameObject.activeInHierarchy);
            if (!gameObject.activeInHierarchy)
                Open();
            else
                Close();
        }

        public void Open()
        {
            VRUserManager.Instance?.SetState(PlayerState.Menu);
            gameObject.SetActive(true);
            if (VRUserManager.Instance.CurrentTool != null)
            {
                for (int i = 0; i < m_toolOptions.Count; i++)
                {
                    if (i == (int)VRUserManager.Instance.CurrentToolType)
                    {
                        EventSystem.current.SetSelectedGameObject(m_toolOptions[i].Button.gameObject);
                        UnityEngine.Debug.Log($"Found current tool: {VRUserManager.Instance.CurrentToolType} | Button: {m_toolOptions[i].gameObject.name} || Time: {Time.time}");
                        return;
                    }
                }
            }
        }

        public void Close()
        {
            VRUserManager.Instance?.SetState(VRUserManager.Instance.PreviousState);
            gameObject.SetActive(false);
        }

        #region Selection-Related Functionality
        public void SelectTool(ToolType _tool)
        {
            if (!VRUserManager.Instance) return;
            VRUserManager.Instance.SpawnTool(_tool);
            Close();
        }

        public void SelectTool(int _toolIndex)
        {
            if (_toolIndex < 0) return;
            SelectTool((ToolType)_toolIndex);
        }
        #endregion
    }
}

