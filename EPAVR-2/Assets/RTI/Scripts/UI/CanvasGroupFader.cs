using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class CanvasGroupFader : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] CanvasGroup m_canvasGroup;
        #endregion
        #region Fade Functionality
        public void Fade(bool fadeOut, float duration)
        {
            // Get value from boolean
            float value = (fadeOut) ? 0.0f : 1.0f;
            // Start the fade coroutine
            StartCoroutine(HandleFade(value, duration));
        }

        public void FadeOut(float duration)
        {
            // Start the fade coroutine
            StartCoroutine(HandleFade(0.0f, duration));
        }

        public void FadeIn(float duration)
        {
            // Start the fade coroutine
            StartCoroutine(HandleFade(1.0f, duration));
        }

        public IEnumerator HandleFade(float value, float duration)
        {
            // Set up variables
            float startValue = m_canvasGroup.alpha;
            float time = 0.0f;
            // Keep fading for the desired duration
            while (time < duration)
            {
                // Lerp the alpha value of the canvas group
                m_canvasGroup.alpha = Mathf.Lerp(startValue, value, time / duration);
                // Iterate time
                time += Time.deltaTime;
                // Continue operation
                yield return null;
            }
            // Set the alpha value to the desired value
            m_canvasGroup.alpha = value;
        }
        #endregion

        #region Helper Methods
        public void SetAlpha(float value)
        {
            m_canvasGroup.alpha = value;
        }
        #endregion
    }
}

