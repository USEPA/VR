using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace L58.EPAVR
{
    public class TutorialTabletGUIState : DeviceGUIState
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] private TutorialVideoPlayer m_videoPlayer;
        [Header("Default Configuration")]
        [SerializeField] private VideoClip m_defaultVideo;
        #endregion
        #region Public Properties
        #endregion

        #region Initialization
        public override void Init()
        {
            // Call base functionality
            base.Init();
            // Initialize the video player
            m_videoPlayer.Init();
        }
        #endregion

        #region State-Related Functionality
        public override void OnEnter()
        {
            // Call base functionality
            base.OnEnter();
            // Load the default video
            m_videoPlayer.SetVideoClip(m_defaultVideo);
        }
        #endregion
    }
}

