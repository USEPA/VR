using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class RamanLaserPointer : XRGrabInteractable, IGrabbable
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Transform m_laserStartPoint;
        [SerializeField] private LineRenderer m_laserLineRenderer;
        [SerializeField] private GameObject m_laserEffectContainer;
        [Header("Default Configuration")]
        [SerializeField] private LayerMask m_layerMask = -1;
        [SerializeField] private Color m_noContaminantColor;
        [SerializeField] private Color m_foundContaminantColor;
        #endregion
        #region Private Variables
        private GeminiRamanMode m_parent;
        private bool m_armed = false;
        private bool m_active = false;

        private float m_tickRate;
        private float m_maxTargetDistance;
        public float m_chemicalIdentifyTime;

        private int m_blockLayerMask = -1;
        public Collider m_currentHitCollider;
        private RaycastHit hit;
        public Contamination m_currentTarget;

        public SampleSurfaceArea m_sampleArea;

        public float m_chemicalIdentifiedTimer = -1.0f;
        public bool m_chemicalIdentified = false;
        private bool m_isFixedToViewRotation = false;
        private Light m_laserLight;

        private Action<bool> m_onSetActive;
        private Action<Sample> m_onConfirmAgent;
        #endregion
        #region Public Properties
        public bool Active
        {
            get => m_active;
            set
            {
                m_active = value;
                //m_laserLineRenderer.enabled = value;
                m_laserEffectContainer.SetActive(value);
                m_onSetActive?.Invoke(value);
            }
        }

        public bool Armed { get => m_armed; set => m_armed = value; }
        public bool IsFixedToViewRotation { get => m_isFixedToViewRotation; set => m_isFixedToViewRotation = value; }
        public Action<bool> OnSetActive { get => m_onSetActive; set => m_onSetActive = value; }
        public Action<Sample> OnConfirmAgent { get => m_onConfirmAgent; set => m_onConfirmAgent = value; }
        #endregion

        #region Initialization
        public void Init(GeminiRamanMode _parent)
        {
            // Cache references
            m_parent = _parent;
            // Set up layer mask
            //m_layerMask = LayerMask.GetMask("Sample", "Default");
            //m_blockLayerMask = LayerMask.GetMask("Default");
            // Set max target distance
            m_maxTargetDistance = m_parent.LaserMaxDistance;
            m_chemicalIdentifyTime = m_parent.LaserIdentificationTime;
            m_laserLineRenderer.SetPosition(0, Vector3.zero);
            m_laserLight = m_laserEffectContainer.GetComponentInChildren<Light>();
            //m_laserLight.range = m_maxTargetDistance;
            //SetSampleArea(null);
            SetTarget(null);
            // Disable the laser pointer by default
            Active = false;
            // Ignore collision with the main Gemini collider
            Physics.IgnoreCollision(GetComponent<Collider>(), _parent.Parent.GetComponent<Collider>(), true);
        }
        #endregion

        #region Sensor-Related Functionality
        public void Tick()
        {
            if (m_isFixedToViewRotation) FixToViewRotation();
            // Make sure sensor is active
            if (!m_active) return;

            // Set laser start point
            //m_laserLineRenderer.SetPosition(0, m_laserStartPoint.position);
            // Raycast from laser tip
            //Ray ray = new Ray(m_laserStartPoint.position, m_laserStartPoint.forward);
            /*
            // First check for any objects obstructing the laser
            if (!(Physics.Raycast(ray, out RaycastHit blockHit, m_maxTargetDistance, m_blockLayerMask)))
            {
                // Check for any samples within this area
            }
            */
            if (hit.collider != null)
            {
                /*
                // First, check if this is a container
                if (hit.collider.TryGetComponent<ChemLiquidContainer>(out ChemLiquidContainer liquidContainer))
                {

                }
                */
                // Do the normal check for a contaminated object
                if (hit.collider.TryGetComponent<Contamination>(out Contamination contamination))
                {
                    // Check if this is the first target contacted
                    if (!m_currentTarget || (m_currentTarget != null && (m_currentTarget != contamination)))
                    {
                        // Set this as the actiive target
                        SetTarget(contamination);
                    }

                    // Check if this is the current target
                    if (m_currentTarget == contamination)
                    {
                        if (!m_chemicalIdentified)
                        {
                            m_chemicalIdentifiedTimer += Time.deltaTime;

                            if (m_chemicalIdentifiedTimer >= m_chemicalIdentifyTime)
                            {
                                UnityEngine.Debug.Log($"Identified chemical || Time: {Time.time}");
                                m_chemicalIdentified = true;
                                m_chemicalIdentifiedTimer = 0.0f;
                                // Get the contaminant amount
                                float contaminationAmount = m_currentTarget.GetConcentrationFromPoint(hit.collider.ClosestPointOnBounds(hit.point));
                                m_onConfirmAgent?.Invoke(new Sample(m_currentTarget, contaminationAmount));
                                //m_onConfirmAgent?.Invoke(m_sampleArea, m_sampleArea.Contaminant);
                            }
                        }
                    }
                }
                else
                {
                    // Clear any current target
                    if (m_currentTarget) SetTarget(null);
                }
                //m_laserLineRenderer.SetPosition(1, m_laserStartPoint.InverseTransformPoint(hit.point));
                /*
                // Try to get a sample surface component from this surface
                if (hit.collider.TryGetComponent<SampleSurfaceArea>(out SampleSurfaceArea sampleArea))
                {
                    // Check if this is the first sample area contacted
                    if (!m_sampleArea || (m_sampleArea != null && (m_sampleArea != sampleArea)))
                    {
                        SetSampleArea(sampleArea);

                        // Identify the chemical
                        //if (m_sampleArea.Contaminant != null) m_onConfirmAgent?.Invoke(m_sampleArea.Contaminant);
                    }

                    if (m_sampleArea == sampleArea)
                    {
                        if (!m_chemicalIdentified)
                        {
                            m_chemicalIdentifiedTimer += Time.deltaTime;

                            if (m_chemicalIdentifiedTimer >= m_chemicalIdentifyTime)
                            {
                                UnityEngine.Debug.Log($"Identified chemical || Time: {Time.time}");
                                m_chemicalIdentified = true;
                                m_chemicalIdentifiedTimer = 0.0f;
                                m_onConfirmAgent?.Invoke(new Sample(m_sampleArea, m_sampleArea.Contaminant));
                                //m_onConfirmAgent?.Invoke(m_sampleArea, m_sampleArea.Contaminant);
                            }
                        }
                    }
                }
                else
                {
                    if (m_sampleArea) SetSampleArea(null);
                }
                */
            }
            else
            {
                //if (m_sampleArea) SetSampleArea(null);
                if (m_currentTarget) SetTarget(null);

                m_laserLineRenderer.SetPosition(1, m_laserStartPoint.InverseTransformPoint(m_laserStartPoint.position + (m_laserStartPoint.forward * m_maxTargetDistance)));
                //m_laserLineRenderer.SetPosition(1, m_laserStartPoint.position + (m_laserStartPoint.forward * m_maxTargetDistance));
            }
        }

        private void SetSampleArea(SampleSurfaceArea _sampleArea)
        {
            m_sampleArea = _sampleArea;
            m_parent.Parent.SetSampleArea(_sampleArea);
            if (m_sampleArea == null)
            {
                m_laserLineRenderer.startColor = m_noContaminantColor;
                m_laserLineRenderer.endColor = new Color(m_noContaminantColor.r, m_noContaminantColor.g, m_noContaminantColor.b, m_noContaminantColor.a * 0.75f);
                m_chemicalIdentified = false;
            }
            else
            {
                m_laserLineRenderer.startColor = m_foundContaminantColor;
                m_laserLineRenderer.endColor = m_foundContaminantColor;
                m_chemicalIdentified = false;
                m_chemicalIdentifiedTimer = 0.0f;
            }

        }

        public void SetTarget(Contamination _source)
        {
            m_currentTarget = _source;
            //m_parent.Parent.SetSampleArea(_sampleArea);
            if (m_currentTarget == null)
            {
                m_laserLineRenderer.startColor = m_noContaminantColor;
                m_laserLineRenderer.endColor = new Color(m_noContaminantColor.r, m_noContaminantColor.g, m_noContaminantColor.b, 0.0f);
                m_chemicalIdentified = false;
            }
            else
            {
                m_laserLineRenderer.startColor = m_foundContaminantColor;
                m_laserLineRenderer.endColor = m_foundContaminantColor;
                m_chemicalIdentified = false;
                m_chemicalIdentifiedTimer = 0.0f;
            }
        }

        public void ConfigureLineRenderer(bool _hasTarget)
        {
            if (_hasTarget)
            {
                m_laserLineRenderer.startColor = m_foundContaminantColor;
                m_laserLineRenderer.endColor = m_foundContaminantColor;
            }
            else
            {
                m_laserLineRenderer.startColor = m_noContaminantColor;
                m_laserLineRenderer.endColor = new Color(m_noContaminantColor.r, m_noContaminantColor.g, m_noContaminantColor.b, 0.0f); //m_noContaminantColor.a * 0.5f
            }
        }
        #endregion

        #region Interaction-Related Functionality
        public void ToggleActive()
        {
            Active = !Active;
        }

        public void OnGrab()
        {
            // Make sure device is armed
            //if (!m_armed) return;
            // Set active
            //Active = true;
        }

        public void OnDrop()
        {
            // Make sure device is armed
            //if (!m_armed) return;
            // Set inactive
            //Active = false;
        }


        protected override void Grab()
        {
            // Call base functionality
            base.Grab();
            // Invoke any specific grab functionality
            OnGrab();
        }

        protected override void Drop()
        {
            // Call base functionality
            base.Drop();
            // Invoke any specific drop functionality
            OnDrop();
        }
        #endregion

        #region Helper Methods
        public void FixToViewRotation()
        {
            //transform.rotation = Camera.main.transform.rotation;
            transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
        #endregion

        void Update()
        {
            if (!m_active) return;

            // Raycast from laser tip
            Ray ray = new Ray(m_laserStartPoint.position, m_laserStartPoint.forward);
            if (Physics.Raycast(ray, out hit, m_maxTargetDistance, m_layerMask))
            {
                // Set line renderer position to hit point
                m_laserLineRenderer.SetPosition(1, m_laserStartPoint.InverseTransformPoint(hit.point));
            }
            else
            {
                ConfigureLineRenderer(false);
                // Set line renderer position to max value
                m_laserLineRenderer.SetPosition(1, m_laserStartPoint.InverseTransformPoint(m_laserStartPoint.position + (m_laserStartPoint.forward * m_maxTargetDistance)));
            }
            //Tick();
        }
    }
}

