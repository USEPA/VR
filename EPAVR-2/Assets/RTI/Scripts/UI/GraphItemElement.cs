using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class GraphItemElement : MonoBehaviour
    {
        #region Public Variables
        public RectTransform m_rectTransform;
        public Slider m_slider;
        public Image m_sliderFill;
        public GameObject m_hazardIcon;
        public RectTransform m_indexIndicator;
        public int m_index = 0;
        #endregion
        #region Public Properties
        public RectTransform RectTransform { get => m_rectTransform; }
        public Slider Slider { get => m_slider; }

        public float Value { get => m_slider.value; }
        public int Index { get => m_index; set => m_index = value; }
        #endregion

        public void CacheReferences(RectTransform _rectTransform, Slider _slider, GameObject _hazardIcon)
        {
            m_rectTransform = _rectTransform;
            m_slider = _slider;
            m_sliderFill = m_slider.fillRect.GetComponent<Image>();
            m_hazardIcon = _hazardIcon;
            SetValue(0.0f);
        }


        public void SetValue(float value)
        {
            m_slider.value = value;
            if (value > 0.9f)
            {
                m_hazardIcon.SetActive(true);
                m_sliderFill.color = Color.red;
            }
            else
            {
                m_hazardIcon.SetActive(false);
                float colorValue = (43.0f / 255.0f);
                m_sliderFill.color = new Color(colorValue, colorValue, colorValue);
            }
        }

        public void SetValue(float value, int copyIndex)
        {
            SetValue(value);
            ConfigureIndexIndicator(copyIndex);
        }

        public void ConfigureIndexIndicator(int _newIndex)
        {
            if ((_newIndex+1) % 5 == 0)
            {
                m_indexIndicator.anchorMin = new Vector2(m_indexIndicator.anchorMin.x, 0.0f);
                m_indexIndicator.anchoredPosition = Vector3.zero;
            }
            else
            {
                m_indexIndicator.anchorMin = new Vector3(m_indexIndicator.anchorMin.x, (m_indexIndicator.anchorMax.y * 0.6f));
                m_indexIndicator.anchoredPosition = Vector3.zero;
            }
            Index = _newIndex;
        }
    }
}

