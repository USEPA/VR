using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class TimedUIScaler : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] protected float m_desiredScale = 1.5f;
        [SerializeField] protected float m_transitionDuration = 0.5f;
        #endregion
        #region Protected Variables
        protected RectTransform m_rectTransform;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            TryGetComponent<RectTransform>(out m_rectTransform);
        }

        public void ScaleElementTransition()
        {
            ScaleElementTransition(m_desiredScale, m_transitionDuration);
        }

        public void ScaleElementTransition(float _scale, float _duration)
        {
            StartCoroutine(ScaleTransition(_scale, _duration));
        }

       

        public IEnumerator ScaleTransition(float _scale, float _duration)
        {
            // Set up variables
            float time = 0.0f;
            float startValue = 1.0f;
            float desiredScale = _scale;
            // Keep fading for the desired duration
            while (time < _duration)
            {
                // Set lerping values based on timer
                desiredScale = (time < (_duration / 2)) ? _scale : 1.0f;
                startValue = (time < (_duration / 2)) ? 1.0f : _scale;
                // Lerp the scale based on time
                float value = Mathf.Lerp(startValue, desiredScale, time / _duration);
                m_rectTransform.localScale = new Vector3(value, value, 1);
                // Iterate time
                time += Time.deltaTime;
                // Continue operation
                yield return null;
            }
            // Set the alpha value to the desired value
            m_rectTransform.localScale = Vector3.one;
        }
    }
}

