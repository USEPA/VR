using UnityEditor;
using UnityEngine;
using WinterboltGames.BillboardGenerator.Runtime.Renderers;

namespace WinterboltGames.BillboardGenerator.Editor
{
	[CustomEditor(typeof(IndexBillboardRenderer))]
	[CanEditMultipleObjects]
	internal sealed class IndexBillboardRendererInspector : UnityEditor.Editor
	{
		private IndexBillboardRenderer _preview;

		private void OnEnable()
		{
			if (EditorApplication.isPlaying || !(target is IndexBillboardRenderer billboard) || !billboard.IsValid()) return;

			_preview = Instantiate(billboard);

			_preview.gameObject.hideFlags = HideFlags.HideAndDontSave;

			_preview.PrepareBillboard();

			EditorApplication.update += OnEditorApplicationUpdate;

			SceneView.duringSceneGui += OnDuringSceneGui;
		}

		private void OnDisable()
		{
			if (_preview != null) DestroyImmediate(_preview.gameObject);

			EditorApplication.update -= OnEditorApplicationUpdate;

			SceneView.duringSceneGui -= OnDuringSceneGui;
		}
		private void OnEditorApplicationUpdate()
		{
			if (_preview == null) return;

			_preview.transform.position = ((IndexBillboardRenderer)target).transform.position;

			_preview.UpdateBillboard();

			EditorApplication.QueuePlayerLoopUpdate();
		}

		private void OnDuringSceneGui(SceneView sceneView)
		{
			if (_preview == null) return;

			_preview.RenderBillboard();

			sceneView.Repaint();
		}

		public override void OnInspectorGUI()
		{
			SerializedProperty useBillboardTargetProperty = serializedObject.FindProperty("useBillboardTarget");

			if (useBillboardTargetProperty.boolValue)
			{
				EditorGUILayout.HelpBox("Billboard will automatically target the transform assigned to [BillboardTarget.Transform].", MessageType.Info, true);
			}
			else
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
			}

			_ = EditorGUILayout.PropertyField(useBillboardTargetProperty);

			SerializedProperty useAlternativeWorkflowProperty = serializedObject.FindProperty("useAlternativeWorkflow");

			_ = EditorGUILayout.PropertyField(useAlternativeWorkflowProperty);

			if (useAlternativeWorkflowProperty.boolValue)
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("meshFilter"));

				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("meshRenderer"));
			}
			else
			{
				_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
			}

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BillboardRendererBase.update)));

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BillboardRendererBase.render)));

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("atlas"));

			_ = EditorGUILayout.PropertyField(serializedObject.FindProperty("uvIndex"));

			_ = serializedObject.ApplyModifiedProperties();
		}
	}
}
