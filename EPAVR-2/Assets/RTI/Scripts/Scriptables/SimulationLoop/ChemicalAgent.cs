using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [CreateAssetMenu(menuName = "Gameplay Loop/Chemical Agent")]
    public class ChemicalAgent : Agent
    {
        #region Inspector Assigned Variables
        [Header("Chemical Identification")]
        [SerializeField] protected string m_fullName = default;
        [SerializeField] protected string m_abbreviation = default;
        [SerializeField] protected Vector3Int m_casRegistryNumber = default;
        [Header("Chemical Information")]
        [SerializeField] protected HazardProperties m_hazardProperties = default;
        [Header("Spawn Configuration")]
        [SerializeField] protected List<MatterType> m_spawnableTypes;
        [SerializeField] protected MatterType m_spawnableType;
        [SerializeField] protected bool m_isSpawnableLiquid = false;
        [SerializeField] protected bool m_isSpawnableSolid = false;
        [Header("Misc. Configuration")]
        [SerializeField] protected Color m_spawnColor;
        #endregion
        #region Public Properties
        public override Gamemode Mode => Gamemode.ChemicalHunt;
        public override string Name { get => m_fullName; }
        public override string Abbreviation { get => m_abbreviation; }
        public string CAS_RN { get => $"{m_casRegistryNumber.x}-{m_casRegistryNumber.y}-{m_casRegistryNumber.z}"; }
        public HazardProperties HazardProperties { get => m_hazardProperties; }

        public List<MatterType> SpawnableTypes { get => m_spawnableTypes; }
        public bool IsSpawnableLiquid { get => m_isSpawnableLiquid; }
        public bool IsSpawnableSolid { get => m_isSpawnableSolid; }
        public Color SpawnColor { get => m_spawnColor; }
        #endregion
    }

    [System.Serializable]
    public class HazardProperties
    {
        #region Inspector Assigned Variables
        [SerializeField] protected bool m_isHazardous = false;
        [SerializeField] protected bool m_isWashable = false;
        [SerializeField] protected bool m_isExplosive = false;
        [SerializeField] protected bool m_isFlammable = false;
        #endregion
        #region Public Properties
        public bool IsHazardous { get => m_isHazardous; }
        public bool IsWashable { get => m_isWashable; }
        public bool IsExplosive { get => m_isExplosive; }
        public bool IsFlammable { get => m_isFlammable; }
        #endregion
    }
}

