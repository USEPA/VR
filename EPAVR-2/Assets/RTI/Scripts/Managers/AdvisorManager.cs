using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class AdvisorManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Prefab References")]
        [SerializeField] private Camera m_advisorCameraPrefab;
        [SerializeField] private AdvisorGUI m_guiPrefab;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;

        private AdvisorGUI m_gui;
        private Camera m_advisorCam;
        #endregion

        #region Public Properties
        public static AdvisorManager Instance { get; set; }
        public ManagerStatus Status => m_status;

        public AdvisorGUI GUI { get => m_gui; }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Set singleton
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }
        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            // Create the advisor camera
            if (!m_advisorCam) m_advisorCam = Instantiate(m_advisorCameraPrefab);
            // Create the Advisor GUI and initialize it
            if (!m_gui) m_gui = Instantiate(m_guiPrefab);
            m_gui.Init();
            m_gui.gameObject.SetActive(false);
            // Finish initialization
            UnityEngine.Debug.Log($"AdvisorManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }
        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin reset
            m_status = ManagerStatus.Resetting;
            // Finish reset
            UnityEngine.Debug.Log($"AdvisorManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }
        #endregion
    }
}

