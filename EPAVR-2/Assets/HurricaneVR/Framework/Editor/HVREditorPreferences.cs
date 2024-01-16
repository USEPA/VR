using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Shared;
using UnityEditor;
using UnityEngine;

namespace HurricaneVR.Editor
{
    public class HVREditorPreferences : ScriptableObject
    {
        private static HVREditorPreferences instance;

        [SerializeField]
        private string _updatedDisplayedVersion;

        public static HVREditorPreferences Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<HVREditorPreferences>("HVREditorPreferences");
                    if (instance == null)
                    {
                        instance = CreateInstance<HVREditorPreferences>();
                        Save();
                    }
                }
                return instance;
            }
        }

        public static string UpdateDisplayedVersion
        {
            get { return Instance._updatedDisplayedVersion; }
            set
            {
                Instance._updatedDisplayedVersion = value;
                Save();
            }
        }

        public static void Save()
        {
            EditorUtility.SetDirty(instance);
            HVRSettings.Instance.AddAssetToResource(instance, "HVREditorPreferences");
        }
    }
}