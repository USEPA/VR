using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class XRToolbelt : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] List<XRSocket> m_attachPoints;
        [SerializeField] List<XRSocket> m_otherAttachPoints;
        [SerializeField] XRSocket m_backWaistAttachPoint;
        [Header("Belt Configuration")]
        [SerializeField] float m_hipRadius = 2.0f;
        [SerializeField] float m_defaultAngle = 60.0f;
        [SerializeField] float lineWidth = 0.05f;
        [Header("Tracking Configuration")]
        [SerializeField] float m_maxHeadsetDifference = 1.0f;
        
        #endregion
        #region Private Variables
        LineRenderer m_lineRenderer;
        public List<XRToolbeltItem> m_beltItems;

        public Vector3 m_headsetOffset;
        public float m_headsetOffsetDistance;
        private Transform m_followTarget;

        private const float m_toolbeltPositionRatio = 0.70710672727f;
        #endregion
        #region Public Properties
        public List<XRToolbeltItem> Items { get => m_beltItems; }
        public List<XRSocket> AttachPoints { get => m_attachPoints; }

        public XRSocket MainDeviceAttachPoint { get => m_attachPoints[1]; }
        public XRSocket TabletAttachPoint { get => m_attachPoints[2]; }
        public List<XRSocket> OtherAttachPoints { get => m_otherAttachPoints; }

        public XRSocket BackWaistAttachPoint { get => m_backWaistAttachPoint; }
        public Transform FollowTarget { get => m_followTarget; set => m_followTarget = value; }
        #endregion

        #region Initialization

        #endregion
        // Start is called before the first frame update
        void Start()
        {
            if (TryGetComponent<LineRenderer>(out m_lineRenderer))
            {
                DrawBeltCircle();
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            // Do the thing
        }

        private void LateUpdate()
        {
            if (!VRUserManager.Instance) return;
            
            // Track to headset position
            Vector3 cameraLocalPosition = Camera.main.transform.localPosition;
            m_headsetOffset = cameraLocalPosition - transform.localPosition;
            m_headsetOffset.y = 0.0f;
            m_headsetOffsetDistance = m_headsetOffset.magnitude;
     
            /*
            // Set position
            if (m_followTarget != null)
            {
                transform.position = m_followTarget.position;
                transform.rotation = m_followTarget.rotation;
            }
            */
            //transform.localPosition = new Vector3(cameraLocalPosition.x, transform.localPosition.y, cameraLocalPosition.z);
        }

        #region Item-Related Functionality
        public XRToolbeltItem CreateToolObjectOnBelt(XRGrabInteractable _item, bool _autoSnapOnRelease = false)
        {
            if (m_beltItems == null) m_beltItems = new List<XRToolbeltItem>();
            //if (m_beltItems.Count + 1 > m_attachPoints.Count) return null;
            //GameObject attachPoint = new GameObject { name = $"Belt Attach Point #{m_beltItems.Count + 1}" };
            if (m_beltItems.Count + 1 > m_attachPoints.Count) 
            {
                UnityEngine.Debug.Log($"ERROR: No more attach points available to add {_item.gameObject.name} to belt! || Time: {Time.time}");
                return null;
            }
            Transform attachPoint = m_attachPoints[m_beltItems.Count].transform;
            XRToolbeltItem beltItem = CreateToolObject(_item, attachPoint, _autoSnapOnRelease);
            m_beltItems.Add(beltItem);
            return beltItem;
            /*attachPoint.transform.parent = transform;
            float angle = (m_beltItems.Count == 0) ? m_defaultAngle : -m_defaultAngle;
            SetItemBeltPosition(attachPoint.gameObject, angle);*/
        }

        public XRToolbeltItem CreateToolObject(XRGrabInteractable _item, Transform _target, bool _autoSnapOnRelease)
        {
            // Add the toolbelt item component
            XRToolbeltItem item = _item.gameObject.AddComponent<XRToolbeltItem>();
            // Initialize the item with the desired configuration
            item.Init(_item, _target, _autoSnapOnRelease);
            return item;
        }

        public void ClearItems()
        {
            if (m_beltItems == null || m_beltItems.Count < 1) return;
            // Loop through each item and destroy it
            for (int i = 0; i < m_beltItems.Count; i++) Destroy(m_beltItems[i].gameObject);
            // Clear all references in the belt items list
            m_beltItems.Clear();
        }
        #endregion

        public void DrawBeltCircle()
        {
            var segments = 360;
            var line = m_lineRenderer;
            line.useWorldSpace = false;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.positionCount = segments + 1;

            var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
            var points = new Vector3[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                var rad = Mathf.Deg2Rad * (i * 360f / segments);
                points[i] = new Vector3(Mathf.Sin(rad) * m_hipRadius, 0, Mathf.Cos(rad) * m_hipRadius);
            }

            line.SetPositions(points);
        }

        public void SetItemBeltPosition(GameObject obj, float angle)
        {
            Vector3 pos = Vector3.zero;
            //pos.x = transform.position.x + m_hipRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.x = m_hipRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            //pos.z = transform.position.z + m_hipRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.z = m_hipRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            obj.transform.localPosition = pos;
            Quaternion rot = Quaternion.FromToRotation(transform.forward, transform.position - transform.TransformPoint(pos));
            obj.transform.rotation = rot;
        }

        public void SetItemBeltRotation(GameObject obj)
        {
            Quaternion rot = Quaternion.LookRotation(transform.position - obj.transform.position);
            obj.transform.rotation = rot;
        }

        #if UNITY_EDITOR
        [ContextMenu("Position around Circle")]
        public void PositionAroundCircle()
        {
            // Get the transform
            Transform target = m_attachPoints[1].transform;
            // Get the current rotation
            float angle = target.localEulerAngles.y;
            SetItemBeltPosition(target.gameObject, 45.0f);
            //SetItemBeltPosition(m_attachPoints[0].gameObject, 0.0f);
            /*
            float angle = m_defaultAngle;
            for(int i = 0; i < 2; i++)
            {
                //float angle = Random.value * 360;
                SetItemBeltPosition(m_attachPoints[i].gameObject, angle);
                angle *= -1.0f;
                //angle += 270.0f;
            }
            */
            
        }

        [ContextMenu("Reset Child Transforms")]
        public void ResetChildTransforms()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.localPosition = Vector3.zero;
                child.localEulerAngles = Vector3.zero;
            }
        }

        [ContextMenu("Reset Child Positions")]
        public void ResetChildPositions()
        {
            // Set the first toolbelt item position
            m_attachPoints[0].transform.localPosition = new Vector3(0.0f, 0.0f, m_hipRadius);

            // Loop through each other attach point and set its position accordingly
            for (int i = 1; i < m_attachPoints.Count; i++)
            {
                // Get the value it should be
                float value = m_hipRadius * m_toolbeltPositionRatio;
                Vector3 adjustedPosition = m_attachPoints[i].transform.localPosition;
                adjustedPosition.x = Mathf.Sign(adjustedPosition.x) * value;
                adjustedPosition.z = Mathf.Sign(adjustedPosition.z) * value;

                m_attachPoints[i].transform.localPosition = adjustedPosition;
            }
        }

        [ContextMenu("Configure Attach Point Positions")]
        public void ConfigureAttachPointPositions()
        {
            for (int i = 1; i < m_attachPoints.Count; i++)
            {
                float sign = (i == 1) ? 1.0f : -1.0f;
                GameObject target = m_attachPoints[i].gameObject;
                SetItemBeltPosition(target, m_defaultAngle * sign);
            }
        }

        [ContextMenu("Configure Attach Point Rotations")]
        public void ConfigureAttachPointRotations()
        {
            for (int i = 1; i < m_attachPoints.Count; i++)
            {
                float sign = (i == 1) ? 1.0f : -1.0f;
                GameObject target = m_attachPoints[i].gameObject;
                SetItemBeltRotation(target);
            }
        }
#endif
    }
}

