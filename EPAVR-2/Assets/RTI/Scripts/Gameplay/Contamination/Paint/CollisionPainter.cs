using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class CollisionPainter : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] Color m_spillColor;
        [SerializeField] float m_spillStrength = 1.0f;
        [SerializeField] float m_spillHardness = 0.15f;
        [Header("Debug Values")]
        public float m_radius;
        public List<Paintable> m_affectedObjects;
        public bool foundObject = false;
        #endregion
        #region Protected Variables
        protected Collider m_collider;
        protected bool m_initialized;
        #endregion
        #region Public Properties
        public Collider Collider
        {
            get
            {
                if (!m_collider) m_collider = GetComponent<Collider>();
                return m_collider;
            }
        }
        #endregion

        #region Initialization
        public void Init(float _radius, Color _spillColor, float _strength, float _hardness)
        {
            // Cache values
            m_radius = _radius;
            m_spillColor = _spillColor;
            m_spillStrength = _strength;
            m_spillHardness = _hardness;
            // Scale the object according to radius
            float scale = m_radius * 2;
            transform.localScale = new Vector3(scale, scale, scale);

            m_affectedObjects = new List<Paintable>();

            StartCoroutine(PaintProcess());
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

        public IEnumerator PaintProcess()
        {
            // Enable the collider
            Collider.enabled = true;

            yield return new WaitForFixedUpdate();
            UnityEngine.Debug.Log($"{gameObject.name} - Paint Process: {m_affectedObjects.Count} || Time: {Time.time}");
            // Disable the collider
            Collider.enabled = false;
        }
        #region Collision-Related Functionality
        private void OnCollisionStay(Collision collision)
        {
            if (!PaintManager.Instance || foundObject) return;
            UnityEngine.Debug.Log($"{gameObject.name} OnCollisionStay | Contacts: {collision.contactCount} || Time: {Time.time}");
            // Get all contacts
            foreach(ContactPoint contact in collision.contacts)
            {

                Collider col = contact.otherCollider;
                if (col.TryGetComponent<Paintable>(out Paintable paintable))
                {
                    if (m_affectedObjects.Contains(paintable)) continue;
                    // Get contact point
                    Vector3 contactPoint = contact.point;
                    // Get the distance from origin
                    float originDistance = MathHelper.QuickDistance(transform.position, contactPoint);
                    float scale = (1 - (originDistance / m_radius));
                    // Get the adjusted radius
                    float adjustedRadius = scale * m_radius;

                    Paint(paintable, contactPoint, m_spillColor, adjustedRadius, m_spillStrength, m_spillHardness);
                    foundObject = true;
                    m_affectedObjects.Add(paintable);
                }
            }
        }
        #endregion

        public void Paint(Paintable p, Vector3 _pos, Color _color, float _radius, float _strength, float _hardness)
        {
            UnityEngine.Debug.Log($"{gameObject.name} painting {p.gameObject.name} | Point: {_pos} | Radius: {_radius} | Strength: {_strength} | Color: {_color} || Time: {Time.time}");
            PaintManager.Instance.Paint(p, _pos, _radius, _hardness, _strength, _color);
        }
    }
}

