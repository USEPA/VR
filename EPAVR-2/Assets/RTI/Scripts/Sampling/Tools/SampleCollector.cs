using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public abstract class SampleCollector : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Sample Configuration")]
        [SerializeField] protected ContainerProperties m_containerProperties;
        #endregion
        #region Protected Variables
        protected Sample m_currentSample;

        protected Action<Sample> m_onSetSample;
        #endregion
        #region Public Properties
        public Sample CurrentSample { get => m_currentSample; }
        public ContainerProperties ContainerProperties { get => m_containerProperties; }

        public Action<Sample> OnSetSample { get => m_onSetSample; set => m_onSetSample = value; }
        #endregion


        #region Sample-Related Functionality
        public virtual Contamination GetClosestContamination(Collider _collider, Vector3 _point)
        {
            // Get any contaminant components
            Component[] surfaceChemicalComponents = _collider.GetComponents(typeof(Contamination));
            if (surfaceChemicalComponents.Length < 1) return null;
            // Set up looping variables
            Contamination closestContaminant = null;
            float closestDistance = float.MaxValue;
            // Loop through each contaminant and get the closest one
            for (int i = 0; i < surfaceChemicalComponents.Length; i++)
            {
                // Cache reference
                Contamination contamination = (Contamination) surfaceChemicalComponents[i];
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed initial check || Time: {Time.time}");
                // First, make sure that this is a valid collectable contaminant and that this point is actually within contaminant bounds
                if (!m_containerProperties.CanHoldType(contamination.Type) || !contamination.PointWithinContaminantBounds(_point)) continue;
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed bounds/container properties check || Time: {Time.time}");
                // Compare the distance to the epicenter
                float epicenterDistance = contamination.GetDistanceFromEpicenter(_point);
                if (epicenterDistance < closestDistance)
                {
                    // Set this as the closest contamination
                    closestDistance = epicenterDistance;
                    closestContaminant = contamination;
                }
            }
            // Return the closest contaminant (if it exists)
            return closestContaminant;
        }

        public virtual Contamination GetClosestContamination(Collider _collider, Color _color)
        {
            // Get any contaminant components
            List<Contamination> surfaceChemicalComponents = new List<Contamination>();
            Component[] sourceChemicalComponents = _collider.GetComponents(typeof(Contamination));

            if (sourceChemicalComponents.Length > 1)
            {
                foreach (Contamination sourceContamination in sourceChemicalComponents)
                    surfaceChemicalComponents.Add(sourceContamination);
            }

            Component[] childChemicalComponents = _collider.GetComponentsInChildren<Contamination>();
            if (childChemicalComponents.Length > 1)
            {
                foreach (Contamination childContamination in childChemicalComponents)
                    surfaceChemicalComponents.Add(childContamination);
            }
            // Set up looping variables
            Contamination closestContaminant = null;
            float closestDistance = float.MaxValue;
            // Get the color
            Vector3 sampledColor = new Vector3(_color.r, _color.g, _color.b);
            UnityEngine.Debug.Log($"==={gameObject.name} collecting sample from {_collider.gameObject.name} | Contaminants: {surfaceChemicalComponents.Count} | Sampled Color: {_color} || Time: {Time.time}===");
            // Loop through each contaminant and get the closest one
            for (int i = 0; i < surfaceChemicalComponents.Count; i++)
            {
                // Cache reference
                Contamination contamination = (Contamination)surfaceChemicalComponents[i];
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed initial check || Time: {Time.time}");
                // First, make sure that this is a valid collectable contaminant and that this point is actually within contaminant bounds
                if (!m_containerProperties.CanHoldType(contamination.Type)) continue;
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed bounds/container properties check || Time: {Time.time}");
                Vector3 contaminantColor = new Vector3(contamination.Agent.SpawnColor.r, contamination.Agent.SpawnColor.g, contamination.Agent.SpawnColor.b);
                Vector3 colorDifference = sampledColor - contaminantColor;

                // Compare the difference
                float colorDifferenceMagnitude = colorDifference.magnitude;
                if (colorDifferenceMagnitude < closestDistance)
                {
                    // Set this as the closest contamination
                    closestDistance = colorDifferenceMagnitude;
                    closestContaminant = contamination;
                }
            }
            UnityEngine.Debug.Log($"==={gameObject.name} found closest contaminant on {_collider.gameObject.name}: {closestContaminant} | Color Difference: {closestDistance} || Time: {Time.time}===");
            // Return the closest contaminant (if it exists)
            return closestContaminant;
        }

        public Contamination GetClosestContamination(Paintable _paintable, Color _color)
        {
            // Get contaminants from this paintable
            if (_paintable.Contaminants != null && _paintable.Contaminants.Count > 0)
            {
                return GetClosestContamination(_paintable.Contaminants, _color);
            }
            return null;
        }
        
        protected Contamination GetClosestContamination(List<Contamination> surfaceChemicalComponents, Color _color)
        {
            // Set up looping variables
            Contamination closestContaminant = null;
            float closestDistance = float.MaxValue;
            // Get the color
            Vector3 sampledColor = new Vector3(_color.r, _color.g, _color.b);

            // Loop through each contaminant and get the closest one
            for (int i = 0; i < surfaceChemicalComponents.Count; i++)
            {
                // Cache reference
                Contamination contamination = (Contamination)surfaceChemicalComponents[i];
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed initial check || Time: {Time.time}");
                // First, make sure that this is a valid collectable contaminant and that this point is actually within contaminant bounds
                if (!m_containerProperties.CanHoldType(contamination.Type)) continue;
                //UnityEngine.Debug.Log($"{gameObject.name} - GetClosestContamination[{i}]: passed bounds/container properties check || Time: {Time.time}");
                Vector3 contaminantColor = new Vector3(contamination.Agent.SpawnColor.r, contamination.Agent.SpawnColor.g, contamination.Agent.SpawnColor.b);
                Vector3 colorDifference = sampledColor - contaminantColor;

                // Compare the difference
                float colorDifferenceMagnitude = colorDifference.magnitude;
                if (colorDifferenceMagnitude < closestDistance)
                {
                    // Set this as the closest contamination
                    closestDistance = colorDifferenceMagnitude;
                    closestContaminant = contamination;
                }
            }
            // Return the closest contaminant (if it exists)
            return closestContaminant;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

