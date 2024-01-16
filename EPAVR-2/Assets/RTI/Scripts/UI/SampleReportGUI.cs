using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class SampleReportGUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] Text stepTitle = default;
        [SerializeField] Text stepResults = default;
        #endregion
        #region Private Variables
        private SampleReportOld _currentReport;
        #endregion
        #region Public Properties
        public SampleReportOld CurrentReport { get => _currentReport; }
        #endregion

        public void UpdateReport(SampleReportOld _report)
        {
            stepTitle.text = $"{_report.Step.Title} Results";
            var percentCompletion = (_report.TotalSampleUnits > 0) ? $"{(_report.PercentCompletion * 100.0f).ToString("#.##")}" : "0";
            stepResults.text = $"Total Sample Units: {_report.TotalSampleUnits} | Percent Completion: {percentCompletion}%";
        }
    }
}

