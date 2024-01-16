using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WinterboltGames.BillboardGenerator.Editor.Utilities;
using WinterboltGames.BillboardGenerator.Runtime;
using WinterboltGames.BillboardGenerator.Runtime.Extensions;
using WinterboltGames.BillboardGenerator.Runtime.Utilities;

namespace WinterboltGames.BillboardGenerator.Editor
{
	internal sealed class GeneratorWindow : EditorWindow
	{
		private Vector2 _scrollView;

		private readonly List<GeneratorWindowEntry> _entries = new List<GeneratorWindowEntry>();

		[MenuItem("Tools/Billboard Generator/Generator Window", priority = -10)]
		private static void OpenWindow()
		{
			GetWindow<GeneratorWindow>().titleContent = new GUIContent("Billboard Generator");
		}

		private void OnGUI()
		{
			DropBoxUtilities.GUILayoutDropBox("(Drag & Drop GameObjects Here)", 35.0f, (GameObject[] gameObjects) =>
			{
				foreach (GameObject gameObject in gameObjects)
				{
					_entries.Add(new GeneratorWindowEntry { GameObject = gameObject, });
				}
			});

			_scrollView = EditorGUILayout.BeginScrollView(_scrollView);

			for (int i = 0; i < _entries.Count; i++)
			{
				GeneratorWindowEntry entry = _entries[i];

				if (entry == null) continue;

				GUILayout.BeginHorizontal();

				entry.IsExpanded = GUILayout.Toggle(entry.IsExpanded, entry.GameObject == null ? "Missing" : entry.GameObject.name, EditorStyles.toolbarButton);

				entry.GameObject = DropBoxUtilities.DropBoxHandler(entry.GameObject, GUILayoutUtility.GetLastRect());

				GUI.color = Color.red;

				if (GUILayout.Button("Remove", EditorStyles.toolbarButton, GUILayout.MaxWidth(64.0f))) _entries.RemoveAt(i);

				GUI.color = Color.white;

				GUILayout.EndHorizontal();

				if (!entry.IsExpanded) continue;

				entry.SettingsAsset = (GeneratorSettingsAsset)EditorGUILayout.ObjectField("Settings Asset", entry.SettingsAsset, typeof(GeneratorSettingsAsset), false);

				if (entry.SettingsAsset == null) continue;

				if (entry.AreSettingsExpanded = EditorGUILayout.Foldout(entry.AreSettingsExpanded, "Settings")) GeneratorSettingsAssetInspector.Draw(new SerializedObject(entry.SettingsAsset));

				if (GUILayout.Button("Generate Billboard", GUILayout.Height(25.0f))) GenerateBillboard(entry.GameObject, entry.SettingsAsset.settings);

				if (GUILayout.Button("Capture Texture", GUILayout.Height(25.0f))) CaptureTexture(entry.GameObject, entry.SettingsAsset.settings);
			}

			EditorGUILayout.EndScrollView();
		}

		public static void GenerateBillboard(GameObject targetGameObject, in GeneratorSettings settings)
		{
			RawTransform targetTransform = new RawTransform(targetGameObject.transform);

			string defaultScenePath = string.Empty;

			if (settings.isolateTarget)
			{
				Scene currentScene = SceneManager.GetActiveScene();

				if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

				defaultScenePath = currentScene.path;

				Scene temporaryScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);

				foreach (Transform clone in Instantiate(targetGameObject).transform.GetTree())
				{
					clone.SetParent(null, true);

					SceneManager.MoveGameObjectToScene(clone.gameObject, temporaryScene);
				}

				EditorSceneManager.CloseScene(currentScene, true);

				if (EditorUtility.DisplayDialog("Confirm", "Bake Lighting?", "Bake Lighting", "Do Not Bake Lighting"))
				{
					_ = Lightmapping.Bake();
				}
			}

			try
			{
				Texture2D[] textures = Generator.Generate(targetTransform, settings);

				(Texture2D texture, List<UVCoordinates> uvs) = PackingUtilities.PackTight(textures, settings.freeRectChoiceHeuristic, settings.initialBinWidth, settings.initialBinHeight);

				AtlasUtilities.CreateAndSave(texture, uvs);
			}
			catch (Exception exception)
			{
				Debug.LogError($"Generation failed: {exception}");
			}
			finally
			{
				if (settings.isolateTarget)
				{
					EditorSceneManager.OpenScene(defaultScenePath, OpenSceneMode.Single);
				}
			}
		}

		public static void CaptureTexture(GameObject targetGameObject, in GeneratorSettings settings)
		{
			try
			{
				Generator.CaptureSingleShotAsync(new RawTransform(targetGameObject.transform), settings, EditorUtility.OpenFolderPanel("Choose Output Folder...", Application.dataPath, string.Empty));
			}
			catch (Exception exception)
			{
				Debug.LogError($"Capturing failed: {exception}");
			}
		}
	}
}
