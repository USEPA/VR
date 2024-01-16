using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace L58.EPAVR
{
    public class TraceSampleSwab : XRGrabInteractable, ISampleContainer, IDisposable
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected Collider m_sampleCollider;
        [SerializeField] protected TextMeshPro m_contaminantValueDisplay;
        [SerializeField] protected MeshRenderer m_contaminantValueMesh;
        [Header("Default Configuration")]
        [SerializeField] protected Vector2 m_heldContaminationRange = new Vector2(0.0f, 100.0f);
        [SerializeField] protected ContainerProperties m_containerProperties;
        #endregion
        #region Protected Variables
        protected MXTraceSampler m_traceSampler;
        protected int m_spawnIndex = -1;
        public float m_contamination = 0.0f;

        protected Sample m_sample;
        protected ChemicalAgent m_contaminant;

        protected TriggerEventListener m_triggerListener;

        public bool m_atAttachPoint = false;
        protected Collider m_attachPointCollider;
        protected bool m_analyzed = false;
        protected Action<float> m_onSetContamination;
        #endregion
        #region Public Properties
        public bool Active { get; set; } = false;

        public MXTraceSampler TraceSampler { get => m_traceSampler; }
        public int SpawnIndex { get => m_spawnIndex; }
        public float Contamination { get => m_sample.Amount; }

        public Sample CurrentSample { get => m_sample; }
        public ChemicalAgent Contaminant { get => m_sample.Chemical; }

        public bool AtAttachPoint { get => m_atAttachPoint; set => m_atAttachPoint = value; }
        public Collider AttachPointCollider { get => m_attachPointCollider; set => m_attachPointCollider = value; }

        public ContainerProperties Properties { get => m_containerProperties; }
        public bool CanTransferSample => false;

        public bool Analyzed { get => m_analyzed; set => m_analyzed = value; }

        public XRGrabInteractable Interactable { get => this; }

        public Action OnDisposed { get; set; }
        #endregion

        #region Initialization
        public void Init(MXTraceSampler _sampler, int _spawnIndex)
        {
            // Set references
            m_traceSampler = _sampler;
            m_spawnIndex = _spawnIndex;

            // Check if the trigger listener needs to be created
            if (!m_triggerListener)
            {
                m_triggerListener = m_sampleCollider.gameObject.AddComponent<TriggerEventListener>();
                m_triggerListener.Init();

                m_triggerListener.OnTriggerEntered += i => ProcessTriggerStay(i);
                m_triggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
            }
        }
        #endregion

        /*public void LateUpdate()
        {
            if (!isSelected) return;

            Vector3 offsetRotation = transform.eulerAngles + m_holdRotationOffset;
            transform.eulerAngles = offsetRotation;
        }*/

        #region Sample-Related Functionality
        public void SetSample(Sample _sample)
        {
            UnityEngine.Debug.Log($"{gameObject.name} set sample: {_sample.Chemical} | Amount: {_sample.Amount} | Source: {_sample.Source.gameObject.name} || Time: {Time.time}");
            // Copy sample
            m_sample = new Sample(_sample);
            SetContaminantLevel(_sample.Amount);
        }

        public void ClearSample()
        {
            // Do nothing, sample cannot be cleared on this object
        }
        #endregion

        #region Contaminantion Functionality
        public void DiluteToSwab(TraceSampleSwab _otherSwab)
        {
            // Make sure base conditions are valid
            if (!_otherSwab.Active || m_sample == null || m_sample.Amount <= 0.0f) return;
            UnityEngine.Debug.Log($"{gameObject.name} - arrived in DiluteToSwab with Sample: {m_sample.Chemical} | Current Amount: {m_sample.Amount}|| Time: {Time.time}");
            // Check for cross contamination
            if (!m_sample.IsCrossContaminated && _otherSwab.CurrentSample != null && _otherSwab.CurrentSample.Chemical != null && (m_sample.Chemical != _otherSwab.CurrentSample.Chemical))
            {
                // This sample is now cross-contaminated
                UnityEngine.Debug.Log($"{gameObject.name} was cross-contaminated by {_otherSwab.gameObject.name} | Current Sample: {m_sample.Chemical} | Other Sample: {_otherSwab.CurrentSample.Chemical} || Time: {Time.time}");
                m_sample.IsCrossContaminated = true;
            }
            // Calculate the diluted contaminant level
            float otherContaminationLevel = (_otherSwab.CurrentSample != null) ? _otherSwab.Contamination : 0.0f;
            float dilutedValue = (m_sample.Amount + otherContaminationLevel) * 0.5f;
            // Assign the contaminant levels
            SetContaminantLevel(dilutedValue);
            // Copy contaminant reference
            _otherSwab.SetSample(m_sample);
            //SetSample(_otherSwab.Sample);
            //SetContaminant(_otherSwab.Contaminant);

            //_otherSwab.SetContaminantLevel(dilutedValue);
        }

        public void SetContaminantLevel(float _value)
        {
            // Set value
            m_sample.Amount = Mathf.Clamp(_value, m_heldContaminationRange.x, m_heldContaminationRange.y);
            UnityEngine.Debug.Log($"{gameObject.name} - arrived in SetContaminantLevel | Original Value: {_value} | Clamped Value: {m_sample.Amount} || Time: {Time.time}");
            //m_contamination = _value;
            // Enable display if necessary
            //if (!m_contaminantValueDisplay.gameObject.activeInHierarchy) m_contaminantValueDisplay.gameObject.SetActive(true);
            if (m_contaminantValueMesh)
            {
                Color targetColor = m_contaminantValueMesh.material.color;
                float alpha = Mathf.Clamp(m_sample.Amount * 0.01f, 0.0f, 1.0f);
                targetColor.a = alpha;
                m_contaminantValueMesh.material.color = targetColor;
            }
            // Update the display value
            //m_contaminantValueDisplay.text = $"{m_contamination.ToString("F2")}%";
            // Invoke any necessary functionality
            m_onSetContamination?.Invoke(m_sample.Amount);
        }

        public void SetContaminant(ChemicalAgent _contaminant)
        {
            UnityEngine.Debug.Log($"{gameObject.name} set contaminant: {_contaminant} || Time: {Time.time}");
            m_contaminant = _contaminant;
        }
        #endregion

        #region Interaction Logic
        public void OnGrabSwab()
        {
            // Send a message that this swab has been grabbed
            m_traceSampler.GrabSwab(this);
        }

        public void OnDropSwap()
        {
            // Check if this is in the attach point
            if (m_atAttachPoint) 
            {
                if (m_attachPointCollider == m_traceSampler.SampleSwabInsertCollider)
                    InsertSwab();
                else if (m_traceSampler.DeliveryBox != null && m_attachPointCollider == m_traceSampler.DeliveryBox.Trigger)
                    DeliverSwab();
            }
            
        }

        void InsertSwab()
        {
            m_traceSampler.InsertSwab(this);
            m_contaminantValueDisplay.gameObject.SetActive(false);
        }

        void DeliverSwab()
        {
            m_traceSampler.DeliveryBox.DeliverSwab(this);
            m_contaminantValueDisplay.gameObject.SetActive(true);
        }
        #endregion

        #region Collision Logic
        public void ProcessTriggerStay(Collider other)
        {
            if (!Active) return;

            // Get the tag
            switch (other.transform.tag)
            {
                case "TraceSampleSwab":
                    // Try to get sample swab component
                    if (other.transform.parent.parent.parent.TryGetComponent<TraceSampleSwab>(out TraceSampleSwab otherSwab) && otherSwab.Active)
                    {
                        UnityEngine.Debug.Log($"{gameObject.name} hit other trace sample swab || Time: {Time.time}");
                        // Attempt to dilute the contamination
                        DiluteToSwab(otherSwab);
                    }
                    break;
                /*case "SampleArea":
                    // Try to get sample area
                    if (other.TryGetComponent<SampleSurfaceArea>(out SampleSurfaceArea sampleArea))
                    {
                        // Get contaminant from sample area
                        SetContaminant(sampleArea.Contaminant);
                        // Simply set contaminant to 100%
                        SetContaminantLevel(100);
                    }
                    break;*/
                case "AttachPoint":
                    //UnityEngine.Debug.Log($"{gameObject.name} entered attach point: {other.gameObject.name} || Time: {Time.time}");
                    if (m_analyzed == true)
                    {
                        /*
                        if (m_traceSampler.DeliveryBox == null)
                        {
                            UnityEngine.Debug.Log($"EnterAttachPoint - Delivery Box was null! || Time: {Time.time}");
                        }
                        else if (other != m_traceSampler.DeliveryBox.Trigger)
                        {
                            UnityEngine.Debug.Log($"EnterAttachPoint - Collider ({other.gameObject.name}) was not Insert Trigger! || Time: {Time.time}");
                        }
                        */
                    }
                    if (other == m_traceSampler.SampleSwabInsertCollider || (m_analyzed == true && m_traceSampler.DeliveryBox != null && other == m_traceSampler.DeliveryBox.Trigger)) 
                    {
                        m_attachPointCollider = other;
                        m_atAttachPoint = true;
                        //UnityEngine.Debug.Log($"{gameObject.name} actually entered attach point: {m_attachPointCollider.gameObject.name} || Time: {Time.time}");
                    }
                    break;
                default:
                    // Try to get sample area
                    if (other.TryGetComponent<Contamination>(out Contamination contamination))
                    {
                        Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);
                        float contaminationAmount = contamination.GetConcentrationFromPoint(closestPoint);
                        UnityEngine.Debug.Log($"{gameObject.name} hit contaminated surface: {contamination.gameObject.name} | Contaminant: {contamination.Agent} |  Concentration: {contaminationAmount} || Time: {Time.time}");
                        // Get contaminant from sample area
                        if (m_sample == null)
                        {
                            SetSample(new Sample(contamination));
                        }
                        else
                        {
                            // Check for cross-contamination
                            if (m_sample.Chemical != null && m_sample.Chemical != contamination.Agent) m_sample.IsCrossContaminated = true;
                        }
               
                        //SetContaminant(contamination.Agent);
                        // Simply set contaminant to 100%
                        //SetContaminantLevel(100);
                    }
                    break;
            }
        }

        private void ProcessTriggerExit(Collider other)
        {
            if (!Active) return;

            // Get the tag
            switch (other.transform.tag)
            {
                case "AttachPoint":
                    if (m_atAttachPoint && other == m_traceSampler.SampleSwabInsertCollider || (m_analyzed == true && m_traceSampler.DeliveryBox != null && other == m_traceSampler.DeliveryBox.Trigger)) 
                    {
                        UnityEngine.Debug.Log($"{gameObject.name} exited attach point: {m_attachPointCollider.gameObject.name} || Time: {Time.time}");
                        m_attachPointCollider = null;
                        m_atAttachPoint = false;
                    }
        
                    break;
            }
        }
        #endregion

        public void Dispose()
        {
            OnDisposed?.Invoke();
        }

        public void OnDestroy()
        {
            if (m_traceSampler == null || m_traceSampler.ActiveSwabs == null || m_traceSampler.ActiveSwabs.Count < 1) return;

            //OnDisposed?.Invoke();

            if (m_traceSampler.ActiveSwabs.Contains(this))
            {
                m_traceSampler.ActiveSwabs.Remove(this);
            }

        }

        #region Helper Methods
#if UNITY_EDITOR
        [ContextMenu("Force Grab Swab")]
        public void ForceGrabSwab()
        {
            // Make sure that there is a valid device to insert into
            if (m_traceSampler == null) return;
            // Grab the swab
            OnGrabSwab();
        }
        [ContextMenu("Force Insert to Trace Sampler for Analysis")]
        public void ForceInsertForAnalysis()
        {
            // Make sure that there is a valid device to insert into
            if (m_traceSampler == null) return;
            // Insert the swab
            InsertSwab();
        }
        #endif
        #endregion
    }
}

