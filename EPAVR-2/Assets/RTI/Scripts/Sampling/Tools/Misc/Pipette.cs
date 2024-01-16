using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Pipette : SampleCollector
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private Transform m_samplePoint;
        [SerializeField] private Animator m_anim;
        [SerializeField] private MeshRenderer m_fillMesh;
        [SerializeField] private ParticleSystem m_splashEffect;
        [SerializeField] private Transform m_dilutePoint;
        [Header("Default Configuration")]
        [SerializeField] private float m_collectRadius = 0.015f;
        [SerializeField] [Range(0.01f, 1.0f)] private float m_collectAmount = 1.0f;
        [SerializeField] private float m_minCollectableAmount = 50.0f;
        [SerializeField] private float m_maxSampleConcentration = 100.0f;
        [SerializeField] LayerMask m_sampleAreaMask = -1;
        [SerializeField] private LayerMask m_collectBlockMask;
        #endregion
        #region Private Variables

        private LayerMask m_sampleDiluteMask;

        private Action<Sample> m_onEmptySample;
        #endregion
        #region Public Properties

        public Action<Sample> OnEmptySample { get => m_onEmptySample; set => m_onEmptySample = value; }
        #endregion

        #region Initialization
        public void Init()
        {
            // Set up layer mask
            //m_sampleAreaMask = LayerMask.GetMask("Sample");
            // Hook up events
            m_onSetSample += i => 
            {
                m_fillMesh.gameObject.SetActive(true);
                m_fillMesh.material.color = i.Chemical.SpawnColor;
                m_splashEffect.startColor = i.Chemical.SpawnColor;
            };

            m_onEmptySample += i => 
            {
                m_fillMesh.gameObject.SetActive(false);
                m_splashEffect.Play(); 
            };
        }
        #endregion

        #region Sample-Related Functionality
        public void AttemptCollectSample()
        {
            // Spherecast to get a sampleable object
            //if (Physics.SphereCast(m_samplePoint.position, m_collectRadius, m_samplePoint.forward, out RaycastHit hit, m_collectRadius, m_sampleAreaMask))
            Ray ray = new Ray(m_samplePoint.position, m_samplePoint.forward);
            if (Physics.Raycast(m_samplePoint.position, m_samplePoint.forward, out RaycastHit hit, m_collectRadius, m_sampleAreaMask))
            {
                // Cache collider reference
                Collider col = hit.collider;
                UnityEngine.Debug.Log($"{gameObject.name} attempting to collect sample from hit object: {hit.transform.gameObject.name} || Time: {Time.time}");
                // Check if this is a container
                if (col.TryGetComponent<ISampleContainer>(out ISampleContainer container) && container.CurrentSample != null && container.CurrentSample.Type == MatterType.Liquid && container.CanTransferSample)
                {
                    UnityEngine.Debug.Log($"Attempting to collect from container: {col.gameObject.name} | Sample: {container.CurrentSample.Chemical} || Time: {Time.time}");
                    CollectSample(container);
                    container.ClearSample();
                    return;
                }
                else
                {
                    // Try to sample a color from this
                    if (PaintManager.Instance && PaintManager.Instance.HasKey(col))
                    {
                        Paintable paintable = PaintManager.Instance.GetPaintable(hit.collider);
                        if (paintable.IsInitialized && paintable.MeshCollider != null)
                        {
                            if (paintable.MeshCollider.Raycast(ray, out RaycastHit meshHit, m_collectRadius))
                            {
                                Color sampledColor = PaintManager.Instance.Sample(meshHit.textureCoord, paintable);
                                UnityEngine.Debug.Log($"Pipette collected color sample: {hit.collider.gameObject.name} | Color: {sampledColor} || Time: {Time.time}");
                                if (sampledColor.a > 0.1f)
                                {
                                    UnityEngine.Debug.Log($"Pipette passed alpha test: {hit.collider.gameObject.name} | Value: {sampledColor.a} || Time: {Time.time}");

                                    Contamination chemObject = GetClosestContamination(col, sampledColor);
                                    if (chemObject != null) // && chemObject.Concentration >= m_minCollectableAmount
                                    {
                                        // Collect the sample
                                        CollectSample(chemObject);
                                        return;
                                    }
                                }
                            }
                        }
                        //Color sampledColor = PaintManager.Instance.Sample(hit);
                    }
                    /*
                    Contamination chemObject = GetClosestContamination(col, hit.point);
                    if (chemObject != null && chemObject.Concentration >= m_minCollectableAmount)
                    {
                        UnityEngine.Debug.Log($"{gameObject.name} found closest contamination on object: {chemObject.gameObject.name} | Agent: {chemObject.Agent.Name} || Time: {Time.time}");
                        // Collect the sample
                        CollectSample(chemObject);
                        return;
                    }
                    */
                }
                /*
                // Otherwise, check if this is a contaminated surface
                else if (col.TryGetComponent<Contamination>(out Contamination chemObject) && chemObject.Type == MatterType.Liquid && chemObject.Concentration >= m_minCollectableAmount)
                {
                    // Collect the sample
                    //CollectSample(chemObject, hit.transform.InverseTransformPoint(hit.point));
                    CollectSample(chemObject);
                    return;
                }
                */
            }
            /*
            else
            {
                RaycastHit testHit;
                if (Physics.Raycast(m_samplePoint.position, m_samplePoint.forward, out testHit, m_collectRadius, m_sampleAreaMask))
                {
                    UnityEngine.Debug.Log($"{gameObject.name} failed to collect sample within specified range and radius but found object: {testHit.transform.gameObject.name} || Time: {Time.time}");
                }
                else
                {
                    UnityEngine.Debug.Log($"{gameObject.name} completely failed to collect sample within specified range and radius || Time: {Time.time}");
                }
                
            }
            */
        }
        /*
        public void AttemptCollectSample()
        {
            // Collect any potential sample areas from the pipette's tip
            Collider[] potentialSampleObjects = Physics.OverlapSphere(m_samplePoint.position, m_collectRadius, m_sampleAreaMask);
            // Make sure there are sample objects
            if (potentialSampleObjects.Length > 0)
            {
                UnityEngine.Debug.Log($"Found possible liquid samples: {potentialSampleObjects.Length} || Time: {Time.time}");
                // Set up looping variables
                //ChemLiquidObject closestLiquid = null;
                Contamination closestLiquid = null;
                float closestDistance = float.MaxValue;
                // First and foremost, check if there is a valid container nearby
                for (int i = 0; i < potentialSampleObjects.Length; i++)
                {
                    // Get the collider
                    Collider col = potentialSampleObjects[i];
                    if (col.TryGetComponent<ISampleContainer>(out ISampleContainer container) && container.Sample != null && container.Sample.Type == MatterType.Liquid && container.CanTransferSample)
                    {
                        UnityEngine.Debug.Log($"Attempting to collect from container: {col.gameObject.name} | Sample: {container.Sample.Chemical} || Time: {Time.time}");
                        CollectSample(container);
                        container.ClearSample();
                        return;
                    }
                }
                // Loop through each collider and check if there is any object in the way
                for (int i = 0; i < potentialSampleObjects.Length; i++)
                {
                    // Get the collider
                    Collider col = potentialSampleObjects[i];
                    // Make sure the collider has a valid contamination
                    if (col.TryGetComponent<Contamination>(out Contamination chemObject) && chemObject.Type == MatterType.Liquid && chemObject.Concentration >= m_minCollectableAmount) // col.TryGetComponent<ChemLiquidObject>(out ChemLiquidObject chemObject)
                    {
                        // Get hit direction
                        RaycastHit sampleHit;
                        Vector3 closestPoint = col.ClosestPointOnBounds(m_samplePoint.position);
                        //Vector3 centerToSampleCenterDirection = col.transform.position - m_samplePoint.position;
                        Vector3 centerToSampleCenterDirection = closestPoint - m_samplePoint.position;
                        if (Physics.Raycast(m_samplePoint.position, centerToSampleCenterDirection, out sampleHit, m_collectRadius * 4.0f, m_sampleAreaMask) || sampleHit.collider == col)
                        {
                            UnityEngine.Debug.Log($"Passed first raycast test: {col.gameObject.name} || Time: {Time.time}");
                            Vector3 hitDirection = sampleHit.point - m_samplePoint.position;
                            float hitDistance = MathHelper.QuickDistance(m_samplePoint.position, sampleHit.point);
                            // Check to make sure there is nothing between the sample site
                            RaycastHit blockHit;
                            if (!Physics.Raycast(m_samplePoint.position, hitDirection, out blockHit, hitDistance, m_collectBlockMask) || blockHit.collider == col)
                            {
                                UnityEngine.Debug.Log($"Passed second raycast test: {col.gameObject.name} || Time: {Time.time}");
                                // Compare the hit distance
                                if (hitDistance < closestDistance)
                                {
                                    closestDistance = hitDistance;
                                    closestLiquid = chemObject;
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.Log($"Failed second raycast test: {col.gameObject.name} | Blocker: {blockHit.transform.name} || Time: {Time.time}");
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.Log($"Failed first raycast test: {col.gameObject.name} || Time: {Time.time}");
                            if (sampleHit.collider != null)
                            {
                                UnityEngine.Debug.Log($"First raycast test failed due to {sampleHit.collider.gameObject.name} || Time: {Time.time}");
                            }
                        }
                    }
                }

                if (closestLiquid != null) CollectSample(closestLiquid);
            }
        }
        */

        public void CollectSample(SampleArea _sampleArea)
        {
            // Check to make sure this sample area has a contaminant
            if (_sampleArea.Contaminant == null) return;
            // Create a new sample
            Sample sample = new Sample(_sampleArea, _sampleArea.Contaminant, m_collectAmount * 100.0f);
            SetSample(sample);
        }

        public void CollectSample(ChemLiquidObject _liquid)
        {
            UnityEngine.Debug.Log($"Arrived in CollectSample: {_liquid.gameObject.name} || Time: {Time.time}");
            // Make sure the liquid has a valid chemical agent
            if (_liquid.Agent == null) return;
            // Create a new sample
            Sample sample = new Sample(_liquid.Agent, m_collectAmount * 100.0f);
            SetSample(sample);
        }

        public void CollectSample(Contamination _liquid)
        {
            UnityEngine.Debug.Log($"Arrived in CollectSample: {_liquid.gameObject.name} || Time: {Time.time}");
            // Make sure the liquid has a valid chemical agent
            if (_liquid.Agent == null) return;
            // Create a new sample
            Sample sample = new Sample(_liquid, m_collectAmount * 100.0f);
            SetSample(sample);
        }

        public void CollectSample(Contamination _liquid, Vector3 _collectPosition)
        {
            UnityEngine.Debug.Log($"Arrived in CollectSample: {_liquid.gameObject.name} || Time: {Time.time}");
            // Make sure the liquid has a valid chemical agent
            if (_liquid.Agent == null) return;
            // Get the contaminant amount
            float amount = Mathf.Clamp(_liquid.GetConcentrationFromPoint(_collectPosition), 0.0f, m_maxSampleConcentration);
            // Create a new sample
            Sample sample = new Sample(_liquid, amount * m_collectAmount);
            SetSample(sample);
        }

        public void CollectSample(ISampleContainer _container)
        {
            if (_container.CurrentSample == null || _container.CurrentSample.Type != MatterType.Liquid) return;
            // Create a new sample
            Sample sample = new Sample(_container.CurrentSample);
            SetSample(sample);
        }

        public void SetSample(Sample _newSample)
        {
            // Set reference
            m_currentSample = _newSample;
            // Invoke any necessary events
            m_onSetSample?.Invoke(m_currentSample);
        }

        public void EmptySample()
        {
            if (m_currentSample == null) return;
            // Check if there are any objects that the chemical can be diluted to
            Collider[] potentialSampleContainerObjects = Physics.OverlapSphere(m_dilutePoint.position, m_collectRadius);
            if (potentialSampleContainerObjects.Length > 0)
            {
                UnityEngine.Debug.Log($"EmptySample - Found Possible Containers: {potentialSampleContainerObjects.Length} || Time: {Time.time}");
                // Loop through each container object and see if a sample can be diluted to it
                for (int i = 0; i < potentialSampleContainerObjects.Length; i++)
                {
                    // Cache reference
                    Collider col = potentialSampleContainerObjects[i];
                    //UnityEngine.Debug.Log($"[{i}]: {col.gameObject.name} || Time: {Time.time}");
                    // Try to get a container component
                    if (col.transform.TryGetComponent<ISampleContainer>(out ISampleContainer sampleContainer) && sampleContainer.Properties.CanHoldType(MatterType.Liquid) && sampleContainer.CurrentSample == null)
                    {
                        UnityEngine.Debug.Log($"EmptySample - Found Container: {col.gameObject.name} || Time: {Time.time}");
                        // Copy the sample to this container
                        sampleContainer.SetSample(new Sample(m_currentSample));
                    }
                }
            }

            // Invoke any necessary events
            m_onEmptySample?.Invoke(m_currentSample);
            m_currentSample = null;
        }

        public void ClearSample()
        {
            // Clear the reference
            m_currentSample = null;
        }
        #endregion

        #region Interaction-Related Functionality
        public void OnInteract()
        {
            // Play animation
            m_anim.SetTrigger("ForceSqueeze");
            // Check if there is a valid current sample
            if (m_currentSample == null)
                AttemptCollectSample();
            else
                EmptySample();
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            Init();
        }
    }
}

