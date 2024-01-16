using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadSite : ChemicalContaminantSite
    {
        #region Inspector Assigned Variables
        [SerializeField] protected RadCloud m_radCloudPrefab;
        [SerializeField] protected RadType m_availableTypes = RadType.Gamma;
        [SerializeField] protected Vector2 m_radSpreadSize = new Vector2(0.75f, 2.5f);
        [SerializeField] protected RadCloud m_presetRadCloud;
        #endregion
        #region Protected Variables
        protected RadCloud m_radCloud;
        protected RadType m_type;
        #endregion
        #region Public Properties
        public override Gamemode Gamemode => Gamemode.RadiationSurvey;
        public RadType Type => m_type;

        public float RadRadius{ get => m_radCloud.Radius; }
        public RadCloud Cloud => m_radCloud;

        #endregion
        #region Initialization

        private void Start()
        {
            //InitRadCloud();
        }
        public override void Init(ChemicalAgent _agent)
        {
            //InitContaminatedObjects();
            //base.Init(_agent);
        }

        public override void InitContaminatedObjects()
        {
            UnityEngine.Debug.Log($"{gameObject.name} initialized: {m_agent.Name} || Time: {Time.time}");
        }

        public void InitRadCloud(float _radLevel, bool _enableSmokeEffect = true)
        {
            // Create the rad cloud
            m_radCloud = (!m_presetRadCloud) ?Instantiate(m_radCloudPrefab.gameObject, transform).GetComponent<RadCloud>() : m_presetRadCloud;
            m_radCloud.gameObject.SetActive(true);
            // Get a random radiation type
            m_type = GetRandomRadType();
            // Initialize the rad cloud
            m_radCloud.Init(this, m_type, m_radSpreadSize, _radLevel, _enableSmokeEffect);
        }
        #endregion

        #region Clear-Related Functionality
        public override void ClearSite()
        {
            base.ClearSite();
            UnityEngine.Debug.Log($"{gameObject.name} cleared: {m_type} || Time: {Time.time}");
            m_onSiteCleared?.Invoke(null, null);
    
            //m_radCloud.gameObject.SetActive(false);
        }
        #endregion

        #region Helper Methods
        public RadType GetRandomRadType()
        {
            RadType[] availableTypes = Enum.GetValues(typeof(RadType))
                   .Cast<RadType>()
                   .Where(i => (m_availableTypes & i) == i)    // or use HasFlag in .NET4
                   .ToArray();

            return availableTypes[new System.Random().Next(availableTypes.Length)];
        }
        #endregion

        private void OnDestroy()
        {
            if (m_onCleared != null) m_onCleared.RemoveAllListeners();
        }
    }
}

