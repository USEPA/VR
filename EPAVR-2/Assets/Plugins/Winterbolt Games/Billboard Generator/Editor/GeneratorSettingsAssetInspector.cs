using UnityEditor;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime;

namespace WinterboltGames.BillboardGenerator.Editor
{
	[CustomEditor(typeof(GeneratorSettingsAsset))]
	[CanEditMultipleObjects]
	internal sealed class GeneratorSettingsAssetInspector : UnityEditor.Editor
	{
		public static void Draw(SerializedObject serializedObject)
		{
			SerializedProperty settingsProperty = serializedObject.FindProperty(nameof(GeneratorSettingsAsset.settings));

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.directions)));

			EditorGUILayout.HelpBox($"The maximum supported texture size by your system is {SystemInfo.maxTextureSize}", MessageType.Warning, true);

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.textureWidth)));

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.textureHeight)));

			SerializedProperty useMainCameraProperty = settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.useMainCamera));

			_ = EditorGUILayout.PropertyField(useMainCameraProperty);

			if (!useMainCameraProperty.boolValue)
			{
				_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.textureBackgroundColor)));

				SerializedProperty useOrthographicCameraProperty = settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.useOrthographicCamera));

				_ = EditorGUILayout.PropertyField(useOrthographicCameraProperty);

				_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.cameraSizeOrFieldOfView)), useOrthographicCameraProperty.boolValue ? new GUIContent("Orthographic Size") : new GUIContent("Field Of View"));

				_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.cameraClearFlags)));

				_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.cameraOffset)));

				if (GUILayout.Button("Copy Main Camera Position")) settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.cameraOffset)).vector3Value = Camera.main.transform.position;
			}
			else
			{
				EditorGUILayout.HelpBox("Some options are disabled when 'Use Main Camera' is enabled.", MessageType.Info, true);
			}

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.freeRectChoiceHeuristic)));

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.initialBinWidth)));

			_ = EditorGUILayout.PropertyField(settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.initialBinHeight)));

			SerializedProperty isolateTargetProperty = settingsProperty.FindPropertyRelative(nameof(GeneratorSettings.isolateTarget));

			if (isolateTargetProperty.boolValue && useMainCameraProperty.boolValue)
			{
				EditorGUILayout.HelpBox("'Use Main Camera' MUST be disabled when 'Isolate Target' is enabled otherwise the rendering is going to fail or give 100% undesirable results.", MessageType.Warning, true);
			}

			_ = EditorGUILayout.PropertyField(isolateTargetProperty);

			_ = serializedObject.ApplyModifiedProperties();
		}

		public override void OnInspectorGUI()
		{
			Draw(serializedObject);
		}
	}
}
