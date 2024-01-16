using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Shared;
using HurricaneVR.Framework.Shared.Utilities;
using UnityEditor;
using UnityEngine;

namespace HurricaneVR.Editor
{
    [InitializeOnLoad]
    public class HVREditorManager
    {
        private const string HurricaneVRUploader = "HurricaneVRUploader";
        public const string Version = "2.7";


        static HVREditorManager()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            EditorApplication.update -= CheckShowUpdate;
            EditorApplication.update += CheckShowUpdate;

            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            EditorApplication.update -= CheckShowUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (Application.productName != HurricaneVRUploader)
            {
                var kickit = HVRSettings.Instance;
            }
        }

        private static void CheckShowUpdate()
        {
            if (Application.productName != HurricaneVRUploader)
            {
                if (HVREditorPreferences.UpdateDisplayedVersion != Version)
                {
                    HVRSetupWindow.ShowWindow();
                }

                HVREditorPreferences.UpdateDisplayedVersion = Version;
            }

            EditorApplication.update -= CheckShowUpdate;
        }
    }
}