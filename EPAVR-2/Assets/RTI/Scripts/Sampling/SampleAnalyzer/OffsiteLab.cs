using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class OffsiteLab : SampleAnalyzer
    {
        #region Public Properties
        public static OffsiteLab Instance { get; set; }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Static Methods
        public static void AddSampleRequest(SampleRequest _request)
        {
            if (!Instance) return;
            Instance.AddRequest(_request);
        }
        #endregion
    }
}

