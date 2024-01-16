using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace HurricaneVR.Framework.Core.Player
{
    public class HVRCameraRig : MonoBehaviour
    {
        public const string HeightKey = "SaveHVRHeight";

        [Header("Required Transforms")]
        public Transform Camera;
        public Transform FloorOffset;
        public Transform CameraScale;

        [Header("Manual Camera Offsetting")]
        [Tooltip("Manually modify the camera height if needed")]
        public float CameraYOffset;

        [FormerlySerializedAs("PlayerHeight")]
        [Tooltip("Height of the virtual player")]
        public float EyeHeight = 1.66f;

        [Tooltip("Sitting or standing mode")]
        public HVRSitStand SitStanding = HVRSitStand.PlayerHeight;

        [Header("Debugging")]

        [Tooltip("If true, use up and down arrow to change YOffset to help with testing.")]
        public bool DebugKeyboardOffset;
        public bool DebugKeyboardRecalibrate;

        public HVRDebugCalibrate DebugCalibMode = HVRDebugCalibrate.None;

        public float DebugCalibMovedThreshold = .05f;

#if ENABLE_LEGACY_INPUT_MANAGER

        public KeyCode RecalibrateKey = KeyCode.R;

#elif ENABLE_INPUT_SYSTEM

        public Key HeightCalibrateKey = Key.R;

#endif


        [Tooltip("Calibration height is saved to player prefs when height is calibrated.")]
        public bool SaveCalibrationHeight;

        [Header("For Debugging Display only")]
        public float PlayerControllerYOffset = 0f;
        public float AdjustedCameraHeight;
        public float SittingOffset;


        public bool IsMine { get; set; } = true;

        private Vector3 _cameraStartingPosition;

        protected virtual void Start()
        {
            Setup();

            if (DebugCalibMode == HVRDebugCalibrate.HMDMoved)
            {
                StartCoroutine(CalibrateOnHMDMoved());
            }
            else if (DebugCalibMode == HVRDebugCalibrate.Immediately)
            {
                DebugCalibrate();
            }
        }

        private void DebugCalibrate()
        {
            if (SaveCalibrationHeight)
            {
                CalibrateFromSaved();
            }
            else
            {
                Calibrate();
            }
        }

        protected virtual void CalibrateFromSaved()
        {
            if (PlayerPrefs.HasKey(HeightKey))
            {
                var height = PlayerPrefs.GetFloat(HeightKey);
                CalibrateHeight(height);
            }
            else
            {
                Calibrate();
            }
        }

        private IEnumerator CalibrateOnHMDMoved()
        {
            yield return null;
            _cameraStartingPosition = Camera.localPosition;

            while (Vector3.Distance(_cameraStartingPosition, Camera.transform.localPosition) < DebugCalibMovedThreshold)
            {
                yield return null;
            }

            Debug.Log($"Camera movement detected, calibrating height.");
            DebugCalibrate();
        }

        void Update()
        {
            if (FloorOffset)
            {
                var pos = FloorOffset.transform.localPosition;
                var intendedOffset = SittingOffset + CameraYOffset + PlayerControllerYOffset;
                FloorOffset.transform.localPosition = new Vector3(pos.x, intendedOffset, pos.z);
            }

            AdjustedCameraHeight = FloorOffset.transform.localPosition.y + Camera.localPosition.y * _scale;

            if (IsMine)
            {
#if ENABLE_LEGACY_INPUT_MANAGER

                if (DebugKeyboardOffset && UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                {
                    CameraYOffset += -.25f;
                }
                else if (DebugKeyboardOffset && UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    CameraYOffset += .25f;
                }
#elif ENABLE_INPUT_SYSTEM

                if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
                {
                    CameraYOffset += .25f;
                }
                else if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
                {
                    CameraYOffset += -.25f;
                }
#endif

                if (DebugKeyboardRecalibrate)
                {

#if ENABLE_LEGACY_INPUT_MANAGER
                    if (Input.GetKeyDown(RecalibrateKey))
                    {
                        Calibrate();
                    }
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current[HeightCalibrateKey].wasPressedThisFrame)
            {
               Calibrate();
            }
#endif

                }
            }
        }

        private void Setup()
        {
            var offset = CameraYOffset;

            if (FloorOffset)
            {
                var pos = FloorOffset.transform.localPosition;
                FloorOffset.transform.localPosition = new Vector3(pos.x, offset, pos.z);
            }
        }

        private float _scale = 1f;
        public void CalibrateHeight(float height)
        {
            //CalibratedHeight = height;

            if (SitStanding == HVRSitStand.Standing)
            {
                if (height < .01f)
                {
                    height = EyeHeight;
                }

                SittingOffset = 0f;
                _scale = EyeHeight / height;
            }
            else if (SitStanding == HVRSitStand.Sitting)
            {
                SittingOffset = EyeHeight - height;
                _scale = 1f;
            }
            else if (SitStanding == HVRSitStand.PlayerHeight)
            {
                SittingOffset = 0f;
                _scale = 1f;
            }

            if (CameraScale)
            {
                CameraScale.localScale = new Vector3(_scale, _scale, _scale);
            }
        }



        public void Calibrate()
        {
            float height = Camera.localPosition.y + CameraYOffset;
            CalibrateHeight(Camera.localPosition.y + CameraYOffset);
            UnityEngine.Debug.Log($"{gameObject.name} calibrating height | Camera Local Pos Y: {Camera.localPosition.y} | Camera Y Offset: {CameraYOffset} | Height: {ConvertMetersToFeet(height)} ft ({height} m) || Time: {Time.time}");
            if (SaveCalibrationHeight)
            {
                PlayerPrefs.SetFloat(HeightKey, Camera.localPosition.y);
                PlayerPrefs.Save();
            }
        }

        public void SetSitStandMode(HVRSitStand sitStand)
        {
            if (sitStand == HVRSitStand.Standing && !CameraScale)
            {
                Debug.LogWarning($"Standing mode cannot be set without the CameraScale transform assigned and setup properly.");
                sitStand = HVRSitStand.PlayerHeight;
            }

            SitStanding = sitStand;
            Calibrate();
        }

        public float ConvertMetersToFeet(float _value)
        {
            return _value * 3.2808399f;
        }

    }

    public enum HVRSitStand
    {
        Sitting,
        Standing,
        PlayerHeight
    }

    public enum HVRDebugCalibrate
    {
        None, HMDMoved, Immediately
    }
}