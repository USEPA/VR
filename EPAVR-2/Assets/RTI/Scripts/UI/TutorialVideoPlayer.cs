using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

namespace L58.EPAVR 
{
    public class TutorialVideoPlayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Inspector Assigned Variables
        [Header("UI References")]
        [SerializeField] VideoPlayer m_videoPlayer = default;
        [SerializeField] Text m_currentTimeDisplay = default;
        [SerializeField] Text m_totalTimeDisplay = default;
        [SerializeField] Slider m_videoProgress = default;
        [SerializeField] Button m_playButton = default;
        [SerializeField] Image m_buttonImage = default;
        [Header("Button Icons")]
        [SerializeField] List<Sprite> m_videoControlIcons;
        #endregion
        #region Protected Variables
        protected CanvasGroupFader m_buttonFader;
        protected RectTransform m_sliderRect;

        protected bool resetFlag = false;
        protected bool m_pointerInBounds = false;
        #endregion
        #region Public Properties
        public bool Active { get; set; }
        public bool Complete { get; set; }
        #endregion

        public void Init()
        {
            // Cache components
            m_playButton.TryGetComponent<CanvasGroupFader>(out m_buttonFader);
            m_videoProgress.TryGetComponent<RectTransform>(out m_sliderRect);
            // Initialize button
            ConfigureButtonIcon(0);
        }

        // Update is called once per frame
        void Update()
        {
            // Update the video progress display if a clip is playing
            if (m_videoPlayer.isPlaying)
                UpdateProgressDisplay((float)m_videoPlayer.time);
            else if (Active)
                ResetDisplay();
            /*
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (!m_pointerInBounds) OnPointerEnter(VRUserManager.Instance.PointerEventData);
            }
            else
            {
                if (m_pointerInBounds) OnPointerExit(VRUserManager.Instance.PointerEventData);
            }
            */
        }

        void LateUpdate()
        {
            if (resetFlag)
            {
                Play();
                resetFlag = false;
            }
        }

        #region Video Player Functionality
        /// <summary>
        /// Assigns the video player's clip and resets progress display
        /// </summary>
        /// <param name="_newClip"></param>
        public void SetVideoClip(VideoClip _newClip)
        {
            // Reset display
            if (m_videoPlayer.isPlaying) m_videoPlayer.Stop();
            // Set reference
            m_videoPlayer.clip = _newClip;
            m_videoPlayer.time = 0.0f;
            m_videoPlayer.Play();
            m_videoPlayer.Pause();
            UpdateProgressDisplay(0.0f);
            m_buttonFader.SetAlpha(1.0f);
            ConfigureButtonIcon(0);
        }

        public void Play()
        {
            // Play the clip
            m_videoPlayer.Play();
            ConfigureButtonIcon(1);
            Active = true;
        }

        public void Pause()
        {
            // Pause the clip
            m_videoPlayer.Pause();
            ConfigureButtonIcon(0);

            Active = false;
        }

        public void JumpToTime(float value)
        {
            if (m_videoPlayer.clip == null) return;
            // Interpret time from value
            float time = (float)m_videoPlayer.clip.length * value;
            UnityEngine.Debug.Log($"Jumped to time: {time} || Time: {Time.time}");
            m_videoPlayer.time = time;
            // Update display
            UpdateProgressDisplay(time);
            if (!Active) ConfigureButtonIcon(0);
        }
        #endregion

        #region UI Functionality
        /// <summary>
        /// Plays/pauses clip depending on current video player state
        /// </summary>
        public void OnButtonPress()
        {
            // Make sure there is a valid clip
            if (m_videoPlayer.clip == null) return;
            // Play/pause clip depending on video player state
            if (m_videoPlayer.isPlaying)
                Pause();
            else
                Play();
        }

        /// <summary>
        /// Updates the time displays of video as well as the progress slider based on current clip
        /// </summary>
        public void UpdateProgressDisplay(float _time)
        {
            m_currentTimeDisplay.text = SecondsToTimeString(_time);
            m_totalTimeDisplay.text = SecondsToTimeString((float)m_videoPlayer.clip.length);
            m_videoProgress.value = Mathf.Clamp01((float)_time / (float)m_videoPlayer.clip.length);
        }

        public void ResetDisplay()
        {
            if (m_videoPlayer.time > 0)
            {
                ConfigureButtonIcon(2);
                m_buttonFader.SetAlpha(1.0f);
                Active = false;
            }
        }
        /// <summary>
        /// Updates button icon to be play/pause sprite depending on video player state
        /// </summary>
        public void ConfigureButtonIcon(bool isPlaying)
        {
            if (isPlaying)
                m_buttonImage.sprite = m_videoControlIcons[0];
            else
                m_buttonImage.sprite = m_videoControlIcons[1];
        }

        public void ConfigureButtonIcon(int _index)
        {
            // Make sure index is valid
            if (_index < 0 || _index >= m_videoControlIcons.Count) return;
            // Set button icon appropriately
            m_buttonImage.sprite = m_videoControlIcons[_index];
        }

        public void OnSliderClick()
        {
            // Get mouse position
            Vector2 mousePos = SimulationManager.Instance.InputRefs.GetAction(XRActionMap.UI, XRAction.Position).action.ReadValue<Vector2>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_sliderRect, mousePos, Camera.main, out Vector2 rectPos);
            float value = rectPos.x / m_sliderRect.rect.width;
            // Jump to time
            JumpToTime(value);
            UnityEngine.Debug.Log($"Slider was clicked at: {rectPos} | Rectangle Width: {m_sliderRect.rect.width} | Value: {value} || Time: {Time.time}");
        }
        #endregion

        #region Helper Methods
        public string SecondsToTimeString(float seconds, string format = "mm':'ss")
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(format);
        }
        #endregion

        #region Hover Functionality
        public void OnPointerEnter(PointerEventData eventData)
        {
           // UnityEngine.Debug.Log($"OnPointerEnter || Time: {Time.time}");
            if (m_videoPlayer.isPlaying) 
            {
                m_buttonFader.FadeIn(0.15f);
            }
            m_pointerInBounds = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_videoPlayer.isPlaying)
            {
                m_buttonFader.FadeOut(0.15f);
            }
            m_pointerInBounds = false;
            //UnityEngine.Debug.Log($"OnPointerExit || Time: {Time.time}");
        }
        #endregion

        #region Render Texture Image Functionality
        public void ClearRenderTexture()
        {
            m_videoPlayer.targetTexture.Release();
        }
        #endregion
        void OnDestroy()
        {
            ClearRenderTexture();
        }
    }
}


