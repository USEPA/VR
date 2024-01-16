using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace L58.EPAVR
{
    public class ScenarioSelectionState : MenuState
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] RectTransform m_scenarioSelectionRect;
        [SerializeField] TMP_Dropdown m_gamemodeDropdown;
        [SerializeField] RectTransform m_nonVRDebugPrompt;
        [SerializeField] List<Button> m_difficultySelectionButtons;
        [Header("Prefab References")]
        [SerializeField] ScenarioSelectableItem m_scenarioSelectablePrefab;
        #endregion
        #region Private Variables
        private List<ScenarioSelectableItem> m_selectableScenarioObjects;
        public ScenarioSelectableItem m_currentSelectedScenarioObject;

        private Button m_currentSelectedDifficulty;
        #endregion
        #region Public Properties
        public ScenarioAsset CurrentSelectedScenario { get => m_currentSelectedScenarioObject.Scenario; }
        #endregion

        #region Enter/Exit-Related Functionality
        public override void OnStateEnter()
        {
            // Set gamemode display
            if (CoreGameManager.Instance) m_gamemodeDropdown.value = (int)CoreGameManager.Instance.CurrentGamemode;
            // Load scenarios if necessary
            if (m_selectableScenarioObjects == null || m_selectableScenarioObjects.Count < 1) LoadSelectableScenarios();


            // Call base functionality
            base.OnStateEnter();
            // Select the current difficulty
            SelectDifficulty((int)ScenarioManager.Instance.Difficulty);
            ForceSelectFirstAvailableScenario();
        }

        public override void OnStateExit()
        {
            // Call base functionality
            base.OnStateExit();
        }
        #endregion

        #region Gamemode-Related Functionality
        public void SetCurrentGamemode(int value)
        {
            if (CoreGameManager.Instance == null || ((int) CoreGameManager.Instance.CurrentGamemode) == value) return;
            UnityEngine.Debug.Log($"{gameObject.name} set gamemode: {(Gamemode)value} || Time: {Time.time}");
            // Relay the gamemode to the CoreGameManager
            CoreGameManager.Instance.SetGamemode((Gamemode)value);
            // Clear the selectable scenarios
            ClearSelectableScenarios();
            // Reload selectable scenarios
            LoadSelectableScenarios();
        }
        #endregion


        #region Scenario-Related Functionality
        private void LoadSelectableScenarios()
        {
            UnityEngine.Debug.Log($"{gameObject.name} entered LoadSelectableScenarios || Time: {Time.time}");
            if (CoreGameManager.Instance == null || CoreGameManager.Instance.CurrentGamemodeConfig == null) return;
            // Load scenarios based on current selected gamemode
            LoadSelectableScenarios(CoreGameManager.Instance.CurrentGamemode);
        }

        private void LoadSelectableScenarios(Gamemode _mode)
        {
            GamemodeConfigAsset gamemode = CoreGameManager.GetGamemodeConfig(_mode);
            UnityEngine.Debug.Log($"{gameObject.name} load selectable scenarios: {_mode} | Config Asset: {gamemode} | Available Scenarios: {gamemode.AvailableScenarios.Count} || Time: {Time.time}");
            if (gamemode == null || gamemode.AvailableScenarios == null || gamemode.AvailableScenarios.Count < 1) return;
            // Initialize scenario selectable list if necessary
            if (m_selectableScenarioObjects == null) m_selectableScenarioObjects = new List<ScenarioSelectableItem>();
            // Loop through each scenario file and create a selectable object for it
            foreach (ScenarioAsset _scenario in gamemode.AvailableScenarios)
            {
                // Instantiate/initialize the scenario selectable object and add it to the stored list
                ScenarioSelectableItem scenarioSelectable = Instantiate(m_scenarioSelectablePrefab.gameObject, m_scenarioSelectionRect).GetComponent<ScenarioSelectableItem>();
                scenarioSelectable.gameObject.name = $"{scenarioSelectable.gameObject.name}_{m_selectableScenarioObjects.Count}";
                scenarioSelectable.Init(_scenario);
                scenarioSelectable.Button.onClick.AddListener(() => SelectScenario(scenarioSelectable));
                //UnityEngine.Debug.Log($"Added listener to: {scenarioSelectable.gameObject.name} || Time: {Time.time}");
                m_selectableScenarioObjects.Add(scenarioSelectable);
            }

            // Force select first available scenario
            ForceSelectFirstAvailableScenario();
        }

        private void ClearSelectableScenarios()
        {
            // Reset current selected scenario reference
            m_currentSelectedScenarioObject = null;
            // Destroy each scenario object
            for (int i = 0; i < m_selectableScenarioObjects.Count; i++)
            {
                Destroy(m_selectableScenarioObjects[i].gameObject);
            }
            // Reset selectable scenario
            m_selectableScenarioObjects.Clear();
            m_selectableScenarioObjects = null;
        }

        public void ForceSelectFirstAvailableScenario()
        {
            if (ScenarioManager.Instance.EndScenarioInfo != null && m_selectableScenarioObjects != null && m_selectableScenarioObjects.Count > 0)
            {
                for(int i = 0; i < m_selectableScenarioObjects.Count; i++)
                {
                    ScenarioSelectableItem scenarioItem = m_selectableScenarioObjects[i];
                    if (scenarioItem.Scenario == ScenarioManager.Instance.EndScenarioInfo.Scenario)
                    {
                        SelectScenario(i);
                        return;
                    }
                }
            }
            SelectScenario(0);

        }
        public void SelectScenario(int _index)
        {
            if (m_selectableScenarioObjects == null || m_selectableScenarioObjects.Count < 1) return;
            UnityEngine.Debug.Log($"Selecting scenario by index: {_index} || Time: {Time.time}");
            SelectScenario(m_selectableScenarioObjects[_index]);
        }


        public void SelectScenario(ScenarioSelectableItem _scenarioObject)
        {
            //UnityEngine.Debug.Log($"Selected scenario: {_scenarioObject.name} || Time: {Time.time}");
            if (m_currentSelectedScenarioObject != null && m_currentSelectedScenarioObject != _scenarioObject)
            {
                m_currentSelectedScenarioObject.GetComponent<Animator>().SetBool("ForceSelected", false);
            }

            m_currentSelectedScenarioObject = _scenarioObject;

            m_currentSelectedScenarioObject.GetComponent<Animator>().SetBool("ForceSelected", true);
        }

        public void SelectDifficulty(int _difficultyIndex)
        {
            if (!ScenarioManager.Instance) return;
            //UnityEngine.Debug.Log($"Selected Difficulty: {(Difficulty)_difficultyIndex} || Time: {Time.time}");
            if (m_currentSelectedDifficulty != null) m_currentSelectedDifficulty.GetComponent<Animator>().SetBool("ForceSelected", false);

            ScenarioManager.Instance.SetDifficulty((Difficulty)_difficultyIndex);
            m_currentSelectedDifficulty = m_difficultySelectionButtons[(int)ScenarioManager.Instance.Difficulty];
            m_currentSelectedDifficulty.GetComponent<Animator>().ResetTrigger("Normal");
           
            
            m_currentSelectedDifficulty.GetComponent<Animator>().SetTrigger("Selected");
            m_currentSelectedDifficulty.GetComponent<Animator>().SetBool("ForceSelected", true);
        }

        public void AttemptLoadScenario()
        {
            //UnityEngine.Debug.Log($"Arrived in AttemptLoadScenario | Selected: {((m_currentSelectedScenarioObject != null) ? m_currentSelectedScenarioObject.name : "N/A")} || Time: {Time.time}");
            // Make sure there is a valid scenario to load
            if (!CoreGameManager.Instance || m_currentSelectedScenarioObject == null) return;
            CoreGameManager.Instance.LoadScenario(CurrentSelectedScenario);
            /*
            // Check if there is no XR device plugged in
            if (CoreGameManager.Instance.HasXRDevicesLoaded())
            {
                CoreGameManager.Instance.LoadScenario(CurrentSelectedScenario);
            }
            else
            {
                m_nonVRDebugPrompt.gameObject.SetActive(true);
                //UnityEngine.Debug.Log($"About to load scenario with no XR devices: {CurrentSelectedScenario.Title} || Time: {Time.time}");
            }
            */
        }

        public void LoadScenario(bool _nonVRMode = false)
        {
            // Make sure there is a valid scenario to load
            if (!CoreGameManager.Instance || m_currentSelectedScenarioObject == null) return;
            // Check if there is no XR device plugged in
            if (!CoreGameManager.Instance.HasXRDevicesLoaded())
            {
                UnityEngine.Debug.Log($"About to load scenario with no XR devices: {CurrentSelectedScenario.Title} || Time: {Time.time}");
            }
            if (_nonVRMode && VRUserManager.Instance) VRUserManager.Instance.IsNonVRMode = true;
            // Load the current selected scenario
            CoreGameManager.Instance.LoadScenario(CurrentSelectedScenario);
        } 
        #endregion
    }
}

