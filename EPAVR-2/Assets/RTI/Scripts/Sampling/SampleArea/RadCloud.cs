using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    [RequireComponent(typeof(SphereCollider))]
    public class RadCloud : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Default Configuration")]
        [SerializeField] protected RadType m_type = RadType.Gamma;
        [SerializeField] protected Vector2 m_radiusRange = new Vector2(1.0f, 2.5f);
        [SerializeField] protected Vector2 m_windDirectionRange = new Vector2(0, 360.0f);
        [SerializeField] protected float m_coneRadiusAngleRange = 30.0f;
        [SerializeField] protected float m_baseRadAmount = 300.0f;

        [SerializeField] protected ParticleSystem m_smokePrefab;

        [SerializeField] protected float m_radius = 2.0f;
        [SerializeField] protected float m_windDirection = 60.0f;
        [SerializeField] protected bool m_useRandomRadius = true;
        [SerializeField] protected bool m_randomizeDirection = true;
        [SerializeField] protected bool m_spawnSmokeEffect = true;
        [SerializeField] protected bool m_useAngleCalculation = true;
        #endregion
        #region Protected Variables
        protected RadSite m_parent;
        protected Vector3 m_epicenter;
        protected SphereCollider m_collider;
        protected RadSiteMapMarker m_mapMarker;

   
        public Vector3 m_windDirectionVector;

        public float m_targetDistance = 0.0f;
        public float m_targetAngle = 0.0f;
        public float m_targetAngleDifference = 0.0f;
        public float m_coneRadius;


        public float m_radLevel;
        public float m_distanceMultiplier;
        public float m_angleDifferenceMultiplier;
        public float m_multiplier = 0.0f;
        #endregion
        #region Public Properties
        public Vector3 Epicenter { get => transform.position; }

        public RadType Type { get => m_type; }

        public RadSite Parent { get => m_parent; }
        public float Radius 
        { 
            get => m_radius; 
            private set
            {
                m_radius = value;
                Collider.radius = m_radius;
            }
        }

        public float ConeRadiusAngleRange { get => m_coneRadiusAngleRange; }

        public SphereCollider Collider
        {
            get
            {
                if (!m_collider) m_collider = GetComponent<SphereCollider>();
                return m_collider;
            }
        }

        public float BaseRadAmount { get => m_baseRadAmount; }
        public float WindDirection { get => m_windDirection; }

        public RadSiteMapMarker MapMarker
        {
            get => m_mapMarker;
            set => m_mapMarker = value;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Set radius
            //if (m_useRandomRadius) Radius = Random.Range(m_radiusRange.x, m_radiusRange.y);
            //Init(RadType.Gamma, m_radiusRange);

            if (!ScenarioManager.Instance) Init(null, RadType.Gamma, m_radiusRange, m_baseRadAmount);
        }

        #region Initialization
        public void Init(RadSite _parent, RadType _type)
        {
            // Cache parent reference
            if (_parent != null) m_parent = _parent;
            // Set rad type
            m_type = _type;

            // Set random direction
            if (m_randomizeDirection) m_windDirection = Random.Range(m_windDirectionRange.x, m_windDirectionRange.y);

            float spreadAngleInRadians = m_windDirection * Mathf.Deg2Rad;
            m_coneRadius = m_radius / Mathf.Tan(spreadAngleInRadians / 2f);

            m_windDirectionVector = new Vector3(Mathf.Sin(Mathf.Deg2Rad * m_windDirection), 0, Mathf.Cos(Mathf.Deg2Rad * m_windDirection));

            if (m_smokePrefab && m_spawnSmokeEffect) 
            {
                Ray downRay = new Ray(transform.position, Vector3.down);
                Vector3 effectPosition = transform.position;
                if (Physics.Raycast(downRay, out RaycastHit hit, m_radius, LayerMask.GetMask("Default", "Teleport")))
                {
                    effectPosition = hit.point;
                }
                ParticleSystem smokeEffect = Instantiate(m_smokePrefab.gameObject, transform).GetComponent<ParticleSystem>();
                smokeEffect.transform.position = effectPosition;
                Vector3 eulerAngles = smokeEffect.transform.localEulerAngles;
                eulerAngles.x = -30.0f;
                eulerAngles.y = m_windDirection;
                smokeEffect.transform.localEulerAngles = eulerAngles;
            }
            UnityEngine.Debug.Log($"{gameObject.name} was initialized || Time: {Time.time}");
        }

        public void Init(RadSite _parent, RadType _type, Vector2 _spreadSize, float _radLevel, bool _enableSmokeEffect = true)
        {
            // Determine whether the smoke effect should be enabled
            m_spawnSmokeEffect = _enableSmokeEffect;
            // Call base initialization
            Init(_parent, _type);
            // Set radiation level
            m_baseRadAmount = _radLevel;
            // Set random radius
            SetRadius(_spreadSize);
        }
        #endregion

        private void Update()
        {
            UnityEngine.Debug.DrawRay(transform.position, m_windDirectionVector, Color.magenta);
        }
        #region Rad Level-Related Functionality
        public float CalculateRadLevel(Vector3 _position)
        {
            m_multiplier = 0.0f;
            m_distanceMultiplier = 0.0f;
            m_angleDifferenceMultiplier = 0.0f;
            Vector3 sourcePosition = transform.position;
            sourcePosition.y = 0;

            Vector3 targetPosition = _position;
            targetPosition.y = 0;
            float targetDistance = MathHelper.QuickDistance(sourcePosition, targetPosition);
            m_targetDistance = targetDistance;
            m_radLevel = 0.0f;
            // Calculate angle
            Vector3 toolDirection = targetPosition - sourcePosition;
            //float angle = Vector3.Angle(toolDirection, m_windDirectionVector);

            Vector3 forward = m_windDirectionVector;
            float angle = Vector3.Angle(toolDirection, forward);
            m_targetAngle = angle;
            /*
            float angle = Vector3.Angle(toolDirection, transform.forward);
            float targetAngleDifference = Mathf.Abs(Mathf.DeltaAngle(angle, m_windDirection));
            */
            float targetAngleDifference = m_windDirection - angle;
            //m_radLevel = 0.0f;
            float score = 0.0f;
            float radLevel = 0.0f;

            //float coneRadius = Mathf.Tan(Mathf.Deg2Rad * (m_windDirection / 2.0f)) * targetDistance;
            //float coneRadius = Mathf.Tan(Mathf.Deg2Rad * targetAngleDifference) * targetDistance;

            float dotProduct = Vector3.Dot(toolDirection.normalized, m_windDirectionVector.normalized);
            m_targetAngleDifference = 0.0f;

            if (targetDistance < m_radius)
            {
                // Get the base multiplier factor based on how far it is
                float distanceFactor = (1 - targetDistance / (m_radius));
                m_distanceMultiplier = distanceFactor;
                float multiplier = distanceFactor;

                if (m_useAngleCalculation && !(angle < (m_coneRadiusAngleRange)))
                {
               
                    // Get the angle difference from the cone radius angle
                    float angleDifference = Mathf.Abs(m_coneRadiusAngleRange - angle);
                    m_targetAngleDifference = angleDifference;
                    if (angleDifference < (m_coneRadiusAngleRange * 0.5f))
                    {
                        // Scale multiplier based on how far the angle is from the target
                        float angleDiffFactor = ((angleDifference / (m_coneRadiusAngleRange * 0.5f)));
                        //float angleDiffFactor = (1-(angleDifference / (m_coneRadiusAngleRange * 0.25f)));
                        m_angleDifferenceMultiplier = angleDiffFactor;
                        multiplier = Mathf.Clamp((multiplier - angleDiffFactor), 0.0f, 1.0f);
                       // multiplier *= angleDiffFactor;
                    }
                    else
                    {
                        // Zero out the multiplier
                        multiplier = 0.0f;
                    }
              

                    //float multiplier = m_coneRadius / m_targetDistance;
                    //float multiplier = (1 - targetDistance / (m_spreadRadius + m_coneRadius));


                }
                /*
                if (angle < (m_coneRadiusAngleRange))
                {
                    radLevel = m_baseRadAmount * multiplier;
                    score = multiplier;
                    return radLevel;
                }
                */

                //float multiplier = (1 - targetDistance / (m_radius));
                radLevel = m_baseRadAmount * multiplier;
                m_radLevel = radLevel;
                score = multiplier;
                m_multiplier = score;
                //score = 1 - (m_radLevel / m_baseRadAmount);
                return radLevel;

            }



            return 0.0f;
        }

        public float CalculateRadLevel(RadSensor _sensor)
        {
            m_multiplier = 0.0f;
            m_distanceMultiplier = 0.0f;
            m_angleDifferenceMultiplier = 0.0f;
            Vector3 sourcePosition = transform.position;
            sourcePosition.y = 0;


            Vector3 targetPosition = _sensor.transform.position;
            targetPosition.y = 0;
            float targetDistance = MathHelper.QuickDistance(sourcePosition, targetPosition);
            m_targetDistance = targetDistance;
            m_radLevel = 0.0f;
            // Calculate angle
            Vector3 toolDirection = targetPosition - sourcePosition;
            //float angle = Vector3.Angle(toolDirection, m_windDirectionVector);

            Vector3 forward = m_windDirectionVector;
            float angle = Vector3.Angle(toolDirection, forward);
            m_targetAngle = angle;
            /*
            float angle = Vector3.Angle(toolDirection, transform.forward);
            float targetAngleDifference = Mathf.Abs(Mathf.DeltaAngle(angle, m_windDirection));
            */
            float targetAngleDifference = m_windDirection - angle;
            //m_radLevel = 0.0f;
            float score = 0.0f;
            float radLevel = 0.0f;
            float angleDifference = 0.0f;
            float angleDiffFactor = 1.0f;

            //float coneRadius = Mathf.Tan(Mathf.Deg2Rad * (m_windDirection / 2.0f)) * targetDistance;
            //float coneRadius = Mathf.Tan(Mathf.Deg2Rad * targetAngleDifference) * targetDistance;

            float dotProduct = Vector3.Dot(toolDirection.normalized, m_windDirectionVector.normalized);
            m_targetAngleDifference = 0.0f;
            bool insideAngle = false;
            if (targetDistance < m_radius)
            {
                // Get the base multiplier factor based on how far it is
                float distanceFactor = (1 - targetDistance / (m_radius));
                m_distanceMultiplier = distanceFactor;
                float multiplier = distanceFactor;
                //float closeDistanceScaler = (targetDistance < (m_radius * 0.1f)) ? 0.5f : 1.0f;


                if (m_useAngleCalculation && !(angle < (m_coneRadiusAngleRange)))
                {

                    // Get the angle difference from the cone radius angle
                    angleDifference = Mathf.Abs(m_coneRadiusAngleRange - angle);
                    m_targetAngleDifference = angleDifference;
                    if (angleDifference < (m_coneRadiusAngleRange * 0.5f))
                    {
                        // Scale multiplier based on how far the angle is from the target
                        angleDiffFactor = ((angleDifference / (m_coneRadiusAngleRange * 0.5f)));
                        //float angleDiffFactor = (1-(angleDifference / (m_coneRadiusAngleRange * 0.25f)));
                        m_angleDifferenceMultiplier = angleDiffFactor;
                        multiplier = Mathf.Clamp((multiplier - (angleDiffFactor)), 0.0f, 1.0f);
                        insideAngle = true;
                        // multiplier *= angleDiffFactor;
                    }
                    else
                    {
                        // Zero out the multiplier
                        multiplier = 0.0f;
                    }


                    //float multiplier = m_coneRadius / m_targetDistance;
                    //float multiplier = (1 - targetDistance / (m_spreadRadius + m_coneRadius));


                }

                /*
                if (angle < (m_coneRadiusAngleRange))
                {
                    radLevel = m_baseRadAmount * multiplier;
                    score = multiplier;
                    return radLevel;
                }
                */

                //float multiplier = (1 - targetDistance / (m_radius));
                radLevel = m_baseRadAmount * multiplier;
                m_radLevel = radLevel;
                score = multiplier;
                m_multiplier = score;
                //score = 1 - (m_radLevel / m_baseRadAmount);
                _sensor.CurrentEvaluation = new RadLevelEvaluation(m_coneRadiusAngleRange, targetDistance, angle, angleDifference, angleDiffFactor, multiplier, radLevel, insideAngle);
                return radLevel;

            }


            _sensor.CurrentEvaluation = new RadLevelEvaluation(0.0f);
            return 0.0f;
        }
        #endregion

        #region Helper Methods
        public void SetRadius(float _radius)
        {
            Radius = _radius;
        }

        public void SetRadius(Vector2 _radiusRange)
        {
            SetRadius(Random.Range(_radiusRange.x, _radiusRange.y));
        }

        public void SetMapMarkerStatus(bool _active = false)
        {
            m_mapMarker.gameObject.SetActive(_active);
        }
        #endregion
    }

    public class RadLevelEvaluation
    {
        public float ConeRadiusAngleRange;

        public float Distance;
        public float Angle;

        public float AngleDifference;
        public float AngleDifferenceFactor;

        public float Multiplier;
        public float RadLevel;

        public float DampeningFactor;
        public bool InsideAngle = false;
        
        public RadLevelEvaluation(float _coneAngleRange, float _distance, float _angle, float _angleDifference, float _angleDifferenceFactor, float _multiplier, float _radLevel, bool _insideAngle = false)
        {
            ConeRadiusAngleRange = _coneAngleRange;
            Distance = _distance;
            Angle = _angle;

            AngleDifference = _angleDifference;
            AngleDifferenceFactor = _angleDifferenceFactor;

            Multiplier = _multiplier;
            RadLevel = _radLevel;
            InsideAngle = _insideAngle;
        }

        public RadLevelEvaluation(float _distance)
        {
            ConeRadiusAngleRange = 0.0f;
            Distance = _distance;
            Angle = 0.0f;

            AngleDifference = 0.0f;
            AngleDifferenceFactor = 0.0f;

            Multiplier = 0.0f;
            RadLevel = 0.0f;
            InsideAngle = false;
        }
    }
}

