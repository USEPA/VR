using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class VaporGraphDisplay : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] List<GraphItemElement> m_graphItems;
        [SerializeField] float m_tickInterval = 0.5f;
        [SerializeField] bool m_active = false;
        #endregion
        #region Private Variables
        float m_updateTimer = 0.0f;
        float m_currentValue = 0.0f;
        System.Action<float> m_onUpdateValue;
        #endregion
        #region Public Properties
        public List<GraphItemElement> Items { get => m_graphItems; }
        #endregion

        #region Initialization
        public void Awake()
        {
            m_onUpdateValue += i => UpdateGraph(i);
        }
        public void Init()
        {
            // Default all values
            ResetValues();
        }
        #endregion

        #region Graph-Related Methods
        public void ResetValues()
        {
            // Set all values to default
            for (int i = 0; i < m_graphItems.Count; i++) 
            {
                m_graphItems[i].SetValue(0.0f);
                m_graphItems[i].Index = i;
            }
        }

        public void UpdateGraph(float _newestValue)
        {
            int lastIndex = m_graphItems[0].Index;
            // Shift all current values in the graph
            for (int i = 0; i < m_graphItems.Count-1; i++)
            {
                m_graphItems[i].SetValue(m_graphItems[i + 1].Value, m_graphItems[i+1].Index);
            }
            // Update the last element with the newest value
            m_graphItems[m_graphItems.Count - 1].SetValue(_newestValue, lastIndex);
            //m_graphItems[m_graphItems.Count - 1].SetValue(0.0f, lastIndex);

        }
        #endregion


        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (!m_active) return;

            m_updateTimer += Time.deltaTime;
            if (m_updateTimer >= m_tickInterval)
            {
                UpdateGraph(Random.Range(0.0f, 1.0f));
                m_updateTimer = 0.0f;
            }
            */
        }

        public void UpdateCurrentValue(float value)
        {
            m_currentValue = value;
            m_onUpdateValue.Invoke(m_currentValue);
        }
    }
}

