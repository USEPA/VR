using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HurricaneVR.Framework.Core.Player;

namespace L58.EPAVR
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : HVRScreenFade
    {
        #region Protected Variables
        protected CanvasGroup m_canvasGroup;
        #endregion
        #region Public Properties
        public static ScreenFader Instance { get; set; }
        public override float CurrentFade => m_canvasGroup.alpha;
        #endregion

        private void Awake()
        {
            // Set singleton
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);
            // Cache component
            m_canvasGroup = GetComponent<CanvasGroup>();
        }
        public override void UpdateFade(float alpha)
        {
            // Set alpha value of canvas group
            m_canvasGroup.alpha = alpha;
        }

        protected override void Enable()
        {
            //gameObject.SetActive(true);
        }

        protected override void Disable()
        {
            //gameObject.SetActive(false);
        }
    }
}

