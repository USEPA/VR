using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class SampleSurfaceArea : SampleArea
    {
        #region Inspector Assigned Variables
        [SerializeField] LineRenderer m_surfaceOutline;
        #endregion
        #region Protected Variables
        #endregion
        #region Public Properties
        public override string TypeID => "SurfaceArea";
        #endregion

        private void Start()
        {
            //Init();
        }
        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Initialize the sample grid
            InitSampleGrid();
        }

        public void InitSampleGrid()
        {
            // Get parent bounds
            transform.parent.TryGetComponent<MeshRenderer>(out m_parentMesh);
            // Init grid
            m_size = (int)Mathf.Sqrt(m_totalSamples);
            m_itemSize = new Vector3(Bounds.size.x / m_size, Bounds.size.y / m_size, Bounds.size.z);
            m_origin = new Vector3((Bounds.min.x + Bounds.size.x) - (m_itemSize.x / 2), Bounds.min.y + (m_itemSize.y / 2), Bounds.center.z + 0.00001f);
            m_sampleGrid = new SampleUnit[m_size, m_size];
            // Create the contaminant area
            sampleCenter = new Vector2Int(Random.Range(0, m_size - 1), Random.Range(0, m_size - 1));
            if (m_collider)
            {
                Vector3 randomPoint = new Vector3(Random.Range(Bounds.min.x, Bounds.min.x + Bounds.size.x), Random.Range(Bounds.min.y, Bounds.min.y + Bounds.size.y), Random.Range(Bounds.min.z, Bounds.min.z + Bounds.size.z));
                m_epicenter = m_collider.ClosestPointOnBounds(randomPoint);
                /*
                GameObject epicenterMarker = new GameObject();
                epicenterMarker.transform.position = m_epicenter;
                epicenterMarker.transform.rotation = m_collider.transform.rotation;
                epicenterMarker.name = $"{gameObject.name}_Epicenter";
                */
            }

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    Vector2 uvPosition = new Vector2((float)x / (float)(m_size - 1), (float)y / (float)(m_size - 1));
                    Vector3 worldPosition = new Vector3(m_origin.x - (x * (m_itemSize.x)), m_origin.y + (y * (m_itemSize.y)), m_origin.z);
                    Vector3 position = transform.InverseTransformPoint(worldPosition);
                    m_sampleGrid[x, y] = new SampleUnit(index, uvPosition, position);
                    // Check if it is within the sample bounds
                    bool isSample = UnitInSampleBounds(index);
                    float distance = MathHelper.QuickDistance((Vector2)index, (Vector2)sampleCenter);
                    m_sampleGrid[x, y].SetIsSample(isSample, distance);
                }
            }
            // Set up the outline
            SetupOutline();
        }
        #endregion

        #region Show/Hide Area-Related Functionality
        public override void ShowArea()
        {
            //UnityEngine.Debug.Log($"Bound Size: {Bounds.size} | Center: {Bounds.center} | Min: {Bounds.min} | Max: {Bounds.max} | Individual Item Size: {itemSize} || Time: {Time.time}");
            UnityEngine.Debug.Log($"Arrived in ShowSampleGrid || Time: {Time.time}");
            // Loop through grid and create objects
            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    SampleUnit unit = m_sampleGrid[x, y];
                    unit.Object = CreateSampleUnitObject(unit);
                }
            }
            // Call base functionality
            base.ShowArea();
        }

        public override void HideArea()
        {
            // Loop through grid and delete objects
            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    SampleUnit unit = m_sampleGrid[x, y];
                    unit.SetCleared(false);
                    Destroy(unit.Object.gameObject);
                    unit.Object = null;
                }
            }
            // Call base functionality
            base.HideArea();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets the points of a LineRenderer around the bounds of the surface
        /// </summary>
        public virtual void SetupOutline()
        {
            // Make sure there is a valid LineRenderer component
            if (!m_surfaceOutline) return;
            // Initialize positions
            m_surfaceOutline.positionCount = 5;
            Vector3[] points = new Vector3[5];
            float halfZLength = (Mathf.Abs(Bounds.min.z - Bounds.center.z) * 8);
            float zValue = Bounds.min.z;
            // Set the points of the outline based on max/min values of bounds
            points[0] = new Vector3(Bounds.min.x, Bounds.min.y, zValue);
            points[1] = new Vector3(Bounds.max.x, Bounds.min.y, zValue);
            points[2] = new Vector3(Bounds.max.x, Bounds.max.y, zValue);
            points[3] = new Vector3(Bounds.min.x, Bounds.max.y, zValue);
            points[4] = new Vector3(Bounds.min.x, Bounds.min.y, zValue);
            // Loop through each point and put it into local space
            for (int i = 0; i < 5; i++)
            {
                Vector3 localPoint = transform.InverseTransformPoint(points[i]);
                //float sign = ((localPoint + transform.forward.normalized * 1.01f).z > transform.localPosition.z) ? 1.0f : -1.0f;
                points[i] = localPoint + (transform.forward.normalized*0.01f);
                //UnityEngine.Debug.Log($"Point[{i}]: {points[i]} | Original Transformed Point: {localPoint} || Time: {Time.time}");
            }
            // Send the transformed positions to the LineRenderer
            m_surfaceOutline.SetPositions(points);
        }
        #endregion
    }
}

