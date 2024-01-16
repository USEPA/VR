using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class SampleTool : MonoBehaviour
    {
        #region Protected Variables
        protected SampleArea m_currentSampleArea;
        protected XRToolbeltItem m_socketItem;
        protected Action<Sample> m_onIdentifyAgent;
        #endregion
        #region Public Properties
        public abstract ToolType Type { get; }
        public XRToolbeltItem SocketItem
        {
            get
            {
                if (!m_socketItem) m_socketItem = GetComponent<XRToolbeltItem>();
                return m_socketItem;
            }
        }
        public SampleArea CurrentSampleArea { get => m_currentSampleArea; }

        public virtual float CurrentReading { get; }
        public virtual float MaxReading { get; }
        public Action<Sample> OnIdentifyAgent { get => m_onIdentifyAgent; set => m_onIdentifyAgent = value; }
        #endregion

        #region Initialization
        public abstract void Init();

        public virtual List<GameObject> SpawnAdditionalObjects()
        {
            return null;
        }

        public virtual void OnSpawn()
        {
            // Do the thing
        }

        public virtual void OnDespawn()
        {
            // Do the thing
        }
        #endregion

        #region Sample-Related Functionality
        public virtual void IdentifyChemical(Sample _sample)
        {
            // Invoke necessary events
            m_onIdentifyAgent?.Invoke(_sample);
            // Relay identification to Contamination component
            _sample.Source?.OnSampleIdentified(_sample);
        }
        public virtual void SetSampleArea(SampleArea _area)
        {
            m_currentSampleArea = _area;
            if (m_currentSampleArea != null) m_currentSampleArea.Tool = this;
        }

        public abstract SampleReportOld GenerateReport(ScenarioStep _step);

        #endregion

        #region Helper Methods
        public virtual void ForceToggleInView()
        {
            // Do the thing
        }

        public virtual void ReturnToBelt()
        {
            if (!SocketItem.HasLinkedSocket || SocketItem.LinkedSocket != VRUserManager.Instance.Player.MainDeviceAttachPoint) return;
            SocketItem.ForceAttachToSocket(VRUserManager.Instance.Player.MainDeviceAttachPoint);
        }

        public virtual int GetAdditionalActiveItemCount()
        {
            int activeItemCount = 0;
            if (SocketItem != null && !SocketItem.IsAttached) activeItemCount++;
            return activeItemCount;
        }
        #endregion
    }
}

