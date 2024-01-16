using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public enum ToolType { Wipe, MX908, Gemini, Fluke}
    public class SampleToolOld : XRGrabInteractable
    {
        #region Inspector Assigned Variables
        [Header("Tool Configuration")]
        [SerializeField]
        protected ToolType m_toolType;
        #endregion
        #region Protected Variables
        protected Collider col;
        protected List<SampleUnitCollision> sampleCollisionQueue;
        public int sampleCollisionsPerUpdate = 0;

        protected Action<SampleReportOld> m_onSampleComplete;
        public Vector2 mousePosition;
        protected Vector3 prevPosition;
        protected Vector3 initialPosition;

        public Vector3 direction;

        protected SampleArea currentSampleArea;

        Camera mainCamera;
        float cameraDistance;
        #endregion
        #region Public Properties
        public List<SampleUnitCollision> SampleCollisionQueue { get => sampleCollisionQueue; }
        public int SampleCollisionsPerUpdateCount { get => sampleCollisionQueue.Count; }
        public SampleArea CurrentSampleArea { get => currentSampleArea; }
        public ToolType Type { get => m_toolType; }
        public bool Active { get; set; } = false;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Cache references
            col = GetComponent<Collider>();
            mainCamera = Camera.main;
            // Initialize sample collision queue
            sampleCollisionsPerUpdate = 0;
            sampleCollisionQueue = new List<SampleUnitCollision>();
            // Initialize directionality
            prevPosition = transform.position;
            initialPosition = transform.position;
            //SetSampleArea(m_defaultArea);
        }

        // Update is called once per frame
        void Update()
        {
            /*if (Active)
            {
                // Get camera distance
                cameraDistance = mainCamera.WorldToScreenPoint((transform.position)).z;
                // Get screen position and transform that to world position
                Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, cameraDistance);
                Vector3 desiredPosition = mainCamera.ScreenToWorldPoint(screenPosition);
                transform.position = new Vector3(desiredPosition.x, desiredPosition.y, transform.position.z);
            }*/

            direction = (transform.position - prevPosition).normalized;
            prevPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj))
            {
                if (sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit) < 0)
                {
                    // Calculate point of impact/direction
                    Vector3 hitPoint = col.ClosestPoint(unitObj.transform.position);
                    //Vector3 hitDirection = (transform.position - hitPoint).normalized;
                    Vector3 hitDirection = direction;
                    //unitObj.Unit.SetIsSample(true, 0.0f);
                    unitObj.Unit.SetCleared(true);
                    if (SimulationManager.Instance.DebugMode) unitObj.SetColor();
                    // Add this to the collisions per update
                    sampleCollisionQueue.Add(new SampleUnitCollision(unitObj.Unit, hitPoint, hitDirection));
                    sampleCollisionsPerUpdate++;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj))
            {
                int index = sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit);
                if (index >= 0)
                {
                    // Calculate point of exit
                    Vector3 exitPoint = col.ClosestPointOnBounds(unitObj.transform.position);
                    Vector3 exitDirection = (transform.position - exitPoint).normalized;
                    sampleCollisionQueue[index].SetExitDirection(exitDirection);
                }
            }
        }

        public SampleReportOld GenerateReport(ScenarioStep _step)
        {
            // Return report
            var report = new SampleReportOld(_step, sampleCollisionQueue, sampleCollisionQueue.Count, ((float) sampleCollisionQueue.Count / (float) currentSampleArea.TotalSamples));
            UnityEngine.Debug.Log(report.ToString());
            // Fire off action with this report
            m_onSampleComplete?.Invoke(report);
            // Reset queue
            ClearSampleQueue();
            return report;
        }

        public void ToggleActive()
        {
            Active = !Active;
        }

        public void SetSampleArea(SampleArea _area)
        {
            currentSampleArea = _area;
            //currentSampleArea.Tool = this;
        }

        public void ClearSampleQueue()
        {
            sampleCollisionQueue.Clear();
        }

        public void SetDebugPosition(Vector2 _position)
        {
            mousePosition = _position;
        }
    }



    public struct SampleReportOld
    {
        #region Public Properties
        public ScenarioStep Step { get; }
        public List<SampleUnitCollision> SampleCollisionQueue { get; }
        public int TotalSampleUnits { get; }
        public float PercentCompletion { get; }
        public float TimeElapsed { get; }
        #endregion

        public SampleReportOld(ScenarioStep _step, List<SampleUnitCollision> _sampleCollisionQueue, int _totalSampleUnits, float _percentCompletion)
        {
            Step = _step;
            SampleCollisionQueue = new List<SampleUnitCollision>(_sampleCollisionQueue);
            TotalSampleUnits = _totalSampleUnits;
            PercentCompletion = _percentCompletion;
            TimeElapsed = _step.TimeElapsed;
        }

        public override string ToString()
        {
            string percentCompletion = (TotalSampleUnits > 0) ? $"{(PercentCompletion * 100.0f).ToString("#.##")}" : "0";
            string indexDisplay = (TotalSampleUnits > 0) ? $"| First Item: {SampleCollisionQueue[0].Index.ToString()} | Last Item: {SampleCollisionQueue[SampleCollisionQueue.Count - 1].Index.ToString()} " : "";
            return $"{Step.Title} Report - Total Units: {TotalSampleUnits} | Percent Completion: {percentCompletion}% {indexDisplay}|| Time: {Time.time}";
        }
    }
}

