using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class SampleSwabDeliveryBox : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] TriggerEventListener m_openTriggerListener;
        [SerializeField] Collider m_trigger;
        [SerializeField] List<Transform> m_swabAttachPointPresets;
        [SerializeField] RectTransform m_sendButtonContainer;
        #endregion
        #region Private Variables
        private MXTraceSampler m_parent;

        private Animator m_anim;
        private List<Transform> m_swabAttachPoints;
        private List<TraceSampleSwab> m_insertedSwabs;
        #endregion
        #region Public Properties
        public Collider Trigger { get => m_trigger; }
        public Collider OpenTrigger { get => m_openTriggerListener.Collider; }

        public List<TraceSampleSwab> InsertedSwabs { get => m_insertedSwabs; }
        #endregion

        #region Initialization
        public void Init(MXTraceSampler _parent)
        {
            // Cache components
            m_parent = _parent;
            if (!m_anim) m_anim = GetComponent<Animator>();
            // Initialize the open trigger listener
            m_openTriggerListener.Init();
            // Hook up open/close events
            m_openTriggerListener.OnTriggerEntered += i => ProcessSwabEnter(i);
            m_openTriggerListener.OnTriggerExited += i => ProcessSwabExit(i);
            // Initialize swab attach points
            m_swabAttachPoints = new List<Transform>();
            foreach (Transform attachPoint in m_swabAttachPointPresets) m_swabAttachPoints.Add(attachPoint);
            //m_anim.SetBool("IsOpen", true);
            m_insertedSwabs = new List<TraceSampleSwab>();
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

        public void Open()
        {
            m_anim.SetBool("IsOpen", true);
        }

        public void Close()
        {
            m_anim.SetBool("IsOpen", false);
        }

        public void ProcessSwabEnter(Collider collider)
        {
            // Make sure this collider is a swab
            if (collider.TryGetComponent<TraceSampleSwab>(out TraceSampleSwab swab) && swab.Analyzed && !m_insertedSwabs.Contains(swab)) // && swab.Analyzed
            {
                Open();
            }
        }

        public void ProcessSwabExit(Collider collider)
        {
            // Make sure this collider is a swab
            if (collider.TryGetComponent<TraceSampleSwab>(out TraceSampleSwab swab) && swab.Analyzed && !m_insertedSwabs.Contains(swab))
            {
                Close();
            }
        }

        #region Swab Delivery-Related Functionality
        public void DeliverSwab(TraceSampleSwab swab)
        {
            // Get a random attach point
            //Transform attachPoint = m_swabAttachPoints[Random.Range(0, m_swabAttachPoints.Count)];
            Transform attachPoint = m_swabAttachPoints[0];
            if (m_swabAttachPoints.Count > 1)
            {
                float closestDistance = float.MaxValue;
                for (int i = 0; i < m_swabAttachPoints.Count; i++)
                {
                    float distance = MathHelper.QuickDistance(swab.transform.position, m_swabAttachPoints[i].position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        attachPoint = m_swabAttachPoints[i];
                    }
                }
            }
            // Parent the swab to a random attach point
            swab.transform.parent = attachPoint;
            swab.transform.localPosition = Vector3.zero;
            swab.transform.localEulerAngles = Vector3.zero;
            // Remove its collider/rigidbody
            Destroy(swab.GetComponent<Rigidbody>());
            Destroy(swab.GetComponent<Collider>());

            // Remove this attach point from the list of possibilities
            m_swabAttachPoints.Remove(attachPoint);
            if (m_insertedSwabs.Count < 1)
            {
                m_sendButtonContainer.gameObject.SetActive(true);
            }
            m_insertedSwabs.Add(swab);
            Close();
        }

        public void SendSwabsToLab()
        {
            if (m_insertedSwabs.Count < 0) return;
            UnityEngine.Debug.Log($"Sending swabs to lab || Time: {Time.time}");
            m_parent.DestroyDeliveryBox();
        }
        #endregion


    }
}

