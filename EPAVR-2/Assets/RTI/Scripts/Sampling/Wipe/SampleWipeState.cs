using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class SampleWipeState : MonoBehaviour
    {
        #region Protected Variables
        protected SampleWipe m_parent;
        protected WipeMode m_mode;

        protected Collider m_collider;
        protected TriggerEventListener m_triggerListener;

        protected List<SampleUnitCollision> sampleCollisionQueue;
        #endregion
        #region Public Properties
        public SampleWipe Parent { get => m_parent; }
        public WipeMode Mode { get => m_mode; }
        public Collider Collider { get => m_collider; }
        public TriggerEventListener TriggerListener { get => m_triggerListener; }
        #endregion

        #region Initialization
        public void Init(SampleWipe _parent, WipeMode _mode, Collider _collider)
        {
            // Cache references
            m_parent = _parent;
            m_mode = _mode;
            m_collider = _collider;
            //m_triggerListener = _triggerListener;
            // Initialize sample collision queue
            sampleCollisionQueue = new List<SampleUnitCollision>();
        }
        #endregion

        #region State-Related Functionality
        public void OnStateEnter()
        {
            // Enable the game object
            gameObject.SetActive(true);
            /*
            // Enable the trigger listener
            m_triggerListener.OnTriggerEntered += i => ProcessTriggerEnter(i);
            m_triggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            m_triggerListener.gameObject.SetActive(true);
            */
        }

        public void OnStateExit()
        {
            // Disable the game object
            gameObject.SetActive(false);
            /*
            // Disable the trigger listener
            m_triggerListener.OnTriggerEntered -= i => ProcessTriggerEnter(i);
            m_triggerListener.OnTriggerExited -= i => ProcessTriggerExit(i);
            m_triggerListener.gameObject.SetActive(false);
            */
        }
        #endregion

        #region Sample-Related Functionality
        public void ClearSampleQueue()
        {
            // Clear the queue
            sampleCollisionQueue.Clear();
        }
        #endregion

        #region Trigger-Related Functionality
        public void ProcessTriggerEnter(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj)) 
            {
                UnityEngine.Debug.Log($"{gameObject.name} ProcessTriggerEnter: {unitObj.gameObject.name} || Time: {Time.time}");
                if (sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit) < 0)
                {
                    // Calculate point of impact/direction
                    Vector3 hitPoint = m_parent.Collider.ClosestPoint(unitObj.transform.position);
                    Vector3 hitDirection = (m_parent.transform.position - hitPoint).normalized;
                    //Vector3 hitDirection = direction;
                    //unitObj.Unit.SetIsSample(true, 0.0f);
                    unitObj.Unit.SetCleared(true);
                    //unitObj.SetColor();
                    //if (SimulationManager.Instance.DebugMode) unitObj.SetColor();
                    // Add this to the collisions per update
                    sampleCollisionQueue.Add(new SampleUnitCollision(unitObj.Unit, hitPoint, hitDirection));
                    //sampleCollisionsPerUpdate++;
                    // Check whether or not there is a sample associated with this
                    if (unitObj.Source != null)
                    {
                        if (m_parent.CurrentSample == null || m_parent.CurrentSample.Source != unitObj.Source)
                        {
                            UnityEngine.Debug.Log($"{m_parent.gameObject.name} arrived before SetSample logic | Contacted Unit: {unitObj.gameObject.name} || Time: {Time.time}");
                            m_parent.SetSample(new Sample(unitObj.Source, (unitObj.Amount * m_parent.WipeStateCountFactor)));
                        }
                        else
                        {
                            m_parent.CurrentSample.Amount += (unitObj.Amount * m_parent.WipeStateCountFactor);
                        }
                    }
                }
            }
        }

        public void ProcessTriggerExit(Collider other)
        {
            // Validate it
            if (other.TryGetComponent<SampleUnitObject>(out SampleUnitObject unitObj))
            {
                int index = sampleCollisionQueue.FindIndex(i => i.Unit == unitObj.Unit);
                if (index >= 0)
                {
                    // Calculate point of exit
                    Vector3 exitPoint = m_parent.Collider.ClosestPointOnBounds(unitObj.transform.position);
                    Vector3 exitDirection = (m_parent.transform.position - exitPoint).normalized;
                    sampleCollisionQueue[index].SetExitDirection(exitDirection);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            ProcessTriggerExit(other);
        }
        #endregion
    }
}

