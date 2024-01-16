using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace L58.EPAVR
{
    [RequireComponent(typeof(SphereCollider))]
    public class RadSensor : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected RadType m_sensableRadiation;
        [SerializeField] protected float m_tickInterval = 0.05f;
        [SerializeField] protected float m_maxReadingLevel = 250.0f;
        [SerializeField] protected float m_minContactDistance = 0.25f;
        [SerializeField] protected float m_minContactRatio = 0.25f;
        [SerializeField] protected float m_minConfirmLevel = 0.9f;
        [SerializeField] protected float m_levelScaleFactor = 0.9f;
        [SerializeField] protected float m_sensorRadius = 0.25f;
        [SerializeField] protected int m_minConfirmReadings = 4;
        [SerializeField] protected bool m_useObstacleDetection = true;
        [SerializeField] protected bool m_clampReadingLevel = false;
        [SerializeField] protected LayerMask m_obstacleLayers;
        [SerializeField] protected EmissiveColorSwapper m_siteStatusDisplay;
        [Header("Events")]
        [SerializeField] protected UnityEvent<float> m_onLevelUpdated;
        #endregion
        #region Protected Variables
        protected SphereCollider m_collider;
        public int m_successiveConfirmations = 0;

        protected bool m_isActive = false;
        protected float m_updateTimer = 0.0f;

        protected float m_currentLevel = 0.0f;

        protected RadCloud m_currentRadCloud;
        public float m_targetDistance;
        public float m_targetDistanceDifference;

        public float currentScore;
        public Collider m_currentObstacle;
        protected Vector3 m_currentObstacleHitPosition;
        protected Vector3 m_currentObstacleOppositeHitPosition;
        protected float m_currentObstacleHitDistance = 0.0f;
        protected float m_currentObstacleInterferingDistance = 0.0f;
        protected float m_currentDampeningFactor;
        protected float m_currentAdjustedDistance = 0.0f;
        protected float m_currentAdjustedDistanceWithDampener = 0.0f;
        protected PhysicMaterial m_currentObstacleMaterial;
        public bool m_insideAngle = false;
        protected RadLevelEvaluation m_currentEvaluation;
        protected System.Action<float> m_onUpdateLevel;
        #endregion
        #region Public Properties
        public bool IsActive
        {
            get => m_isActive;
            set
            {
                m_isActive = value;
                Collider.enabled = value;
            }
        }

        public SphereCollider Collider
        {
            get
            {
                if (!m_collider) m_collider = GetComponent<SphereCollider>();
                return m_collider;
            }
        }

        public float SensorRadius { get => m_sensorRadius; }

        public float TickInterval { get => m_tickInterval; set => m_tickInterval = value; }

        public float CurrentReading { get => m_currentLevel; }
        public float MaxReading { get => m_maxReadingLevel; }

        public RadCloud CurrentRadCloud { get => m_currentRadCloud; }
        public RadLevelEvaluation CurrentEvaluation { get => m_currentEvaluation; set => m_currentEvaluation = value; }

        public System.Action<float> OnUpdateLevel { get => m_onUpdateLevel; set => m_onUpdateLevel = value; }
        #endregion

        #region Initialization
        protected virtual void Start()
        {
            //Init();
        }
        public virtual void Init()
        {
            // Set radius
            Collider.radius = m_sensorRadius;
            // Reset all values
            m_currentLevel = 0.0f;
            m_updateTimer = 0.0f;
            ResetValues();

            if (m_onLevelUpdated.GetPersistentEventCount() > 0) m_onUpdateLevel += i => m_onLevelUpdated.Invoke(i);
            m_onUpdateLevel?.Invoke(m_currentLevel);
            //SetActive(false);
        }
        #endregion

        #region Update Logic
        public virtual void Tick()
        {
            if (m_tickInterval != 0.0f)
            {
                // Check to make sure the rad sensor is active first
                m_updateTimer += Time.deltaTime;
                // Check if it is time to update again
                if (m_updateTimer >= m_tickInterval)
                {
                    // Get the level of radiation
                    UpdateRadLevel(GetRadLevel());
                    //UpdateRadLevel(CalculateRadLevel());
                    m_updateTimer = 0.0f;
                }
            }
            else
            {
                // Get the level of radiation
                UpdateRadLevel(GetRadLevel());
            }

        }

        protected virtual void Update()
        {
            Tick();

            if (m_currentEvaluation != null)
            {
                m_insideAngle = m_currentEvaluation.InsideAngle;
            }
            m_insideAngle = false;
        }
        #endregion

        #region Collision Logic
        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<RadCloud>(out RadCloud radCloud) && m_sensableRadiation.HasFlag(radCloud.Type) && m_currentRadCloud != radCloud)
            {
                // Set cloud reference
                SetRadCloud(radCloud);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (m_currentRadCloud != null && (other.TryGetComponent<RadCloud>(out RadCloud radCloud) && m_currentRadCloud == radCloud))
            {
                // Reset references/values
                m_currentRadCloud = null;
                //UnityEngine.Debug.Log($"{gameObject.name} exited rad cloud: {radCloud.gameObject.name} || Time: {Time.time}");

                if (m_siteStatusDisplay) m_siteStatusDisplay.gameObject.SetActive(false);
                ResetValues();
            }
            /*
            if (other.transform.CompareTag("SampleArea") && m_currentSampleArea != null && other.transform.GetComponent<SampleVaporArea>() == m_currentSampleArea)
            {
                m_currentSampleArea = null;
                m_contactedAgent = null;
                ResetValues();
            }
            */
        }
        #endregion

        #region Sampling Logic
        public void SetRadCloud(RadCloud _radCloud)
        {
            // Set vapor cloud reference and get its agent
            m_currentRadCloud = _radCloud;

            //UnityEngine.Debug.Log($"{gameObject.name} entered rad cloud: {_radCloud.gameObject.name} || Time: {Time.time}");
            if (m_siteStatusDisplay && m_currentRadCloud.Parent) 
            {
                m_siteStatusDisplay.gameObject.SetActive(true);
                m_siteStatusDisplay.SetColor(m_currentRadCloud.Parent.IsCleared);
            }

        }

        public float CalculateRadLevel()
        {
            float backgroundRadiation = (BackgroundRadiation.Instance) ? BackgroundRadiation.Instance.CurrentLevel : 0.0f;
            if (m_currentRadCloud == null) return backgroundRadiation;
            // First, get the closest position to the vapor sensor relative to the bounds of the sample area
            //Vector3 sensorBoundsPosition = m_currentSampleArea.Collider.ClosestPointOnBounds(transform.position);
            // Get the distance between the sample area's epicenter and the vapor sensor's origin
            //m_targetDistance = MathHelper.QuickDistance(transform.position, m_currentSampleArea.Epicenter);
            Vector3 targetPosition = m_currentRadCloud.Epicenter; //m_currentRadCloud.transform.position
            m_targetDistance = MathHelper.QuickDistance(transform.position, targetPosition);
            float dampeningFactor = 1.0f;
            if (m_useObstacleDetection)
            {
                Ray ray = new Ray(transform.position, targetPosition - transform.position);
                //Ray ray = new Ray(targetPosition, transform.position - targetPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, m_targetDistance, m_obstacleLayers))
                {
                    // Obstacles are in the way of target epicenter
                    m_currentObstacle = hit.collider;
                    m_currentObstacleHitPosition = hit.point;
                    // Get the obstac
                    m_currentObstacleHitDistance = MathHelper.QuickDistance(transform.position, m_currentObstacleHitPosition);
                    dampeningFactor = GetRadPenetrationFactor(m_currentRadCloud, hit.collider, hit.point);
                    m_currentDampeningFactor = dampeningFactor;
                    //return 0.0f;
                }
                else
                {
                    //UnityEngine.Debug.DrawLine(transform.position, m_currentRadCloud.Epicenter, Color.cyan);
                    m_currentObstacle = null;
                    m_currentObstacleHitDistance = 0.0f;
                    m_currentDampeningFactor = dampeningFactor;

                }

            }
            m_targetDistanceDifference = m_targetDistance - m_minContactDistance;
            //float score = Mathf.Clamp(((-0.9f * Mathf.Abs(m_targetDistance - m_minContactDistance)) + 1.0f), 0.0f, 1.0f);
            //float score = m_minContactDistance / m_targetDistance;
            //float score = ((m_currentRadCloud.Radius * 2) * m_minContactRatio) / m_targetDistance;
            //float adjustedDistance = m_targetDistance - (SensorRadius * 0.5f);

            float adjustedDistance = m_targetDistanceDifference;

            m_currentAdjustedDistance = adjustedDistance;
            //float score = (m_currentRadCloud.Radius * 2) / m_targetDistance;

            float rawScore = (1 - (adjustedDistance / m_currentRadCloud.Radius));

            float score = Mathf.Clamp((rawScore * dampeningFactor) + backgroundRadiation, 0.0f, 1.0f);
            currentScore = score;
            // Score the current value based on the minimum detection distance
            return score;
            //return (m_targetDistance / m_minContactDistance);
        }

        public float GetRadLevel()
        {
            float backgroundRadiation = (BackgroundRadiation.Instance) ? BackgroundRadiation.Instance.CurrentLevel : 0.0f;
            if (m_currentRadCloud == null) return backgroundRadiation;

            // Get the base rad level from the current rad cloud
            Vector3 targetPosition = m_currentRadCloud.Epicenter; //m_currentRadCloud.transform.position
            m_targetDistance = MathHelper.QuickDistance(transform.position, targetPosition);
            //float baseRadLevel = m_currentRadCloud.CalculateRadLevel(transform.position);
            float baseRadLevel = m_currentRadCloud.CalculateRadLevel(this);

            float radLevel = baseRadLevel + backgroundRadiation;
            float dampeningFactor = 1.0f;

            if (m_useObstacleDetection)
            {
                Ray ray = new Ray(transform.position, targetPosition - transform.position);
                if (Physics.Raycast(ray, out RaycastHit hit, m_targetDistance, m_obstacleLayers))
                {
                    // Obstacles are in the way of target epicenter
                    m_currentObstacle = hit.collider;
                    m_currentObstacleHitPosition = hit.point;
                    // Get the obstac
                    m_currentObstacleHitDistance = MathHelper.QuickDistance(transform.position, m_currentObstacleHitPosition);
                    dampeningFactor = GetRadPenetrationFactor(m_currentRadCloud, hit.collider, hit.point);
                    m_currentDampeningFactor = dampeningFactor;
                    //return 0.0f;
                }
                else
                {
                    //UnityEngine.Debug.DrawLine(transform.position, m_currentRadCloud.Epicenter, Color.cyan);
                    m_currentObstacle = null;
                    m_currentObstacleHitDistance = 0.0f;
                    m_currentDampeningFactor = dampeningFactor;

                }

            }

            // Apply dampening factor
            radLevel *= dampeningFactor;
            if (m_currentEvaluation != null)
            {
                m_currentEvaluation.DampeningFactor = dampeningFactor;
                m_currentEvaluation.RadLevel = radLevel;
            }
            if (m_clampReadingLevel) radLevel = Mathf.Clamp(radLevel, 0.0f, m_maxReadingLevel);

            /*
            // Calculate score
            float rawScore = 1 - (radLevel / m_maxReadingLevel);
            float score = Mathf.Clamp(rawScore, 0.0f, 1.0f);
            currentScore = score;

            return score;
            */

            return radLevel;
        }

        protected virtual void UpdateRadLevel(float _value)
        {
            m_currentLevel = _value;
            // Calculate score
            float rawScore = (m_currentLevel / m_maxReadingLevel);
            float score = Mathf.Clamp(rawScore, 0.0f, 1.0f);
            currentScore = score;

            m_onUpdateLevel?.Invoke(currentScore);
            /*
            if (m_currentRadCloud && !m_currentRadCloud.Parent.IsCleared)
            {
                if (m_currentLevel >= m_minConfirmLevel)
                {
                    // Iterate confirm level
                    m_successiveConfirmations++;
                    if (m_successiveConfirmations >= m_minConfirmReadings)
                    {
                        // Confirm the radiation
                        m_currentRadCloud.Parent.ClearSite();
                        m_siteStatusDisplay.SetColor(m_currentRadCloud.Parent.IsCleared);
                        m_successiveConfirmations = 0;
                    }
                }
                else
                {
                    // Reset confirmations
                    m_successiveConfirmations = 0;
                }
            }
            */
            //m_onLevelUpdated?.Invoke();
        }


        #endregion

        #region Helper Methods
        public void SetActive(bool _value)
        {
            IsActive = _value;
            //m_siteStatusDisplay.gameObject.SetActive((_value && m_currentRadCloud != null) ? true : false);
        }

        float GetRadPenetrationFactor(RadCloud _sourceCloud, Collider _collider, Vector3 _hitPosition)
        {
            if (_collider.sharedMaterial != null)
            {
                // Get the distance from the epicenter to the collider in the direction of the hit point
                float originDistance = MathHelper.QuickDistance(_sourceCloud.Epicenter, _hitPosition);
                Ray oppositeRay = new Ray(_sourceCloud.Epicenter, (_hitPosition - _sourceCloud.Epicenter));
                if (_collider.Raycast(oppositeRay, out RaycastHit hit, originDistance))
                {
                    // Get the hit position
                    Vector3 oppositeHitPoint = hit.point;
                    m_currentObstacleOppositeHitPosition = oppositeHitPoint;
                    // Get the distance betweeen the original hit point and this one
                    float interferingDistance = MathHelper.QuickDistance(_hitPosition, oppositeHitPoint);
                    m_currentObstacleInterferingDistance = interferingDistance;
                    m_currentObstacleMaterial = _collider.sharedMaterial;
                    return (GetDampenerFromPhysicsMaterial(_collider.sharedMaterial));
                }
            }
            return 0.0f;
        }

        public float GetDampenerFromPhysicsMaterial(PhysicMaterial _material)
        {
            switch (_material.name)
            {
      
                case "Stone":
                    return 0.25f;
                case "Metal":
                    return 0.5f;
                case "Wood":
                    return 0.75f;
                case "Flesh":
                    return 0.8f;
                case "Glass":
                    return 0.9f;
                default:
                    return 0.0f;
            }
        }
        #endregion

        #region Reset Logic
        public void ResetValues()
        {
            m_currentLevel = 0.0f;
            m_currentObstacle = null;
            m_currentObstacleHitDistance = 0.0f;
            m_successiveConfirmations = 0;
        }
        #endregion

        private void OnDestroy()
        {
            m_onUpdateLevel = null;
        }
    }
}

