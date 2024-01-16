using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class ForceUpdateChildWidth : MonoBehaviour
    {
        #region Private Variables
        private RectTransform _rectTransform, _parentRectTransform;
        private VerticalLayoutGroup verticalLayoutGroup;
        #endregion

        void OnEnable()
        {
            UpdateWidth();
        }

        private void UpdateWidth()
        {
            if (verticalLayoutGroup == null || _rectTransform == null || _parentRectTransform == null)
            {
                verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
                if (verticalLayoutGroup != null)
                {
                    _parentRectTransform = verticalLayoutGroup.GetComponent<RectTransform>();
                    _rectTransform = GetComponent<RectTransform>();
                    _rectTransform.pivot = new Vector2(0, 1);
                    _rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
                }
            }
            else
            {
                _rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
            }
        }
    }
}

