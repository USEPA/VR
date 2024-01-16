using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class RadTestZone : MonoBehaviour
    {
        [SerializeField] List<RadTestMarker> m_radMarkers;
        [SerializeField] Transform m_targetTransform;
        [SerializeField] Gradient m_gradient;
        [SerializeField] float m_baseRadAmount = 300.0f;

        [SerializeField] Vector2 m_spreadRadiusRange = new Vector2(10.0f, 25.0f);
        [SerializeField] Vector2 m_windDirectionRange = new Vector2(0, 360.0f);
        [SerializeField] float m_spreadRadius = 10.0f;
        [SerializeField] float m_windDirection = 60.0f;
        [SerializeField] float m_coneRadiusAngleRange = 30.0f;
        [SerializeField] float m_coneRadiusFactor = 2.0f;
        [SerializeField] Transform m_directionMarker;
        [SerializeField] int m_seed = 0;
        [SerializeField] bool m_randomizeSpread = true;
        [SerializeField] bool m_randomizeDirection = true;
        [SerializeField] bool m_useSeed = false;
        protected SphereCollider m_collider;

        public Vector3 m_windDirectionVector;

        public float m_targetDistance = 0.0f;
        public float m_targetAngle = 0.0f;
        public float m_targetAngleDifference = 0.0f;
        public float m_score = 0.0f;
        public float m_radLevel;
        public Color m_outputColor;

        public float m_multiplier = 0.0f;

        public float m_coneRadius;





        public SphereCollider Collider{
            get{
                if (!m_collider) m_collider = GetComponent<SphereCollider>();
                return m_collider;
            }
        }

        public float Radius{
            get => Collider.radius;
        }
        // Start is called before the first frame update
        void Start()
        {
            if (m_useSeed)
            {
                Random.InitState(m_seed);
            }
            else
            {
                m_seed = Random.seed;
            }
            if (m_randomizeSpread) m_spreadRadius = Random.Range(m_spreadRadiusRange.x, m_spreadRadiusRange.y);
            if (m_randomizeDirection) m_windDirection = Random.Range(m_windDirectionRange.x, m_windDirectionRange.y);

            InitValues();
            ScoreMarkers();
        }

        // Update is called once per frame
        void Update()
        {
            UnityEngine.Debug.DrawRay(transform.position, m_windDirectionVector, Color.magenta);
        }



        public void CalculateScore(RadTestMarker _marker)
        {
            Transform targetTransform = _marker.transform;
    
            Vector3 sourcePosition = transform.position;
            sourcePosition.y = 0;

            Vector3 targetPosition = targetTransform.position;
            targetPosition.y = 0;
            float targetDistance = MathHelper.QuickDistance(sourcePosition, targetPosition);

            // Calculate angle
            Vector3 toolDirection = targetPosition - sourcePosition;
            //float angle = Vector3.Angle(toolDirection, m_windDirectionVector);

            Vector3 forward = m_windDirectionVector;
            float angle = Vector3.Angle(toolDirection, forward);
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

            if (angle < (m_coneRadiusAngleRange) && targetDistance < m_spreadRadius)
            {
                //float multiplier = m_coneRadius / m_targetDistance;
                //float multiplier = (1 - targetDistance / (m_spreadRadius + m_coneRadius));
                float multiplier = (1 - targetDistance / (m_spreadRadius));
                radLevel = m_baseRadAmount * multiplier;
                score = multiplier;
                //score = 1 - (m_radLevel / m_baseRadAmount);
            }

            /*
            if (dotProduct >= (Mathf.Cos(m_coneRadiusAngleRange)))
            {
                //float multiplier = m_coneRadius / m_targetDistance;
                float multiplier = (1 - targetDistance / (m_spreadRadius + m_coneRadius));
                radLevel = m_baseRadAmount * multiplier;
                score = multiplier;
                //score = 1 - (m_radLevel / m_baseRadAmount);
            }
            */
            /*
            //targetAngleDifference <= m_windDirection / 2f
            if (targetAngleDifference <= (m_windDirection / 2f)) // angle <= (m_windDirection / 2)
            {
                //float multiplier = m_coneRadius / m_targetDistance;
                float multiplier = (1 - targetDistance / (m_spreadRadius + m_coneRadius));
                radLevel = m_baseRadAmount * multiplier;
                score = multiplier;
                //score = 1 - (m_radLevel / m_baseRadAmount);
            }
            */

            _marker.Init(this, targetDistance, angle, targetAngleDifference, dotProduct, score, radLevel, m_gradient.Evaluate(score));
            //m_score = score;
            //m_score = 1 - (m_targetDistance / Radius);
            //m_radLevel = m_baseRadAmount * m_score;
            //m_outputColor = m_gradient.Evaluate(m_score);

        }

        public void InitValues()
        {
            Collider.radius = m_spreadRadius;

            float spreadAngleInRadians = m_windDirection * Mathf.Deg2Rad;
            m_coneRadius = m_spreadRadius / Mathf.Tan(spreadAngleInRadians / 2f);

            Vector3 directionMarkerScale = m_directionMarker.transform.localScale;
            Transform minDirectionMarker = m_directionMarker.transform.GetChild(0);
            Transform maxDirectionMarker = m_directionMarker.transform.GetChild(1);

            SetDirectionalMarkerValues(minDirectionMarker, m_windDirection - (m_coneRadiusAngleRange));
            SetDirectionalMarkerValues(maxDirectionMarker, m_windDirection + (m_coneRadiusAngleRange));


            m_windDirectionVector = new Vector3(Mathf.Sin(Mathf.Deg2Rad * m_windDirection), 0, Mathf.Cos(Mathf.Deg2Rad * m_windDirection));
            //m_directionMarker.transform.localScale = new Vector3(directionMarkerScale.x, directionMarkerScale.y, m_spreadRadius);
            //m_directionMarker.transform.localEulerAngles = new Vector3(0, m_windDirection, 0);
        }

        public void SetDirectionalMarkerValues(Transform _target, float _angle)
        {
            Vector3 directionMarkerScale = _target.localScale;
            _target.transform.localScale = new Vector3(directionMarkerScale.x, directionMarkerScale.y, m_spreadRadius);
            _target.transform.localEulerAngles = new Vector3(0, _angle, 0);
        }

        public void ScoreMarkers()
        {
            foreach (RadTestMarker marker in m_radMarkers)
            {
                CalculateScore(marker);
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Force Calculate Score")]
        public void ForceCalculateScore()
        {
            InitValues();
            ScoreMarkers();
        }
        #endif
    }
}

