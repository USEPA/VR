using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class GraphValueCreator : MonoBehaviour
    {
        [SerializeField] RectTransform m_parent;
        [SerializeField] List<GraphItemElement> m_values;
        [SerializeField] int valueCount = 50;
        [SerializeField] GameObject m_hazardIconPrefab;
        [SerializeField] Color m_normalColor;
        [SerializeField] Color m_hazardColor;
        public float itemWidth = 0.0f;
        public float itemWidthValue = 0.0f;
        public Vector3 defaultScale;
        public Vector3 defaultRotation;
        public Vector3 defaultOffsetMin;
        public Vector3 defaultOffsetMax;
        public float defaultZPos;

        #if UNITY_EDITOR
        [ContextMenu("Create Graph Values")]
        public void CreateGraphValues()
        {
            // Calculate the width of each item
            itemWidth = m_parent.rect.width / valueCount;
            itemWidthValue = itemWidth / m_parent.rect.width;
            // Set up start values
            Vector2 itemXAnchorBounds = new Vector2(0.0f, itemWidthValue);
            defaultScale = m_values[0].RectTransform.localScale;
            defaultRotation = m_values[0].RectTransform.localEulerAngles;
            defaultOffsetMin = m_values[0].RectTransform.offsetMin;
            defaultOffsetMax = m_values[0].RectTransform.offsetMax;
            defaultZPos = m_values[0].RectTransform.localPosition.z;
            SetItemPosition(m_values[0].RectTransform, itemXAnchorBounds);
            m_values[0].ConfigureIndexIndicator(0);
            
            // Create remaining items
            for (int i = 1; i < valueCount; i++)
            {
                // Re-calculate anchor bounds
                itemXAnchorBounds.x = m_values[i - 1].RectTransform.anchorMax.x;
                itemXAnchorBounds.y = itemXAnchorBounds.x + itemWidthValue;
                // Duplicate the item
                GraphItemElement item = Instantiate(m_values[0].gameObject).GetComponent<GraphItemElement>();
                SetItemPosition(item.RectTransform, itemXAnchorBounds);
                item.ConfigureIndexIndicator(i);
                item.gameObject.name = $"IndividualGraphValue[{i}]";
                m_values.Add(item);
            }
        }

        public void SetItemPosition(RectTransform target, Vector2 anchorBounds)
        {
            // Set parent
            target.parent = m_parent;
            // Set anchor positions
            target.anchorMin = new Vector2(anchorBounds.x, 0.0f);
            target.anchorMax = new Vector2(anchorBounds.y, 1.0f);
            // Set default values
            target.localScale = defaultScale;
            target.offsetMin = defaultOffsetMin;
            target.offsetMax = defaultOffsetMax;
            target.localEulerAngles = defaultRotation;
            target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, defaultZPos);
        }

        [ContextMenu("Randomize Slider Values")]
        public void SetRandomSliderValue()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                m_values[i].SetValue(Random.Range(0.0f, 1.0f));
                /*
                if (m_values[i].GetChild(1).TryGetComponent<Slider>(out Slider slider))
                {
                    slider.value = Random.Range(0.0f, 1.0f);
                    CheckValueForHazard(m_values[i], slider);
                }
                */
            }
        }

        public void CheckValueForHazard(RectTransform target, Slider slider)
        {
            Image sliderFill = slider.fillRect.GetComponent<Image>();
            if (slider.value > 0.75f)
            {
                if (target.GetChild(0).childCount < 1)
                {
                    // Create hazard icon
                    RectTransform hazardIcon = Instantiate(m_hazardIconPrefab, target.GetChild(0)).GetComponent<RectTransform>();
                }
                sliderFill.color = m_hazardColor;
            }
            else
            {
                if (target.GetChild(0).childCount > 0)
                {
                    List<GameObject> children = new List<GameObject>();
                    for (int i = 0; i < target.GetChild(0).childCount; i++)
                    {
                        children.Add(target.GetChild(0).GetChild(i).gameObject);
                    }

                    for (int y = 0; y < children.Count; y++)
                    {
                        DestroyImmediate(children[y]);
                    }
                }

                sliderFill.color = m_normalColor;
            }
        }

        [ContextMenu("Delete Hazard Markers")]
        public void DeleteHazardMarkers()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                RectTransform target = m_values[i].RectTransform;
                if (target.GetChild(0).childCount > 0)
                {
                    List<GameObject> children = new List<GameObject>();
                    for (int x = 0; x < target.GetChild(0).childCount; x++)
                    {
                        children.Add(target.GetChild(0).GetChild(x).gameObject);
                    }

                    for (int y = 0; y < children.Count; y++)
                    {
                        DestroyImmediate(children[y]);
                    }
                }
            }
        }

        [ContextMenu("Reset Values")]
        public void ResetValues()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                m_values[i].SetValue(0.0f);
            }
        }
        #endif
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
