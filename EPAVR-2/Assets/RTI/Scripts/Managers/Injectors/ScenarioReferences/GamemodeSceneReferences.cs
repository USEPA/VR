using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class GamemodeSceneReferences : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected List<ContaminationSite> m_possibleContaminantSites;
        [SerializeField] protected List<ContaminationSite> m_presetContaminantSites;
        #endregion
        #region Public Properties
        public List<ContaminationSite> PossibleContaminantSites { get => m_possibleContaminantSites; }
        public List<ContaminationSite> PresetContaminantSites { get => m_presetContaminantSites; }

        public abstract Gamemode Gamemode { get; }
        #endregion
    }
}

