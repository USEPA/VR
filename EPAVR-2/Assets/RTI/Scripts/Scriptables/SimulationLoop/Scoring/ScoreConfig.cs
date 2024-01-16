using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Scoring Configuration")]
    public class ScoreConfig : ScriptableObject
    {
        #region Inspector Assigned Variables
        [Header("Individual Default Score Values")]
        [SerializeField] private int m_siteClearScore = 100;
        [SerializeField] private int m_maxTimeBonus = 300;
        [Header("Penalty Configuration")]
        [SerializeField] private int m_unsafeChemicalContactPenalty = 20;
        [SerializeField] private int m_maxUnsafeChemicalContactPenalty = 200;
        #endregion
        #region Public Properties
        public int SiteClearValue { get => m_siteClearScore; }
        public int MaxTimeBonusValue { get => m_maxTimeBonus; }

        public int UnsafeChemicalContactPenaltyValue { get => m_unsafeChemicalContactPenalty; }
        public int MaxUnsafeChemicalContactPenaltyValue { get => m_maxUnsafeChemicalContactPenalty; }
        #endregion
    }

    public enum ScoreType {SitesCleared, TimeBonus, Penalty, EstimatedSpread, EstimatedEpicenter}
    public enum PenaltyType {AbandonedEquipment, UnsafeChemicalHandling, UnsafeCumulativeDose}
}

