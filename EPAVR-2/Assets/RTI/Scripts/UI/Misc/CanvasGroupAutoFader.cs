using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupAutoFader : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [SerializeField] AnimationCurve m_fadeCurve;
        #endregion
        #region Private Variables
        CanvasGroup m_target;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            m_target = GetComponent<CanvasGroup>();
            m_fadeCurve.postWrapMode = WrapMode.Loop;
        }

        // Update is called once per frame
        void Update()
        {
            m_target.alpha = m_fadeCurve.Evaluate(Time.time);
        }
    }
}

