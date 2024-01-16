using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadScenarioTest : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private List<RadSite> m_possibleSites;
        [SerializeField] private List<RadSite> m_presetSites;
        [SerializeField] private int m_numSites = 5;
        [Header("Default Configuration")]
        [SerializeField] private Vector2 m_radiationRange = new Vector2(30.0f, 50.0f);
        #endregion
        #region Private Variables
        private float m_globalScenarioTimer = 0.0f;
        public bool m_scenarioCleared = false;

        public List<RadSite> m_activeSites;
        public List<RadSite> m_clearedSites;

        private Action<RadSite> m_onSiteCleared;
        private Action<float> m_onGlobalScenarioTimerTick;

        #endregion
        #region Public Properties
        public static RadScenarioTest Instance { get; private set; }
        public int RadSiteCount { get => m_numSites; }

        public List<RadSite> ClearedSites { get => m_clearedSites; }

        public int ClearedSiteCount 
        {
            get 
            {
                if (m_clearedSites != null) return m_clearedSites.Count;
                return 0;
            }
        }

        public float GlobalScenarioTimer
        {
            get => m_globalScenarioTimer;
            set
            {
                m_globalScenarioTimer = value;
                m_onGlobalScenarioTimerTick?.Invoke(m_globalScenarioTimer);
            }
        }

        public Action<float> OnGlobalScenarioTimerTick { get => m_onGlobalScenarioTimerTick; set => m_onGlobalScenarioTimerTick = value; }

        public Action<RadSite> OnSiteCleared { get => m_onSiteCleared; set => m_onSiteCleared = value; }

        #endregion

        #region Initialization
        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
        }
        // Start is called before the first frame update
        void Start()
        {
            InitRadSites();
        }

        void InitRadSites()
        {
            List<RadSite> sitePool = new List<RadSite>(m_possibleSites);
            m_activeSites = new List<RadSite>();
            int siteCount = m_numSites;
            if (m_presetSites != null && m_presetSites.Count > 0)
            {
                foreach (RadSite site in m_presetSites)
                {
                    m_activeSites.Add(site);
                    siteCount -= 1;
                }
            }

            if (m_numSites > sitePool.Count) siteCount = sitePool.Count;


            for(int i = 0; i < siteCount; i++)
            {
                RadSite randomSite = sitePool[UnityEngine.Random.Range(0, sitePool.Count)];
                m_activeSites.Add(randomSite);
                sitePool.Remove(randomSite);
            }

            foreach(RadSite site in m_activeSites)
            {
                float radLevel = UnityEngine.Random.Range(m_radiationRange.x, m_radiationRange.y);
                site.InitRadCloud(radLevel);
                site.OnSiteCleared += (i, j) => ClearSite(site);
            }
            m_clearedSites = new List<RadSite>();
        }
        #endregion

        public void ClearSite(ChemicalContaminantSite _site)
        {
            if (_site is RadSite radSite)
            {
                m_clearedSites.Add(radSite);
                m_activeSites.Remove(radSite);
                m_onSiteCleared?.Invoke(radSite);
                radSite.OnSiteCleared -= (i, j) => ClearSite(radSite);
 
                if (m_activeSites.Count == 0) m_scenarioCleared = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_scenarioCleared)
            {
                GlobalScenarioTimer += Time.deltaTime;
            }
        }
    }
}

