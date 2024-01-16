using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Simulation Scenario")]
    public class ScenarioAsset : ScriptableObject
    {
        #region Inspector Assigned Variables
        [SerializeField] string m_title = "";
        [SerializeField] string m_scene = "";
        [SerializeField] Sprite m_icon;
        [SerializeField] Sprite m_mapImage;
        [SerializeField] [TextArea] string m_summary = "";
        [SerializeField] [TextArea] string m_briefing = "";
        [SerializeField] SampleToolOld m_toolPrefab;
        [SerializeField] ToolType m_defaultTool;
        [SerializeField] List<ScenarioStep> m_steps;
        [SerializeField] List<ChemicalAgent> m_spawnableChemicals;
        [SerializeField] ChemicalDistribution m_spawnableChemicalDistribution;
        [SerializeField] float m_maxBonusTime = 10.0f;
        #endregion
        #region Public Properties
        public string Title { get => m_title; }
        public string Scene { get => m_scene; }
        public Sprite Icon { get => m_icon; }
        public Sprite MapImage { get => m_mapImage; }
        public string Summary { get => m_summary; }
        public string Briefing { get => m_briefing; }

        public SampleToolOld ToolPrefab { get => m_toolPrefab; }
        public ToolType DefaultTool { get => m_defaultTool; }
        public List<ScenarioStep> Steps { get => m_steps; }
        public List<ChemicalAgent> SpawnableChemicals { get => m_spawnableChemicals; }
        public ChemicalDistribution SpawnableChemicalDistribution { get => m_spawnableChemicalDistribution; }

        public float MaxBonusTime { get => m_maxBonusTime * 60.0f; }
        public virtual Gamemode Mode { get; }
        #endregion

        #region Instance-Related Functionality
        public virtual ScenarioInstance CreateInstance()
        {
            return new ScenarioInstance(this);
        }

        /*
        public virtual ScenarioInstance CreateInstance(List<ContaminationSite> _sites)
        {
            return new ScenarioInstance(this);
        }*/
        #endregion

        #region Helper Methods
        /// <summary>
        /// Retrieves a specified step from the stored list
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public ScenarioStep GetStep(int _index)
        {
            // If this is a valid index, return the specified step
            if (_index >= 0 && _index < m_steps.Count) return m_steps[_index];
            // Otherwise, invalid index, return null
            return null;
        }


        public ChemicalAgent GetRandomChemical()
        {
            if (m_spawnableChemicals != null && m_spawnableChemicals.Count > 0)
            {
                // Select a random chemical from the pool
                return m_spawnableChemicals[Random.Range(0, m_spawnableChemicals.Count)];
            }
            return null;
        }
        #endregion
        
        private void OnValidate()
        {
            if (m_spawnableChemicalDistribution == null) return;
            m_spawnableChemicalDistribution.OnItemsChange();
        }
       
        #region Context Helper Methods
        #if UNITY_EDITOR
        [ContextMenu("Assign Step IDs")]
        public void AssignStepIDs()
        {
            // Make sure there are steps stored
            if (m_steps.Count < 1) return;
            // Mark this asset as dirty
            EditorUtility.SetDirty(this);
            // Loop through each step and assign its ID
            for (int i = 0; i < m_steps.Count; i++)
            {
                // Assign this step's ID
                m_steps[i].ID = i;
            }
            // Save and refresh assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endif
        #endregion
    }

    public enum Gamemode {ChemicalHunt, RadiationSurvey, Tutorial}
    [System.Serializable]
    public class ScenarioStep
    {
        #region Inspector Assigned Variables
        [SerializeField] string m_title = "";
        [SerializeField] VideoClip m_video = default;
        [SerializeField] bool m_enableSampleArea = false;
        #endregion
        #region Private Variables
        private int m_stepID = -1;
        private float m_timeElapsed = 0.0f;
        #endregion
        #region Public Properties
        public string Title { get => m_title; }
        public VideoClip Video { get => m_video; }
        public bool EnableSampleArea { get => m_enableSampleArea; }

        public int ID { get => m_stepID; set => m_stepID = value; }
        public float TimeElapsed { get => m_timeElapsed; set => m_timeElapsed = value; }
        #endregion

        #region Constructors
        public ScenarioStep(ScenarioStep other)
        {
            m_title = other.Title;
            m_video = other.Video;
            m_enableSampleArea = other.EnableSampleArea;
            m_stepID = other.ID;
        }
        #endregion
    }
}

