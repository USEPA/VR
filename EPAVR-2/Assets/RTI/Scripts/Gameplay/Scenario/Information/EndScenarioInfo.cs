using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class EndScenarioInfo
    {
        #region Protected Variables
        protected ScenarioAsset m_scenario;
        protected Difficulty m_difficulty;
        protected float m_completionTime;

        protected RectTransform m_mapMarkerContainer;
        #endregion
        #region Public Properties
        public abstract Gamemode Mode { get; }
        public ScenarioAsset Scenario { get => m_scenario; }
        public Difficulty Difficulty { get => m_difficulty; }
        public float CompletionTime { get => m_completionTime; }

        public RectTransform MapMarkerContainer { get => m_mapMarkerContainer; }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void LoadInstanceInfo(ScenarioInstance _instance)
        {
            // Get scenario info
            m_scenario = _instance.Scenario;
            m_completionTime = _instance.GlobalScenarioTimer;
            // Get difficulty
            m_difficulty = ScenarioManager.Instance.Difficulty;

            // Make a copy of the static marker container
            if (MapUI.Instance && MapUI.Instance.StaticMarkerContainer)
            {
                GameObject sourceMarkerContainer = MapUI.Instance.StaticMarkerContainer.gameObject;
                m_mapMarkerContainer = GameObject.Instantiate(sourceMarkerContainer).GetComponent<RectTransform>();
                m_mapMarkerContainer.transform.parent = DebugManager.Instance.transform;
            }
        }
    }
}

