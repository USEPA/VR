using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class SampleUnit
    {
        #region Private Variables
        private Vector2Int index;
        private Vector2 uvPosition;
        private Vector3 position;
        private float m_sampleAmount;
        private SampleSurfaceArea m_sourceArea;
        private Contamination m_source;

        private bool isSample;
        private float sampleDistance;

        private bool cleared;
        #endregion
        #region Public Properties
        public Vector2Int Index { get => index; }
        public Vector2 UVPosition { get => uvPosition; }
        public Vector3 Position { get => position; }
        public float Amount { get => m_sampleAmount; }
        public SampleUnitObject Object {get; set;}

        public bool IsSample { get => isSample; }
        public float SampleDistance { get => sampleDistance; }

        public SampleSurfaceArea SourceArea { get => m_sourceArea; set => m_sourceArea = value; }
        public Contamination Source { get => m_source; set => m_source = value; }

        public bool Cleared { get => cleared; }
        #endregion

        public SampleUnit(Vector2Int _index, Vector2 _uvPosition, Vector3 _position, float _amount = 0.0f)
        {
            index = _index;
            uvPosition = _uvPosition;
            position = _position;
            m_sampleAmount = _amount;
        }

        public void SetIsSample(bool _isSample, float _sampleDistance)
        {
            isSample = _isSample;
            sampleDistance = _sampleDistance;
        }

        public void SetCleared(bool _cleared)
        {
            cleared = _cleared;
        }
    }

    public class SampleUnitObject : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] Color m_sampleColor = Color.green;
        public Contamination source;
        #endregion
        #region Private Variables
        private SampleUnit unit;
        private MeshRenderer mesh;
        private Collider col;
        #endregion
        #region Public Properties
        public SampleUnit Unit { get => unit; }
        public Vector2Int Index { get => unit.Index; }
        public Vector2 UVPosition { get => unit.UVPosition; }
        public float Amount { get => unit.Amount; }

        public SampleSurfaceArea SourceArea { get => unit.SourceArea; }
        public Contamination Source { get => unit.Source; }

        public bool Cleared { get => unit.Cleared; }
        #endregion

        #region Initialization
        public void Init(SampleUnit _unit)
        {
            // Cache sample unit reference
            unit = _unit;
            index = unit.Index;
            position = unit.UVPosition;

            isSample = unit.IsSample;
            sampleDistance = unit.SampleDistance;
            mesh = GetComponent<MeshRenderer>();
            col = GetComponent<Collider>();
            //if (unit.IsSample) SetColor();
            if (unit.Cleared) SetColor();

            if (Source != null) source = Source;
        }
        #endregion

        public Vector2 position;
        public Vector2Int index;
        public bool isSample;
        public float sampleDistance;
        
        public void SetColor()
        {
            mesh.enabled = true;
            col.enabled = false;
            mesh.material.color = m_sampleColor;
        }
    }
}

