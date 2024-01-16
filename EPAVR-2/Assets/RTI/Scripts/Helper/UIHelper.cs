using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class UIHelper
    {
        /// <summary>
        /// Forcibly resets a rect transform's anchors and zeros out any offsets
        /// </summary>
        /// <param name="_rectTransform"></param>
        public static void ForceExpandRectTransform(RectTransform _rectTransform)
        {
            _rectTransform.anchorMin = new Vector2(0, 0);
            _rectTransform.anchorMax = new Vector2(1, 1);
            _rectTransform.localPosition = Vector3.zero;
            _rectTransform.localEulerAngles = Vector3.zero;
            _rectTransform.localScale = Vector3.one;
        }
    }
}

