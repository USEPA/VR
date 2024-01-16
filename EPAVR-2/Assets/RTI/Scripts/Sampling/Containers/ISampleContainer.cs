using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public interface ISampleContainer
    {
        #region Public Properties
        public Sample CurrentSample { get; }
        public bool CanTransferSample { get; }
        public ContainerProperties Properties { get; }
        #endregion

        #region Sample Methods
        public void SetSample(Sample _sample);
        public void ClearSample();
        #endregion
    }

    [System.Serializable]
    public class ContainerProperties
    {
        #region Inspector Assigned Variables
        [SerializeField] bool m_canHoldSolid = true;
        [SerializeField] bool m_canHoldLiquid = true;
        [SerializeField] bool m_canHoldGas = false;

        [SerializeField] bool m_canContaminateHand = false;
        [SerializeField] bool m_allowCrossContamination = false;
        #endregion

        #region Public Properties
        public bool CanContaminateHand { get => m_canContaminateHand; }
        public bool AllowCrossContamination { get => m_allowCrossContamination; }
        #endregion

        public bool CanHoldType(MatterType _matterType)
        {
            switch (_matterType)
            {
                case MatterType.Solid:
                    return m_canHoldSolid;
                case MatterType.Liquid:
                    return m_canHoldLiquid;
                case MatterType.Gas:
                    return m_canHoldGas;
                default:
                    return false;
            }
        }
    }
}

