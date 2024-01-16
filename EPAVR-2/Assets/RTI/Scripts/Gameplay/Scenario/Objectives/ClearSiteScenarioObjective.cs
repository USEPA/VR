using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class ClearSiteScenarioObjective : ScenarioObjective
    {
        #region Protected Variables
        protected ContaminationSite m_site;
        #endregion
        #region Public Properties
        public override ObjectiveType Type => ObjectiveType.ClearSite;

        public ContaminationSite Site { get => m_site; }

        public override string ClearMessage => "Site Cleared";
        public override ScoreType ScoreType => ScoreType.SitesCleared;
        public override int ScorePoints => 100;
        #endregion

        #region Initialization
        public ClearSiteScenarioObjective(ScenarioInstance _parent, ContaminationSite _site) : base(_parent)
        {
            // Cache site reference
            m_site = _site;
            // Hook up event
            m_site.OnCleared.AddListener(Complete);
        }
        #endregion
    }
}

